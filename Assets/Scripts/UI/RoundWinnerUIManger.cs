using PixelBattleText;
using UnityEngine;

public class RoundWinnerUIManger : MonoBehaviour
{
    [SerializeField] private Vector3 textSpawnPosition = new Vector3(0.5f, 0.65f, 0);
    [SerializeField] private TextAnimation textAnimation;

    private void Start()
    {
        if(NetworkGameManager.Instance)
            NetworkGameManager.Instance.OnRoundWinnerAnnounced += ShowRoundWinnerText;
    }

    private void OnDestroy()
    {
        if(NetworkGameManager.Instance)
            NetworkGameManager.Instance.OnRoundWinnerAnnounced -= ShowRoundWinnerText;
    }

    private void ShowRoundWinnerText(ulong clientId) 
    {
        PixelBattleTextController.DisplayText(
                $"Player {clientId + 1} won the round!",
                textAnimation,
                textSpawnPosition);
    }

    [ContextMenu("Test Text")]
    private void TestText() 
    {
        ShowRoundWinnerText(0);
    }
}
