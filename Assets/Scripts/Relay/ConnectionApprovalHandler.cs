using Unity.Netcode;
using UnityEngine;

public class ConnectionApprovalHandler : MonoBehaviour
{ 
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) 
    {
        response.Approved = true;
        
        // if using player spawner with a default prefab
        //response.CreatePlayerObject = true;
        //response.PlayerPrefabHash = null;

        if(NetworkManager.Singleton.ConnectedClients.Count == RelayManager.MAX_PLAYERS_COUNT) 
        {
            response.Approved = false;
            response.Reason = "Server is full.";
        }
        response.Pending = false;
    }
}
