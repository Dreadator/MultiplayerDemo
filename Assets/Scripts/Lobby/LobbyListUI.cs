using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance { get; private set; }

    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinLobbyButton;

    [SerializeField] private GameObject noActiveLobbyTextGO;
    [SerializeField] private GameObject creatingLobbyTextGO;

    private void Awake()
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);
        quickJoinLobbyButton.gameObject.SetActive(false);
        creatingLobbyTextGO.SetActive(false);

        refreshButton.onClick.AddListener(RefreshButtonClick);
        createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
        quickJoinLobbyButton.onClick.AddListener(QuickJoinLobbyClick);
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;

        Hide();
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {

        if (lobbyList.Count == 0)
        {
            noActiveLobbyTextGO.SetActive(true);
            quickJoinLobbyButton.gameObject.SetActive(false);
            return;
        }

        noActiveLobbyTextGO.SetActive(false);
        creatingLobbyTextGO.SetActive(false);
        quickJoinLobbyButton.gameObject.SetActive(true);

        foreach (Transform child in container)
        {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void RefreshButtonClick()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateLobbyButtonClick()
    {
        LobbyCreateUI.Instance.Show();
        ShowCreateAndJoinButtons(false);
    }

    private void QuickJoinLobbyClick()
    {
        LobbyManager.Instance.QuickJoinLobby();
        ShowCreateAndJoinButtons(false);
    }

    public void ShowCreateAndJoinButtons(bool show = true)
    {
        createLobbyButton.gameObject.SetActive(show);
        quickJoinLobbyButton.gameObject.SetActive(show);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    public void ShowCreatingLobbyText() 
    {
        noActiveLobbyTextGO.SetActive(false);
        creatingLobbyTextGO.SetActive(true);
    }
}