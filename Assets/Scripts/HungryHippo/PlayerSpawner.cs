using System;
using Unity.Netcode;
using UnityEngine;


public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [SerializeField] GameObject[] playerPrefabs;
    [SerializeField] Transform[] spawnPoints;

    private NetworkVariable<int> spawnIndex = new();

    public event Action OnPlayerSpawned;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    private void Start()
    {
        SpawnPlayerRPC(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerRPC(ulong clientId) 
    {
        NetworkObject playerNO = Instantiate(
            playerPrefabs[spawnIndex.Value],
            spawnPoints[spawnIndex.Value].position,
            spawnPoints[spawnIndex.Value].rotation).GetComponent<NetworkObject>();

        playerNO.SpawnAsPlayerObject(clientId);
        spawnIndex.Value++;
        TriggerPlayerSpawnedClientRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerPlayerSpawnedClientRPC() 
    {
        OnPlayerSpawned?.Invoke();
    }

    [ContextMenu("Debug Spawn Player")]
    private void SpawnPlayerDebug()
    {
        SpawnPlayerRPC(NetworkManager.Singleton.LocalClientId);
    }
}
