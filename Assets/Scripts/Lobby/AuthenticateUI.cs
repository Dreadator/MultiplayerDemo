using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
public class AuthenticateUI : MonoBehaviour {

    private const string ENTER_NAME = "Enter Name...";

    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button authenticateButton;

    [SerializeField] private GameObject lobbyListUI;

    private void Awake() 
    {
        authenticateButton.onClick.AddListener(HandleAuthenticationButtonClicked);
    }

    private void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;

        if (AuthenticationService.Instance.IsSignedIn) 
        {
            gameObject.SetActive(false);
        }
    }

    private void HandleAuthenticationButtonClicked() 
    {
        if (string.IsNullOrEmpty(nameInputField.text) ||
            nameInputField.text == ENTER_NAME) 
        {
            nameInputField.text = "Please enter a name";
            Debug.Log("Name field was empty");
            return;
        }

        string nameCleanup = nameInputField.text.Replace(" ", string.Empty);
        Debug.Log("Clean up name: " + nameCleanup);
        LobbyManager.Instance.UpdatePlayerName(nameCleanup);
        LobbyManager.Instance.Authenticate(nameCleanup);
        lobbyListUI.SetActive(true);
        Hide();
    }

    private void Hide() 
    {
        gameObject.SetActive(false);
    }
}