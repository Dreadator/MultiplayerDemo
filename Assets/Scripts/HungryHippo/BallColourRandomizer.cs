using Unity.Netcode;
using UnityEngine;

public class BallColourRandomizer : NetworkBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BallColourPalleteSO ballColourPalleteSO;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsServer)
            RandomiseSpriteColour();
    }  
    private void RandomiseSpriteColour() 
    {
        int randomIndex = Random.Range(0, ballColourPalleteSO.GetBallColourPalleteCount());
        SetBallColourClientRPC(randomIndex);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetBallColourClientRPC(int index) 
    {
        Color[] ballColourPallete = ballColourPalleteSO.GetBallColourPallete();
        spriteRenderer.color = ballColourPallete[index];
    }
}
