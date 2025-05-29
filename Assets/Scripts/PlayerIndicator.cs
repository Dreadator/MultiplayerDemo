using Unity.Netcode;
using UnityEngine;

public class PlayerIndicator : NetworkBehaviour
{
    [SerializeField] GameObject playerIndicator;

    private void Start() => ShowPlayerIndicator();

    private void ShowPlayerIndicator() 
    {
        if (IsOwner) 
        {
            playerIndicator.SetActive(true);
            playerIndicator.transform.parent = null;
        }
    }
}
