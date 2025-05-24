using UnityEngine;
public class InternetCheck : MonoBehaviour
{
    [SerializeField] private float timerMax = 5f;

    private float timer;
    void Start()
    {
        timer = timerMax;
    }
    void Update()
    {
        timer -= Time.deltaTime; 
        if (timer > 0f) return;
        
        timer = timerMax;
        InternetReachabilityCheck();
    }

    private void InternetReachabilityCheck() 
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection");
        }
        else
        {
            Debug.Log("Internet connection available");
        }
    }
}
