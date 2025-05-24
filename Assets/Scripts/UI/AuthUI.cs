using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signUpButton;
    [SerializeField] private Button GuestButton;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    private void Awake()
    {
        loginButton.onClick.AddListener(HandleLoginButtonClicked);
        signUpButton.onClick.AddListener(HandleSignUpButtonClicked);
        GuestButton.onClick.AddListener(HandleGuestButtonClicked);
    }

    private void HandleLoginButtonClicked() 
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        Debug.Log("Login button clicked");
    }

    private void HandleSignUpButtonClicked()
    {
        Debug.Log("Sign up button clicked");
    }

    private void HandleGuestButtonClicked()
    {
        Debug.Log("Guest button clicked");
    }
}
