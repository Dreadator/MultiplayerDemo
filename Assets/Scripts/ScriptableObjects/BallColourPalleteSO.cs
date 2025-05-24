using UnityEngine;

[CreateAssetMenu(fileName = "BallColourPalette", menuName = "ScriptableObjects/BallColourPallete")]
public class BallColourPalleteSO : ScriptableObject
{
    [SerializeField] Color[] ballColourPallete;

    public Color[] GetBallColourPallete() 
    {
        return ballColourPallete;
    }

    public int GetBallColourPalleteCount() 
    {
        return ballColourPallete.Length;    
    }
}
