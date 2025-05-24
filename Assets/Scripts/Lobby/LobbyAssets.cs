using UnityEngine;

public class LobbyAssets : Singleton<LobbyAssets>
{
    [SerializeField] private Sprite marineSprite;
    [SerializeField] private Sprite ninjaSprite;
    [SerializeField] private Sprite zombieSprite;
    [SerializeField] private Sprite RhinoSprite;

    public Sprite GetSprite(LobbyManager.PlayerCharacter playerCharacter)
    {
        switch (playerCharacter)
        {
            default:
            case LobbyManager.PlayerCharacter.Snake: return marineSprite;
            case LobbyManager.PlayerCharacter.Hippo: return ninjaSprite;
            case LobbyManager.PlayerCharacter.Crocodile: return zombieSprite;
            case LobbyManager.PlayerCharacter.Rhino: return zombieSprite;
        }
    }
}