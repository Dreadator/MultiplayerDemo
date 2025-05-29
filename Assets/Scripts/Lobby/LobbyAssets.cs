using UnityEngine;

public class LobbyAssets : Singleton<LobbyAssets>
{
    [SerializeField] private Sprite snakeSprite;
    [SerializeField] private Sprite hippoSprite;
    [SerializeField] private Sprite crocSprite;
    [SerializeField] private Sprite RhinoSprite;

    public Sprite GetSprite(int index)
    {
        switch (index)
        {
            default:
            case 0: return snakeSprite;
            case 1: return hippoSprite;
            case 2: return crocSprite;
            case 3: return crocSprite;
        }
    }
}