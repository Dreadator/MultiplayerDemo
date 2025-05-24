using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXPrefab;
    [SerializeField] private Transform winSFXPrefab;
    [SerializeField] private Transform loseSFXPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += OnPlacedObject;
        GameManager.Instance.OnGameWin += OnGameWin;
    }

    private void OnPlacedObject() 
    {
        Transform sfxTransform = Instantiate(placeSFXPrefab);
        Destroy(sfxTransform.gameObject, 5f);
    }

    private void OnGameWin(Line line, PlayerType winPlayerType) 
    {
        if (GameManager.Instance.GetLocalPlayerType() == winPlayerType) 
        {
            Transform sfxTransform = Instantiate(winSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
        else
        {
            Transform sfxTransform = Instantiate(loseSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }   
    }
}
