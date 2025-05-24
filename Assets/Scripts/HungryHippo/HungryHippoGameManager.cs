using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HungryHippoGameManager : NetworkBehaviour
{
    public static HungryHippoGameManager Instance { get; private set; }

    public event Action OnGameStarted;
    public event Action OnGameEnded;
    public event Action OnRoundEnded;
    public event Action OnRemainingBallsChanged;

    [SerializeField] NetworkBallSpawner ballSpanwer;

    private NetworkVariable<int> remainingBalls = new NetworkVariable<int>();

    private Dictionary<ulong, int> playerProgress = new();

    private bool gameStarted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
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
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectCallback;
        }

        remainingBalls.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnRemainingBallsChanged?.Invoke();
        };
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
        Debug.Log($"Remaining balls = {remainingBalls.Value}");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRPC() 
    {
        OnGameStarted?.Invoke();
        Debug.Log("GameManager - Game Started");
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
        OnRoundEnded?.Invoke();
        Debug.Log("GameManager - Round Ended");
        StartCoroutine (DelayedStartRound());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameEndedRPC() 
    {
        OnGameEnded?.Invoke();
        Debug.Log("GameManager - Game Ended");
    }

    [Rpc(SendTo.Server)]
    public void PlayerCollectedBallRPC() 
    {
        ulong client = 1;
        if (!playerProgress.ContainsKey(client))
            playerProgress[client] = 1;
        else 
        {
            playerProgress[client]++;

            Debug.Log($"Player client : {client} has progress of {playerProgress[client]}");
        }

        remainingBalls.Value--;
        Debug.Log($"player collected,Remaining balls = {remainingBalls.Value}");
        if (remainingBalls.Value == 0)
            TriggerOnRoundEndedRPC();
    }

    private void SpawnBalls() 
    {
        if(IsServer)
            ballSpanwer.SpawnBalls();
    }


    #region Debug
    [ContextMenu("Start Game Local")]
    private void StartGameLocal()
    {
        TriggerOnGameStartedRPC();
    }
    #endregion
}