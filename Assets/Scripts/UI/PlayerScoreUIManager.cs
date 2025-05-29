using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerScoreUIManager : MonoBehaviour
{
    private const string SCORE_STRING = "Score: ";

    [SerializeField] private TextMeshProUGUI ScoreTMP;
    [SerializeField] private TextMeshProUGUI playerIDTMP;
    [SerializeField] private TextMeshProUGUI roundTMP;

    private NetworkHippoController hippoController;

    private IEnumerator Start()
    {
        if(NetworkPlayerSpawner.Instance)
            NetworkPlayerSpawner.Instance.OnPlayerSpawned += SetPlayerIdentifier;

        if (NetworkGameManager.Instance)
        {
            NetworkGameManager.Instance.OnRoundIncreased += SetRoundIndexText;
            NetworkGameManager.Instance.OnGameRestarted += ResetScore;
        }

        yield return new WaitForSeconds(1f);
        FindPlayerAndSubscribe();
    }

    private void OnDestroy()
    {
        if (hippoController)
        {
            hippoController.OnBallCollected -= UpdateScoreText;
            hippoController.OnBallCountReset -= UpdateScoreText;
        }

        if(NetworkPlayerSpawner.Instance)
            NetworkPlayerSpawner.Instance.OnPlayerSpawned -= SetPlayerIdentifier;

        if (NetworkGameManager.Instance)
        {
            NetworkGameManager.Instance.OnRoundIncreased -= SetRoundIndexText;
            NetworkGameManager.Instance.OnGameRestarted -= ResetScore;
        }
    }

    private void FindPlayerAndSubscribe()
    {
        hippoController = GameObject.FindWithTag("Player")
            .GetComponent<NetworkHippoController>();

        hippoController.OnBallCollected += UpdateScoreText;
        hippoController.OnBallCountReset += UpdateScoreText;
    }


    private void ResetScore() =>
        UpdateScoreText(0);

        
    private void UpdateScoreText(int score) =>
        ScoreTMP.text = SCORE_STRING + score.ToString();

    
    private void SetPlayerIdentifier() =>
        playerIDTMP.text = "Player " + (NetworkManager.Singleton.LocalClientId + 1);

    private void SetRoundIndexText(int roundIndex) =>
        roundTMP.text = $"Round :{roundIndex}";

}
