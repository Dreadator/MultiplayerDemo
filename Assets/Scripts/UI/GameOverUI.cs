using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] Color winColour;
    [SerializeField] Color lostColour;
    [SerializeField] Color tiedColour;
    [SerializeField] Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(RematchButtonClick);
    }
    private void Start()
    {
        GameManager.Instance.OnGameWin += OnGameWin;
        GameManager.Instance.OnRematch += HideResult;
        GameManager.Instance.OnGameTied += OnGameTied;

        HideResult();
    }

    private void OnGameTied() 
    {
        resultText.color = tiedColour;
        resultText.text = "Draw!";
        ShowResult();
    }

    private void OnGameWin(Line line, PlayerType playerType) 
    {
        if (playerType == GameManager.Instance.GetLocalPlayerType()) 
        {
            resultText.color = winColour;
            resultText.text = "You Win!!!";
        }
        else 
        {
            resultText.color = lostColour;
            resultText.text = "You Lose!";
        }

        ShowResult();
    }

    private void ShowResult() 
    {
        gameObject.SetActive(true);
        rematchButton.gameObject.SetActive(true);
    }

    private void HideResult()
    {
        gameObject.SetActive(false);
        rematchButton.gameObject.SetActive(false);
    }

    private void RematchButtonClick() 
    {
        GameManager.Instance.RematchRPC();
    }
}
