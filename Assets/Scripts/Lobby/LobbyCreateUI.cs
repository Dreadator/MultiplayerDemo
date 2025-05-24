using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : Singleton<LobbyCreateUI>
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button lobbyNameButton;
    [SerializeField] private Button publicPrivateButton;
    [SerializeField] private Button gameModeButton;
    [SerializeField] private Button maxPlayersButton;
    [SerializeField] private Button decreasePlayersButton;
    [SerializeField] private Button increasePlayersButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI publicPrivateText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;
    [SerializeField] private TextMeshProUGUI gameModeText;

    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;
    private LobbyManager.GameMode gameMode;

    private void Awake()
    {
        createButton.onClick.AddListener(CreateLobbyButtonClicked);
        lobbyNameButton.onClick.AddListener(ShowLobbyNameInputWindow);
        publicPrivateButton.onClick.AddListener(TogglePrivacy);
        gameModeButton.onClick.AddListener(SwitchGameModes);
        maxPlayersButton.onClick.AddListener(ShowMaxPlayersInputWindow);
        decreasePlayersButton.onClick.AddListener(DecreaseMaxPlayersCount);
        increasePlayersButton.onClick.AddListener(IncreaseMaxPlayersCount);
        exitButton.onClick.AddListener(Hide);

        Hide();
    }

    private void SwitchGameModes()
    {
        switch (gameMode)
        {
            default:
            case LobbyManager.GameMode.FeedingFrenzy:
                gameMode = LobbyManager.GameMode.BattleHippos;
                break;
            case LobbyManager.GameMode.BattleHippos:
                gameMode = LobbyManager.GameMode.FeedingFrenzy;
                break;
        }
        UpdateText();
    }

    private void ShowLobbyNameInputWindow()
    {
        UI_InputWindow.Show_Static("Lobby Name", lobbyName, "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ .,-", 20,
        () =>
        {
            // Cancel
        },
        (string lobbyName) =>
        {
            this.lobbyName = lobbyName;
            UpdateText();
        });
    }

    private void ShowMaxPlayersInputWindow()
    {
        UI_InputWindow.Show_Static("Max Players", maxPlayers,
        () =>
        {
            // Cancel
        },
        (int maxPlayers) =>
        {
            this.maxPlayers = maxPlayers;
            UpdateText();
        });
    }

    private void TogglePrivacy()
    {
        isPrivate = !isPrivate;
        UpdateText();
    }

    private void CreateLobbyButtonClicked()
    {
        LobbyManager.Instance.CreateLobby(
               lobbyName,
               maxPlayers,
               isPrivate,
               gameMode
           );

        LobbyListUI.Instance.ShowCreatingLobbyText();

        Hide();
    }

    private void UpdateText()
    {
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        maxPlayersText.text = maxPlayers.ToString();
        gameModeText.text = gameMode.ToString();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        lobbyName = "MyLobby";
        isPrivate = false;
        maxPlayers = 1;
        gameMode = LobbyManager.GameMode.FeedingFrenzy;

        UpdateText();
    }

    private void Hide() => gameObject.SetActive(false);

    private void DecreaseMaxPlayersCount() 
    {
        if (maxPlayers <= 1)
        { 
            maxPlayers = 1;
            return; 
        }

        --maxPlayers;
        maxPlayersText.text = maxPlayers.ToString();
    }

    private void IncreaseMaxPlayersCount()
    {
        if (maxPlayers >= RelayManager.MAX_PLAYERS_COUNT)
        {
            maxPlayers = RelayManager.MAX_PLAYERS_COUNT;
            return;
        }

        ++maxPlayers;
        maxPlayersText.text = maxPlayers.ToString();
    }

}