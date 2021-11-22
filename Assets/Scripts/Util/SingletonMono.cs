using UnityEngine;
using System.Collections;

public class Singleton<T> where T : class, new()
{
    private static T _Instance;
    private static readonly object syncRoot = new object();
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
                return null;

            if (_Instance == null)
            {
                lock (syncRoot)
                {
                    if (_Instance == null)
                    {
                        _Instance = new T();
                    }
                }
            }

            return _Instance;
        }
    }

    void OnApplicationQuit()
    {
        _Instance = null;
        applicationIsQuitting = true;
    }

    public bool IsCreateInstance() { return _Instance != null; }
    public static void DestroyInstance() { _Instance = null; }
}

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
	#region Private Variables

	private static T instance;
	private static object locking = new object();
	private static bool applicationIsQuitting = false;
    public static bool ApplicationIsQuitting { get { return applicationIsQuitting; } }

    #endregion


    #region Make and Return

    public static T Instance
	{
		get
		{
			if(applicationIsQuitting) 
			{
				Debug.LogWarning("[Singleton] Instance '"+ typeof(T) + "' already destroyed on application quit." + " Won't create again - returning null.");

				return null;
			}
 
#if UNITY_EDITOR
            Debug.AssertFormat(Application.isPlaying, "Not Playing Mode : Singletone({0}) Instance Create!!!!!!!!", typeof(T));
#endif


			lock(locking)
			{
				if(instance == null)
				{
					instance = FindObjectOfType(typeof(T)) as T;
 
					if(FindObjectsOfType(typeof(T)).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong " + " - there should never be more than 1 singleton!" + " Reopening the scene might fix it.");

						return instance;
					}
 
					if(instance == null)
					{
						GameObject singleton = new GameObject();
						instance = singleton.AddComponent<T>();
						singleton.name = "(singleton)"+ typeof(T).ToString();
 
                        if (Application.isPlaying)
                            DontDestroyOnLoad(singleton);
 
						Debug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + singleton + "' was created with DontDestroyOnLoad.");
					} 
					else 
					{
						Debug.Log("[Singleton] Using instance already created: " + instance.gameObject.name);
					}
				}
 
				return instance;
			}
		}
	}

    #endregion


    #region Destory

    public static bool IsCreateInstance() { return instance != null; }

    void OnApplicationQuit()
    {
        instance = null;
        applicationIsQuitting = true;
    }

    #endregion
}