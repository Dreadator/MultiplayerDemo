using Unity.Netcode;
using UnityEngine;

public class ConnectionApprovalHandler : MonoBehaviour
{
    public const int MAX_PLAYERS_ON_SERVER = 6;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    // Update is called once per frame
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) 
    {
        response.Approved = true;
        
        // if using player spawner with a default prefab
        //response.CreatePlayerObject = true;
        //response.PlayerPrefabHash = null;

        if(NetworkManager.Singleton.ConnectedClients.Count == MAX_PLAYERS_ON_SERVER) 
        {
            response.Approved = false;
            response.Reason = "Server is full.";
        }
        response.Pending = false;
    }
}
