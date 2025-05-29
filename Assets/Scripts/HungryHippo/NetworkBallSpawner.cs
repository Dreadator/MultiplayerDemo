using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class NetworkBallSpawner : NetworkBehaviour
{
    [SerializeField] GameObject ballPrefab;

    [SerializeField] List<Transform> ballSpawnPoints;

    [SerializeField] int startBallCount = 10;
    [SerializeField] int numberOfAdditionalBallsPerSpawn = 10;

    private int currentBallCount;

    public event Action<int> OnAllBallsSpawned;

    private void Awake()
    {
        currentBallCount = startBallCount;      
    }

    private void Start()
    {
        if(HungryHippoGameManager.Instance)
            HungryHippoGameManager.Instance.OnGameRestarted += ResetBallCount;
    }

    public override void OnDestroy()
    {
        if (HungryHippoGameManager.Instance)
            HungryHippoGameManager.Instance.OnGameRestarted -= ResetBallCount;

        base.OnDestroy();
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
        currentBallCount += numberOfAdditionalBallsPerSpawn;
    }

    private void ResetBallCount() =>
        currentBallCount = startBallCount;
}
