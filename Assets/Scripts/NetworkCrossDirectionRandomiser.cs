using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkCrossDirectionRandomiser : NetworkBehaviour
{
    [SerializeField] NetworkAnimator networkAnimator;

    private NetworkVariable<int> index = new NetworkVariable<int>();

    private void Start()
    {
        if (!IsServer) return;

        if(NetworkGameManager.Instance)
            NetworkGameManager.Instance.OnRoundEnded += SetRandomIndex;
    }

    public override void OnDestroy()
    {
        if (!IsServer) return;

        if (NetworkGameManager.Instance)
            NetworkGameManager.Instance.OnRoundEnded -= SetRandomIndex;

        base.OnDestroy();
    }


    public override void OnNetworkSpawn() 
    {
        base.OnNetworkSpawn();

        index.OnValueChanged += OnIndexChanged;

        if(!IsServer) return;

        SetRandomIndex();
    }

    private void OnIndexChanged(int oldIndex, int newIndex) =>
        UpdateAnimatorIndex();

    private void SetRandomIndex() =>
        index.Value = Random.Range(0, 2);

    private void UpdateAnimatorIndex() =>
        networkAnimator.Animator.SetInteger("index", index.Value);
}
