using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class NetworkBallSpawner : NetworkBehaviour
{
    [SerializeField] GameObject ballPrefab;

    [SerializeField] List<Transform> ballSpawnPoints;

    [SerializeField] int startBallCount = 10;

    private int currentBallCount;

    public event Action<int> OnAllBallsSpawned;

    private void Awake()
    {
        currentBallCount = startBallCount;      
    }

    public void SpawnBalls() 
    {
        if (!IsServer) return;

        int rand = 0;
        for (int i = 0; i < currentBallCount; ++i) 
        {
            rand = UnityEngine.Random.Range(0, ballSpawnPoints.Count);
            NetworkObject ballNO = NetworkObjectPool.Instance.GetFromPool(ballSpawnPoints[rand].position, Quaternion.identity);
            ballNO.Spawn();
        }
        OnAllBallsSpawned?.Invoke(currentBallCount);
        currentBallCount += 10;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) 
        {
            if (IsServer)
                SpawnBalls();
        }
    }
}
