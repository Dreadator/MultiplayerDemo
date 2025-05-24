using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : Singleton<RelayManager>
{
    public const string DTLS_ENCRYPTION = "dtls";
    public const string WSS_ENCRYPTION = "wss";

    public const int MAX_PLAYERS_COUNT = 4;

    public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);


        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

#if UNITY_WEBGL && !UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
#endif

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

#if UNITY_WEBGL && !UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
#endif

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

    public async Task<Allocation> AllocateRelay() 
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections: MAX_PLAYERS_COUNT);
            return allocation;
        }
        catch (RelayServiceException e) 
        {
            Debug.Log($"Failed to allocate relay : {e.Message}");
            return default;
        }
    }
    public async Task<string> GetRelayJoinCode(Allocation allocation) 
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {

            Debug.Log($"Failed to get join code : {e.Message}");
            return string.Empty;
        }
    }
    public async Task<JoinAllocation> JoinRelay(string relayJoinCode) 
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log($"Failed to get join relay : {e.Message}");
            throw;
        }
    }
    public void SetTransportAndStartHost(Allocation allocation)
    {
        GetTransportAndSetRelayServer(allocation);
        NetworkManager.Singleton.StartHost();
    }

    public void SetTransportAndStartClient(JoinAllocation joinAllocation) 
    {
        GetTransportAndSetRelayServer(joinAllocation);
        NetworkManager.Singleton.StartClient();
    }

    private void GetTransportAndSetRelayServer(Allocation allocation) 
    {
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
#if !UNITY_WEBGL
        unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, DTLS_ENCRYPTION));
#else
        unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, WSS_ENCRYPTION));
        unityTransport.UseWebSockets = true;
#endif
    }

    private void GetTransportAndSetRelayServer(JoinAllocation joinAllocation)
    {
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
#if !UNITY_WEBGL
        unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, DTLS_ENCRYPTION));
#else
        unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, WSS_ENCRYPTION));
        unityTransport.UseWebSockets = true;
#endif
    }
}
