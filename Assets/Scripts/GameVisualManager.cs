using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGOList;

    private void Awake()
    {
        visualGOList = new List<GameObject>();
    }

    private void Start()
    {
        if(GameManager.Instance != null) 
        {
            GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
            GameManager.Instance.OnGameWin += OnGameWin;
            GameManager.Instance.OnRematch += OnGameRematch;
        }     
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClickedOnGridPosition -= GameManager_OnClickedOnGridPosition;
            GameManager.Instance.OnGameWin -= OnGameWin;
        }
    }

    private void GameManager_OnClickedOnGridPosition(int x, int y, PlayerType playerType) 
    {
        Debug.Log("OnClickedOnGridPosition");
        SpawnObjectRPC(x, y, playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRPC(int x, int y, PlayerType playerType) 
    {
        Debug.Log("SPawn Object");

        Transform prefabToSpawn = null;

        switch (playerType) 
        {
            case PlayerType.Cross:
                prefabToSpawn = crossPrefab;
                break;
            
            case PlayerType.Circle:
                prefabToSpawn = circlePrefab;
                break;
        }

        Transform spawnGO = Instantiate(prefabToSpawn, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnGO.GetComponent<NetworkObject>().Spawn(true);

        visualGOList.Add(spawnGO.gameObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y) 
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }

    private void OnGameWin(Line line, PlayerType playerType) 
    {
        if (!NetworkManager.Singleton.IsServer) return;

        float eulerZ = 0f;

        switch (line.orientation) 
        {
            case Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case Orientation.Vertical:
                eulerZ = 90f;
                break;
            case Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }

        Transform winLine = 
            Instantiate(
                lineCompletePrefab, 
                GetGridWorldPosition(line.centerGridPosition.x, line.centerGridPosition.y),
                Quaternion.Euler(0, 0 , eulerZ));
        
        winLine.GetComponent<NetworkObject>().Spawn(true);

        visualGOList.Add(winLine.gameObject);
    }

    private void OnGameRematch() 
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach(GameObject visual in visualGOList) 
        {
            Destroy(visual);
        }
        visualGOList.Clear();
    }
}

