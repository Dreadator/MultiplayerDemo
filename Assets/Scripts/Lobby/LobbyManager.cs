using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_CHARACTER = "Character";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_JOIN_CODE = "RelayJoinCode";

    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public enum GameMode
    {
        FeedingFrenzy,
        BattleHippos
    }

    public enum PlayerCharacter
    {
        Snake,
        Hippo,
        Crocodile,
        Rhino
    }

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;
    private Lobby joinedLobby;
    private string playerName;

    private bool gameStarted;

    private void Update()
    {
        if (!gameStarted)
        {
            HandleRefreshLobbyList();
            HandleLobbyHeartbeat();
            HandleLobbyPolling();
        }
    }

    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
            RefreshLobbyList();
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f)
            {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;

                RefreshLobbyList();
            }
        }
    }
    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public int GetJoinedLobbyPlayerCount()
    {
        return joinedLobby.Players.Count;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerCharacter.Snake.ToString()) }
        });
    }

    public void ChangeGameMode()
    {
        if (IsLobbyHost())
        {
            GameMode gameMode =
                Enum.Parse<GameMode>(joinedLobby.Data[KEY_GAME_MODE].Value);

            switch (gameMode)
            {
                default:
                case GameMode.FeedingFrenzy:
                    gameMode = GameMode.BattleHippos;
                    break;
                case GameMode.BattleHippos:
                    gameMode = GameMode.FeedingFrenzy;
                    break;
            }
            UpdateLobbyGameMode(gameMode);
        }
    }

    public void ToggleInGame()
    {
        gameStarted = !gameStarted;
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, GameMode gameMode)
    {
        Allocation allocation = await RelayManager.Instance.AllocateRelay();
        string relayJoinCode = await RelayManager.Instance.GetRelayJoinCode(allocation);

        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) },
                { KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
            }
        };

        joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

        Debug.Log("Created Lobby " + joinedLobby.Name);

        RelayManager.Instance.SetTransportAndStartHost(allocation);
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error refreshing lobby list :" + e.Message);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Player player = GetPlayer();

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
            {
                Player = player
            });

            joinedLobby = lobby;
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            string relayJoinCode = lobby.Data[KEY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await RelayManager.Instance.JoinRelay(relayJoinCode);

            RelayManager.Instance.SetTransportAndStartClient(joinAllocation);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error joining lobby by code :" + e.Message);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            Player player = GetPlayer();

            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            string relayJoinCode = lobby.Data[KEY_JOIN_CODE].Value;
            Debug.Log("Relay join code : " + relayJoinCode);
            JoinAllocation joinAllocation = await RelayManager.Instance.JoinRelay(relayJoinCode);

            RelayManager.Instance.SetTransportAndStartClient(joinAllocation);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"Join lobby error : {e.Message}");
            throw;
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            options.Player = GetPlayer();

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            string relayJoinCode = lobby.Data[KEY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await RelayManager.Instance.JoinRelay(relayJoinCode);

            RelayManager.Instance.SetTransportAndStartClient(joinAllocation);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"Quick join lobby error: {e.Message}");
        }
    }
    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"Leave lobby error: {e.Message}");
            }
        }
    }
    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log($"kicking player: {playerId}");
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"kick player error: {e.Message}");
            }
        }
    }

    public async void UpdatePlayerName(string playerName)
    {
        this.playerName = playerName;

        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new()
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        {
                            KEY_PLAYER_NAME, new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: playerName)
                        }
                    }
                };
                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"Update player name error: {e.Message}");
            }
        }
    }


    public async void UpdatePlayerCharacter(PlayerCharacter playerCharacter)
    {
        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                        KEY_PLAYER_CHARACTER, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerCharacter.ToString())
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"Update player character error: {e.Message}");
            }
        }
    }


    public async void UpdateLobbyGameMode(GameMode gameMode)
    {
        try
        {
            Debug.Log($"UpdateLobbyGameMode {gameMode}");

            Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
                }
            });

            joinedLobby = lobby;

            OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"Update lobby game mode error: {e.Message}");
        }
    }

    public void DeleteJoinedLobby() 
    {
        _ = LobbyService.Instance.DeleteLobbyAsync(GetJoinedLobby().Id); 
    }

    public void HandleLobbyExit() 
    {
        joinedLobby = null;
        ToggleInGame();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            try
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer < 0f)
                {
                    float heartbeatTimerMax = 15f;
                    heartbeatTimer = heartbeatTimerMax;

                    Debug.Log("Heartbeat");
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"Lobby heartbeat error: {e.Message}");
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null)
        {
            try
            {
                lobbyPollTimer -= Time.deltaTime;
                if (lobbyPollTimer < 0f)
                {
                    float lobbyPollTimerMax = 2.1f;
                    lobbyPollTimer = lobbyPollTimerMax;

                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                    OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    if (!IsPlayerInLobby())
                    {
                        // Player was kicked out of this lobby
                        Debug.Log("Kicked from Lobby!");

                        OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                        joinedLobby = null;
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"Lobby polling error: {e.Message}");
            }
        }
    }
}