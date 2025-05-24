using System;
using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGO;
    [SerializeField] private GameObject circleArromGO;
    [SerializeField] private GameObject CrossYouTextGO;
    [SerializeField] private GameObject CircleYouTextGO;

    [SerializeField] private TextMeshProUGUI CrossScoreTMP;
    [SerializeField] private TextMeshProUGUI CircleScoreTMP;

    private void Awake()
    {
        crossArrowGO.SetActive(false);
        circleArromGO.SetActive(false); 
        CrossYouTextGO.SetActive(false);    
        CircleYouTextGO.SetActive(false);   

        CrossScoreTMP.text = string.Empty;
        CircleScoreTMP.text = string.Empty;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += OnGameStarted;
        GameManager.Instance.OnPlayerTurnChange += OnPlayerTurnChanged;
        GameManager.Instance.OnScoreChanged += OnScoreChanged;
    }


    private void OnDestroy()
    {
        GameManager.Instance.OnGameStarted -= OnGameStarted;
        GameManager.Instance.OnPlayerTurnChange -= OnPlayerTurnChanged;
        GameManager.Instance.OnScoreChanged -= OnScoreChanged;
    }

    private void OnGameStarted() 
    {
        if (GameManager.Instance.localPlayerType == PlayerType.Cross)
            CrossYouTextGO.SetActive(true);
        else 
            CircleYouTextGO.SetActive(true);

        UpdateCurrentArrow();

        CrossScoreTMP.text = "0";
        CircleScoreTMP.text = "0";

        Debug.Log("PlayerUIManager - OnGameStarted");
    }

    private void OnPlayerTurnChanged()
    {
        UpdateCurrentArrow();
    }

    private void OnScoreChanged() 
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);

        CrossScoreTMP.text = playerCrossScore.ToString();
        CircleScoreTMP.text = playerCircleScore.ToString();
    }

    private void UpdateCurrentArrow() 
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == PlayerType.Cross) 
        {
            crossArrowGO.SetActive(true);
            circleArromGO.SetActive(false);
        }
        else 
        {
            crossArrowGO.SetActive(false);
            circleArromGO.SetActive(true);
        }
    }
}
