using UnityEngine;

public class UI_Blocker : Singleton<UI_Blocker> 
{
    private void Start() 
    { 
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        Hide_Static();
    }

    public static void Show_Static() 
    {
        Instance.gameObject.SetActive(true);
        Instance.transform.SetAsLastSibling();
    }

    public static void Hide_Static() =>
        Instance.gameObject.SetActive(false);
}
