using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerScoreUIManager : MonoBehaviour
{
    private const string SCORE_STRING = "Score: ";

    [SerializeField] private TextMeshProUGUI ScoreTMP;

    private HippoController hippoController;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        FindPlayerAndSubscribe();
    }

    private void OnDestroy()
    {
        if (hippoController)
            hippoController.OnBallCollected -= UpdateScoreText;
    }

    private void FindPlayerAndSubscribe()
    {
        hippoController = GameObject.FindWithTag("Player")
            .GetComponent<HippoController>();

        hippoController.OnBallCollected += UpdateScoreText;
    }

    private void UpdateScoreText(int score) =>
        ScoreTMP.text = SCORE_STRING + score.ToString();

}
