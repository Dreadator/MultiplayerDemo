using UnityEditor;
using UnityEngine;

public abstract class Singleton : MonoBehaviour
{
#if UNITY_EDITOR
    public abstract void DebugFixSingleton();
#endif
}

[DefaultExecutionOrder(-100)]
public abstract class Singleton<T> : Singleton where T : Component
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = default;
    }

    private static T instance;

    public bool dontDestroyOnLoad = false;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                if (instance == null)
                {                   
                    Debug.Log($"No instance of {typeof(T).Name} found");
                    return new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null || !instance.gameObject.activeSelf)
        {
            instance = this as T;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance == this)
            {
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
                return;
            }

            Debug.Log($"{name}: {instance.name} Destroying: {gameObject.name} from {gameObject.scene.name}");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (this == instance)
            instance = null;
    }

#if UNITY_EDITOR
    public override void DebugFixSingleton()
    {
        instance = null;
    }
#endif
}