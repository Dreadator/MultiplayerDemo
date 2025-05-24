using TMPro;
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

    private void HandleAuthenticationButtonClicked() 
    {
        if (string.IsNullOrEmpty(nameInputField.text) ||
            nameInputField.text == ENTER_NAME) 
        {
            nameInputField.text = "Please enter a name";
            Debug.Log("Name field was empty");
            return;
        }

        LobbyManager.Instance.UpdatePlayerName(nameInputField.text);
        LobbyManager.Instance.Authenticate(nameInputField.text);
        lobbyListUI.SetActive(true);
        Hide();
    }

    private void Hide() 
    {
        gameObject.SetActive(false);
    }
}