using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using PixelBattleText;

public class GameEndUIManager : NetworkBehaviour
{
    [Header("Text animation settings")]
    [SerializeField] Vector3 textSpawnPosition = new Vector3(0.5f, 0.80f, 0);
    [SerializeField] TextAnimation textAnimation;
    [Header("Player Details UI")]
    [SerializeField] GameEndPlayerScoreUI[] playerScoreDetails;

    [Header("Buttons")]
    [SerializeField] Button retryButton;
    [SerializeField] Button backToLobbyButton;

    private void Awake()
    {
        retryButton.onClick.AddListener(HandleRetryButtonpressed);
        backToLobbyButton.onClick.AddListener(HandleReturnToLobbyPressed);
    }

    private void Start()
    {
        if (NetworkGameManager.Instance)
        {
            NetworkGameManager.Instance.OnGameEnded += HungryHippoGameManager_OnGameEnded;
            NetworkGameManager.Instance.OnGameRestarted += HideEndGameScreen;
        }

        gameObject.SetActive(false);
    }
    private void HungryHippoGameManager_OnGameEnded(Dictionary<ulong, int> clientRoundWinProgress, Dictionary<ulong, int> clientsTotalBallsCollected)
    {
        int highestScore = clientRoundWinProgress.Values.Max();
        var entryWithHighestScore = clientRoundWinProgress.First(kvp => kvp.Value == highestScore);
        ulong clientWithHighestWins = entryWithHighestScore.Key;

        foreach (var kvp in clientRoundWinProgress)
        {
            Debug.Log($"clieent id is : {kvp.Key} , and the round win count is {kvp.Value}");
        }

        foreach (var kvp in clientsTotalBallsCollected)
        {
            Debug.Log($"clieent id is : {kvp.Key} , and the total ball count is {kvp.Value}");
        }
        int index = 0;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            int roundsWon = 0;
            int totalBallsCollected = 0;

            if (clientRoundWinProgress.TryGetValue(client.ClientId, out int wins))
            {
                roundsWon = wins;
            }
            if (clientsTotalBallsCollected.TryGetValue(client.ClientId, out int ballsCollected))
            {
                totalBallsCollected = ballsCollected;
            }

            playerScoreDetails[index].UpdatePlayerScoreInfo(client.ClientId, roundsWon, totalBallsCollected);
            playerScoreDetails[index].gameObject.SetActive(true);

            ++index;
        }
        gameObject.SetActive(true);
        ShowGameWinnerText(clientWithHighestWins);
    }

    private void ShowGameWinnerText(ulong clientId)
    {
        PixelBattleTextController.DisplayText(
                $"Player {clientId + 1} Wins!",
                textAnimation,
                textSpawnPosition);
    }

    private void HandleRetryButtonpressed() =>
        NetworkGameManager.Instance.RestartGameServerRPC();

    private void HandleReturnToLobbyPressed() =>
        RequestBackToLobbyRPC();

    [Rpc(SendTo.Server)]
    private void RequestBackToLobbyRPC()
    {
        TriggerDisconnectClientRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerDisconnectClientRPC() 
    {
        Disconnect();
    }

    private void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();

        if (IsHost)
            LobbyManager.Instance.DeleteJoinedLobby();

        LobbyManager.Instance.HandleLobbyExit();
        SceneManager.LoadScene("LobbyScene");
    }

    private void HideEndGameScreen() =>
            gameObject.SetActive(false);
}
