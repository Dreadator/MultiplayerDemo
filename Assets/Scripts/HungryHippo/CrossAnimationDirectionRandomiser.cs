using Unity.Netcode;
using UnityEngine;

public class CrossAnimationDirectionRandomiser : NetworkBehaviour
{
    [SerializeField] Animator animator;

    private NetworkVariable<int> index = new NetworkVariable<int>();

    private void Start()
    {
        if (!IsServer) return;

        if(HungryHippoGameManager.Instance)
            HungryHippoGameManager.Instance.OnRoundEnded += SetRandomIndex;
    }

    private void OnDestroy()
    {

        if (!IsServer) return;

        if (HungryHippoGameManager.Instance)
            HungryHippoGameManager.Instance.OnRoundEnded -= SetRandomIndex;
    }


    public override void OnNetworkSpawn() 
    {
        index.OnValueChanged += OnIndexChanged;

        if(!IsServer) return;

        SetRandomIndex();
    }

    private void OnIndexChanged(int oldIndex, int newIndex) 
    {
        UpdateAnimatorIndex();
    }

    private void SetRandomIndex() 
    {
        index.Value = Random.Range(0, 2);
    }

    private void UpdateAnimatorIndex() 
    {
        Debug.Log("Updating animator index");
        animator.SetInteger("index", index.Value);
    }
}
