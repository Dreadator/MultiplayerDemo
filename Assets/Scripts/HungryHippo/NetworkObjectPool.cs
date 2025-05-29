using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectPool : NetworkBehaviour
{
    public static NetworkObjectPool Instance { get; private set; }

    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 10;

    private readonly List<NetworkObject> pool = new List<NetworkObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        InitializePool();
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(this));
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
            CreatePooledObject();       
    }

    private NetworkObject CreatePooledObject()
    {
        GameObject obj = Instantiate(prefab);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        obj.SetActive(false);
        pool.Add(netObj);
        return netObj;
    }

    public NetworkObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        foreach (var netObj in pool)
        {
            if (!netObj.gameObject.activeInHierarchy)
            {
                netObj.transform.SetPositionAndRotation(position, rotation);
                netObj.gameObject.SetActive(true);
                return netObj;
            }
        }

        var newObj = CreatePooledObject();
        newObj.transform.SetPositionAndRotation(position, rotation);
        newObj.gameObject.SetActive(true);
        return newObj;
    }

    public void ReturnToPool(NetworkObject netObj)
    {
        netObj.gameObject.SetActive(false);
    }

    private class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        private readonly NetworkObjectPool pool;

        public PooledPrefabInstanceHandler(NetworkObjectPool pool)
        {
            this.pool = pool;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return pool.GetFromPool(position, rotation);
        }

        public void Destroy(NetworkObject networkObject)
        {
            pool.ReturnToPool(networkObject);
        }
    }
}
