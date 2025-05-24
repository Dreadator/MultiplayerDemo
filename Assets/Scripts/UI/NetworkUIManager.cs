using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;

    private void Awake()
    {
        startHostButton.onClick.AddListener(StartHost);
        startClientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        HideUI();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        HideUI();
    }

    private void HideUI() 
    {
        gameObject.SetActive(false);
    }
}
