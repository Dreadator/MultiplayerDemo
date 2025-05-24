using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int,int, PlayerType> OnClickedOnGridPosition;
    public event Action OnPlayerTurnChange;
    public event Action OnGameStarted;
    public event Action<Line, PlayerType> OnGameWin;
    public event Action OnRematch;
    public event Action OnGameTied;
    public event Action OnScoreChanged;
    public event Action OnPlacedObject;

    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

    public PlayerType localPlayerType { get; private set; }
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            // Horizontal Lines
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1, 0) , new Vector2Int(2, 0) },
                centerGridPosition = new Vector2Int(1,0),
                orientation = Orientation.Horizontal,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,1), new Vector2Int(1, 1) , new Vector2Int(2, 1) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Horizontal,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2), new Vector2Int(1, 2) , new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(1,2),
                orientation = Orientation.Horizontal,
            },

            //Vertical Lines
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(0, 1) , new Vector2Int(0, 2) },
                centerGridPosition = new Vector2Int(0,1),
                orientation = Orientation.Vertical,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(1,0), new Vector2Int(1, 1) , new Vector2Int(1, 2) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Vertical,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(2,0), new Vector2Int(2, 1) , new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical,
            },

            // Diagonal Lines
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1, 1) , new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA,

            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 2), new Vector2Int(1, 1) , new Vector2Int(2, 0) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalB,
            },
        };
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        OnClientConnectCallback(2);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (NetworkManager.Singleton.LocalClientId == 0)
            localPlayerType = PlayerType.Cross;
        else
            localPlayerType = PlayerType.Circle;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnPlayerTurnChange?.Invoke();
        };

        playerCrossScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke();
        };

        playerCircleScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke();
        };
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectCallback;
        }
    }
    private void OnClientConnectCallback(ulong obj) 
    {
        if (!IsServer) return;
  
        if(NetworkManager.Singleton.ConnectedClientsList.Count == 2) 
        {
            Debug.Log("Right number of clients, triggering start game");
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRPC();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRPC() 
    {
       OnGameStarted?.Invoke();
       Debug.Log("Trigger start rpc");
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRPC(int x, int y, PlayerType playerType) 
    {
        Debug.Log($"ClickedOnGridPosition: {x}, {y}");

        if (playerType != currentPlayablePlayerType.Value) 
            return;

        if (playerTypeArray[x, y] != PlayerType.None) 
            return;

        playerTypeArray[x, y] = playerType;

        OnClickedOnGridPosition?.Invoke(x,y, playerType);

        TriggerOnPlacedObjectRPC();

        switch (currentPlayablePlayerType.Value) 
        {
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }
        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlacedObjectRPC()
    {
        OnPlacedObject?.Invoke();
    }

    public PlayerType GetLocalPlayerType() 
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine( playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
                           playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
                           playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]);
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType) 
    {
        return
            aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType;
    }

    private void TestWinner() 
    {
        for (int i = 0; i < lineList.Count; i++) 
        {
            if (TestWinnerLine(lineList[i]))
            {
                PlayerType winPlayerType = playerTypeArray[lineList[i].centerGridPosition.x, lineList[i].centerGridPosition.y];
                TriggerOnGameWinRPC(i, winPlayerType);
                currentPlayablePlayerType.Value = PlayerType.None;

                switch (winPlayerType) 
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;

                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;
                }
                return;
            }
        }

        bool hasTie = true;

        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if(playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;
                }
            }
        }

        if (hasTie) 
            TriggerOnGameTiedRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRPC() 
    {
        OnGameTied?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRPC(int lineIndex, PlayerType playerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(line, playerType);
    }

    [Rpc(SendTo.Server)]
    public void RematchRPC() 
    {
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int  y= 0; y < playerTypeArray.GetLength(1); y++) 
            {
                playerTypeArray[x,y] = PlayerType.None;
            }
        }
        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRPC() 
    {
        OnRematch?.Invoke();
    }

    public void GetScores(out int playerCrossScore, out int playerCircleScore) 
    {
        playerCrossScore = this.playerCrossScore.Value;
        playerCircleScore = this.playerCircleScore.Value;
    }
}

public struct Line 
{
    public List<Vector2Int> gridVector2IntList;
    public Vector2Int centerGridPosition;
    public Orientation orientation;
}

public enum PlayerType
{
    None,
    Cross,
    Circle,
}

public enum Orientation 
{
    Horizontal, 
    Vertical, 
    DiagonalA,
    DiagonalB
}
