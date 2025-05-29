using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    public event Action OnGameStarted;
    public event Action OnRemainingBallsChanged;
    public event Action OnRoundEnded;
    public event Action OnGameRestarted;
    public event Action<ulong> OnRoundWinnerAnnounced;
    public event Action<int> OnRoundIncreased;
    public event Action<Dictionary<ulong, int>, Dictionary<ulong, int>> OnGameEnded;

    [SerializeField] NetworkBallSpawner ballSpanwer;
    [SerializeField] int maxRounds = 5;

    private NetworkVariable<int> roundIndex = new NetworkVariable<int>(1);
    private NetworkVariable<int> remainingBalls = new NetworkVariable<int>();

    private Dictionary<ulong, int> roundProgress = new();
    private Dictionary<ulong, int> roundWinProgress = new();
    private Dictionary<ulong, int> clientsTotalBallsCollected = new();

    private bool gameStarted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        roundProgress = new Dictionary<ulong, int>();
        roundWinProgress = new Dictionary<ulong, int>();
        clientsTotalBallsCollected = new Dictionary<ulong, int>();
    }

    private void Start()
    {
        ballSpanwer.OnAllBallsSpawned += BallSpawner_OnAllBallsSpawned;
        NetworkManager_OnClientConnectCallback(0);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectCallback;

        remainingBalls.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnRemainingBallsChanged?.Invoke();
        };

        roundIndex.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnRoundIncreased?.Invoke(roundIndex.Value);
        };
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnectCallback;

        base.OnNetworkDespawn();
    }

    private void NetworkManager_OnClientConnectCallback(ulong obj)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count ==
            LobbyManager.Instance.GetJoinedLobbyPlayerCount())
        {
            TriggerOnGameStartedRPC();
        }
    }

    private void BallSpawner_OnAllBallsSpawned(int ballCount)
    {
        if (!IsServer) return;

        remainingBalls.Value = ballCount;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRPC()
    {
        Debug.Log("GameManager - Game Started");
        OnGameStarted?.Invoke();
        StartCoroutine(DelayedStartRound());
    }

    private IEnumerator DelayedStartRound()
    {
        Debug.Log("Starting Round in 3s");
        yield return new WaitForSeconds(3f);
        SpawnBalls();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRoundEndedRPC()
    {
        Debug.Log("GameManager - Round Ended");
        IncreaseRoundIndex();
        OnRoundEnded?.Invoke();

        if (roundIndex.Value > maxRounds) return;

        StartCoroutine(DelayedStartRound());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameEndedRPC()
    {
        Debug.Log("GameManager - Game Ended");
        OnGameEnded?.Invoke(roundWinProgress, clientsTotalBallsCollected);
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void PlayerCollectedBallServerRPC(ulong clientId)
    {
        UpdatePlayersProgressClientRPC(clientId);

        remainingBalls.Value--;

        if (remainingBalls.Value == 0)
        {
            if (roundIndex.Value >= maxRounds)
            {
                CheckRoundWinner();
                StartCoroutine(DelayedRoundEnd());
                return;
            }

            TriggerOnRoundEndedRPC();
            CheckRoundWinner();
        }
    }

    private IEnumerator DelayedRoundEnd()
    {
        yield return new WaitForSeconds(3f);
        TriggerOnGameEndedRPC();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void UpdatePlayersProgressClientRPC(ulong clientId)
    {
        if (!roundProgress.ContainsKey(clientId))
            roundProgress[clientId] = 1;
        else
            roundProgress[clientId]++;

        if (!clientsTotalBallsCollected.ContainsKey(clientId))
            clientsTotalBallsCollected[clientId] = 1;
        else
            clientsTotalBallsCollected[clientId]++;
    }

    private void IncreaseRoundIndex()
    {
        if (!IsServer) return;

        roundIndex.Value++;
        if (roundIndex.Value > maxRounds)
            roundIndex.Value = maxRounds;
    }

    private void SpawnBalls()
    {
        if (!IsServer) return;
        ballSpanwer.SpawnBalls();
    }

    private void CheckRoundWinner()
    {
        int highestScore = roundProgress.Values.Max();
        var entryWithHighestScore = roundProgress.First(kvp => kvp.Value == highestScore);
        ulong clientWithHighestScore = entryWithHighestScore.Key;

        UpdateRoundProgressClientRPC(clientWithHighestScore);

        roundProgress.Clear();

        TriggerAnounceRoundWinnerClientRPC(clientWithHighestScore);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateRoundProgressClientRPC(ulong clientID)
    {
        if (!roundWinProgress.ContainsKey(clientID))
            roundWinProgress[clientID] = 1;
        else
            roundWinProgress[clientID]++;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerAnounceRoundWinnerClientRPC(ulong clientID)
    {
        OnRoundWinnerAnnounced?.Invoke(clientID);
    }

    [Rpc(SendTo.Server)]
    public void RestartGameServerRPC()
    {
        if (!IsServer) return;

        roundIndex.Value = 1;

        RestartGameClientRPC();
        TriggerOnGameStartedRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RestartGameClientRPC()
    {
        roundProgress.Clear();
        roundWinProgress.Clear();
        clientsTotalBallsCollected.Clear();

        OnGameRestarted?.Invoke();
    }
}