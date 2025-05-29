using TMPro;
using UnityEngine;

public class GameEndPlayerScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerText;
    [SerializeField] TextMeshProUGUI roundsWonText;
    [SerializeField] TextMeshProUGUI totalBallsCollectedText;

    public void UpdatePlayerScoreInfo(ulong clientId, int roundsWon, int totalBallsCollected) 
    {
        playerText.text = $"Player {clientId + 1}";
        roundsWonText.text = $"Rounds Won: {roundsWon}";
        totalBallsCollectedText.text = $"Total balls collected: {totalBallsCollected}";
    }
}
