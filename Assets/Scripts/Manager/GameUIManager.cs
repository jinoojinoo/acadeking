using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
public enum GAME_UI_MODE
{
    None,
    Start,
    InGame,

    UI_Lobby,
    UI_CreateRoom,
    UI_RoomList,
    UI_MemberList,
    UI_Shop,
    UI_Option,
    UI_GameResult,
    UI_Inventory,
    UI_Coupon,
}

public abstract class GameUISequence : MonoBehaviour
{
    private bool m_destoryObject = false;
    protected bool DestoryObject    
    {
        get
        {
            return m_destoryObject;
        }
    }

    private bool m_isRootUI = false;
    public bool IsRootUI
    {
        get
        {
            return m_isRootUI;
        }
        protected set
        {
            m_isRootUI = value;
        }
    }

    private bool m_ispopupUI = false;
    public bool IsPopupUI
    {
        protected set
        {
            m_ispopupUI = value;
        }
        get
        {
            return m_ispopupUI;
        }
    }

    private GAME_UI_MODE m_myGameUIMode = GAME_UI_MODE.None;
    public GAME_UI_MODE MyGameUIMode
    {
        set
        {
            m_myGameUIMode = value;
        }
        get
        {
            return m_myGameUIMode;
        }
    }

    private GameObject m_myObject = null;
    protected GameObject MyObject
    {
        get
        {
            if (m_myObject == null)
                m_myObject = this.gameObject;

            return m_myObject;
        }
    }

    private Transform m_myTrans = null;
    public Transform MyTrans
    {
        get
        {
            if (m_myTrans == null)
                m_myTrans = this.transform;

            return m_myTrans;
        }
    }

    public GameObject TargetObject
    {
        get;
        set;
    }

    private Camera m_myCamera = null;
    public Camera MyCamera
    {
        get
        {
            if (m_myCamera == null)
            {
                m_myCamera = MyObject.GetComponent<Camera>();
                if (m_myCamera == null)
                    m_myCamera = MyObject.GetComponentInChildren<Camera>();
            }

            return m_myCamera;
        }
    }

    private UICamera m_myUICamera = null;
    public UICamera MyUICamera
    {
        get
        {
            if (m_myUICamera == null)
            {
                m_myUICamera = MyObject.GetComponent<UICamera>();
                if (m_myUICamera == null)
                    m_myUICamera = MyObject.GetComponentInChildren<UICamera>();

                m_myUICamera.name = "FindUICamera";
            }

            return m_myUICamera;
        }
    }

    public void SetUICameraEnable(bool enable)
    {
//        Debug.LogError("camera enable : " + enable);
        MyUICamera.enabled = enable;
    }

    public void ActionMessageSend()
    {
        SetUICameraEnable(false);
    }

    public void ActionMessageReceive()
    {
        SetUICameraEnable(true);
    }

    public abstract void StartGameSequence(int option);
    public virtual void StartGameSequenceForAddUI()
    {

    }
    public abstract int EndGameSequence(GameUISequence togameseq);
    public virtual void EndGameSequenceForPopup(GameUISequence togameseq)
    {

    }

    public virtual void EndGameSequenceForRoot()
    {

    }
    public virtual void PopSequence(GAME_UI_MODE mode = GAME_UI_MODE.None)
    {
        GameUIManager.Instance.PopSequence(mode);
    }

    public void Close()
    {
        PopSequence();
    }

    private Dictionary<Network_ID, System.Action<bool>> m_dicPakcetHandle = new Dictionary<Network_ID, System.Action<bool>>();
    protected void AddPacketHandler(Network_ID id, System.Action<bool> action)
    {
        if (m_dicPakcetHandle.ContainsKey(id) == false)
        {
            m_dicPakcetHandle.Add(id, action);
        }
        else
        {
            m_dicPakcetHandle[id] += action;
        }

        NetworkMessageManager.Instance.AddReceivePacketHandler(id, action);
    }

    private void RemovePacketHandler()
    {
        foreach (Network_ID id in m_dicPakcetHandle.Keys)
        {
            NetworkMessageManager.Instance.RemoveReceivePacketHandler(id, m_dicPakcetHandle[id]);
        }
        m_dicPakcetHandle.Clear();
    }

    protected virtual void OnDestroy()
    {
        m_destoryObject = true;

        RemovePacketHandler();
        RemovePacketHandlerMessage();
    }

    private Dictionary<Network_ID, System.Action<NetworkMessage_Base>> m_dicPakcetHandleMessage = new Dictionary<Network_ID, System.Action<NetworkMessage_Base>>();
    protected void AddPacketHandler(Network_ID id, System.Action<NetworkMessage_Base> action)
    {
        if (m_dicPakcetHandleMessage.ContainsKey(id) == false)
        {
            m_dicPakcetHandleMessage.Add(id, action);
        }
        else
        {
            m_dicPakcetHandleMessage[id] += action;
        }

        NetworkMessageManager.Instance.AddReceivePacketHandler(id, action);
    }

    private void RemovePacketHandlerMessage()
    {
        foreach (Network_ID id in m_dicPakcetHandleMessage.Keys)
        {
            NetworkMessageManager.Instance.RemoveReceivePacketHandler(id, m_dicPakcetHandleMessage[id]);
        }
        m_dicPakcetHandleMessage.Clear();
    }
}

public class SingletonManagerBase : MonoBehaviour
{
    protected static bool _ApplicationIsQuitting = false;

    protected virtual void Awake()
    {
        _ApplicationIsQuitting = false;
    }

    protected virtual void OnApplicationQuit()
    {
        _ApplicationIsQuitting = true;
    }
}

public class SingletonManager<T> : SingletonManagerBase where T : class
{
    protected static T m_instance = null;
    public static T Instance
    {
        get
        {
            if (_ApplicationIsQuitting)
                return null;

            if (m_instance != null)
                return m_instance;

            m_instance = FindObjectOfType(typeof(T)) as T;
            return m_instance;
        }
    }

    public static bool IsCreateInstance()
    {
        return m_instance != null;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        m_instance = null;
    }

    protected virtual void OnDestroy()
    {
        m_instance = null;
    }
}

public class GameUIManager : SingletonManagerBase
{
    private const string Manager_Prefabs_Path = "Prefabs/GameUIManager";
    private const string GameUI_Layer = "UI";

    private static GameUIManager m_instance = null;
    public static GameUIManager Instance
    {
        get
        {
            if (_ApplicationIsQuitting)
                return null;

            if (m_instance != null)
                return m_instance;

            GameObject obj = ResourceManager.Instance.LoadResourceObject(Manager_Prefabs_Path);
            DontDestroyOnLoad(obj);
            m_instance = obj.GetComponent<GameUIManager>();

            return m_instance;
        }
    }

    private int m_layerNumber = int.MinValue;
    private int LayerNumber
    {
        get
        {
            if (m_layerNumber == int.MinValue)
                m_layerNumber = LayerMask.NameToLayer(GameUI_Layer);
            return m_layerNumber;
        }
    }

    private Transform m_myTrans = null;
    public Transform MyTrans
    {
        get
        {
            if (m_myTrans == null)
                m_myTrans = this.transform;
            return m_myTrans;
        }
    }

    private Stack<GameUISequence> m_gamesequenceList = new Stack<GameUISequence>();
    private Dictionary<GAME_UI_MODE, GameObject> m_gameobjectList = new Dictionary<GAME_UI_MODE, GameObject>();

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        //        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnLoaded;
        //        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnLoaded;
    }

    public GameUISequence GetCurrentUISequence()
    {
        if (m_gamesequenceList.Count <= 0)
            return null;

        return m_gamesequenceList.Peek();
    }

    // called second
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (mode == UnityEngine.SceneManagement.LoadSceneMode.Additive)
            return;

        InitGameSequenceObject();
    }

    //     void OnSceneUnLoaded(UnityEngine.SceneManagement.Scene scene)
    //     {
    //         if (string.Compare(scene.name, E_SCENE_NAME.SceneLobby.ToString()) != 0)
    //             return;
    // 
    //         InitGameSequenceObject();
    //     }

    private void OnDestroy()
    {
        InitGameSequenceObject();
    }

    public void InitGameSequenceObject()
    {
        m_gamesequenceList.Clear();
        foreach (GameObject obj in m_gameobjectList.Values)
        {
            if (obj == null)
                continue;
            Destroy(obj);
        }
        m_gameobjectList.Clear();
    }

    private GameObject GetGameUIObject(GAME_UI_MODE type)
    {
        UIGame_DataProperty info = UIGame_Table.Instance.GetUIGameDataProperty(type);
        if (info == null)
            return null;

        if (m_gameobjectList.ContainsKey(type) && m_gameobjectList[type] != null)
            return m_gameobjectList[type];

        if (string.IsNullOrEmpty(info.Path))
            return null;

        GameObject obj = ResourceManager.Instance.LoadResourceObject(info.Path);
        //        obj = Instantiate(obj) as GameObject;

        m_gameobjectList.Remove(type);
        m_gameobjectList.Add(type, obj);

        return obj;
    }

    public GameUISequence PushSequence(GAME_UI_MODE type, GameObject myobject = null)
    {
        if (m_gamesequenceList.Count > 0 && m_gamesequenceList.Peek().MyGameUIMode == type)
            return m_gamesequenceList.Peek();

        GameObject obj = null;
        if (myobject != null)
            obj = myobject;
        else
            obj = GetGameUIObject(type);

        if (obj == null)
        {
            Debug.LogError("not found sequence path : " + type);
            return null;
        }

        return PushSequence(obj, type, myobject == null);
    }

    public GameUISequence PushSequence(GameObject obj, GAME_UI_MODE type, bool setparent = true)
    {
        GameUISequence isequence = obj.GetComponent<GameUISequence>();
        isequence.TargetObject = obj;
        isequence.TargetObject.SetActive(true);
        if (isequence.MyGameUIMode == GAME_UI_MODE.None)
            isequence.MyGameUIMode = type;

        if (setparent == false)
        {
            PushSequence(isequence);
            return isequence;
        }

        Transform trans = obj.transform;

        trans.parent = MyTrans;
        trans.localScale = Vector3.one;
        if (isequence.IsPopupUI == false)
            trans.localPosition = Vector3.zero;

        obj.transform.ChangeLayerReculsively(LayerNumber);

        Camera cam = isequence.MyCamera;
        if (isequence.IsPopupUI)
        {
            cam.depth = 100;
        }
        else
        {
            float depth = CheckCameraDepth(isequence);
            if (depth != float.MinValue)
                cam.depth = depth;
        }
        PushSequence(isequence);
        return isequence;
    }

    private const int m_startcamerDepth = 6;
    private float CheckCameraDepth(GameUISequence currentcequence)
    {
        if (m_gamesequenceList.Count <= 0)
            return m_startcamerDepth;

        float depth = float.MinValue;
        foreach (GameUISequence sequence in m_gamesequenceList)
        {
            if (sequence.TargetObject.activeSelf == false)
                continue;

            if (depth < sequence.MyCamera.depth)
                depth = sequence.MyCamera.depth;
        }

        if (depth == float.MinValue)
            return depth;

        return depth + 1;
    }

    private void PushSequence(GameUISequence sequence)
    {
        PopSequencePopupUI();

        int option = 0;
        if (m_gamesequenceList.Count > 0)
        {
            GameUISequence from = m_gamesequenceList.Peek();
            if (from.IsRootUI)
            {
                from.EndGameSequenceForRoot();
            }
            else if (sequence.IsPopupUI == false)
            {
                option = from.EndGameSequence(sequence);
                if (from.TargetObject != null)
                    from.TargetObject.SetActive(false);
            }
            else
            {
                from.EndGameSequenceForPopup(sequence);
            }
        }

        m_gamesequenceList.Push(sequence);
        sequence.StartGameSequence(option);
    }

    private void PopSequencePopupUI()
    {
        if (m_gamesequenceList.Count <= 0)
            return;

        while (m_gamesequenceList.Count > 0)
        {
            GameUISequence gameseq = m_gamesequenceList.Peek();
            if (gameseq.IsPopupUI == false)
                break;

            if (gameseq.TargetObject != null)
            {
                Destroy(gameseq.TargetObject);
                gameseq.TargetObject = null;
            }

            m_gamesequenceList.Pop();
        }
    }

    private System.Action m_popsequenceFunc = null;
    public System.Action PopSequenceFunc
    {
        set
        {
            m_popsequenceFunc = value;
        }
    }

    private void ActionPopSequecne()
    {
        if (m_popsequenceFunc == null)
            return;

        m_popsequenceFunc();
        m_popsequenceFunc = null;
    }

    public void PopSequence(GAME_UI_MODE mode = GAME_UI_MODE.None)
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);

        if (m_gamesequenceList.Count <= 0)
        {
            Debug.LogError("m_gamesequenceList empty");
            return;
        }

        GameUISequence fromsequence = m_gamesequenceList.Peek();
        if (mode != GAME_UI_MODE.None && fromsequence.MyGameUIMode == mode)
            return;

        m_gamesequenceList.Pop();

        bool dontdestroy = false;
        // 돌아갈 구간에 해당 sequence가 존재할경우 오브젝트는 없애지 않는다.
        // ex) a -> b -> c -> b
        if (mode == GAME_UI_MODE.None)
        {
            foreach (GameUISequence sequence in m_gamesequenceList)
            {
                if (sequence.MyGameUIMode == fromsequence.MyGameUIMode)
                {
                    dontdestroy = true;
                    break;
                }
            }
        }

        if (m_gamesequenceList.Count <= 0)
            return;

        // 구간점프로 돌아갈 경우 endgamesquence는 호출해 주지 않고 오브젝트만 파괴해 준다
        while (mode != GAME_UI_MODE.None)
        {
            GameUISequence sequence = m_gamesequenceList.Peek();
            if (m_gamesequenceList.Count <= 1 ||
                sequence.MyGameUIMode == mode)
                break;

            if (sequence.TargetObject != null)
            {
                Destroy(sequence.TargetObject);
                sequence.TargetObject = null;
            }

            m_gamesequenceList.Pop();
        }

        if (m_gamesequenceList.Count <= 0)
        {
            fromsequence.EndGameSequence(null);
            if (fromsequence.TargetObject != null)
            {
                Destroy(fromsequence.TargetObject);
                fromsequence.TargetObject = null;
            }

            ActionPopSequecne();
            return;
        }

        GameUISequence tosequence = m_gamesequenceList.Peek();
        int option = fromsequence.EndGameSequence(tosequence);
        if (fromsequence.TargetObject != null)
        {
            if (dontdestroy)
            {
                fromsequence.TargetObject.SetActive(false);
            }
            else
            {
                Destroy(fromsequence.TargetObject);
                fromsequence.TargetObject = null;
            }
        }

        if (option == -1)
        {
            ActionPopSequecne();
            return;
        }

        if (tosequence.TargetObject != null)
            tosequence.TargetObject.SetActive(true);
        tosequence.StartGameSequence(option);

        ActionPopSequecne();
    }

    public static void DeepCopy(object src, object tar)
    {
        System.Type srctype = src.GetType();
        System.Type destype = tar.GetType();
        foreach (System.Reflection.FieldInfo finfo in srctype.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            System.Reflection.FieldInfo copyinfo = destype.GetField(finfo.Name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (copyinfo != null)
            {
                copyinfo.SetValue(tar, finfo.GetValue(src));
            }
        }
    }

    public bool IsLastGameUISequence()
    {
        int count = 0;
        foreach (GameUISequence sequence in m_gamesequenceList)
        {
            if (sequence.IsPopupUI)
                continue;

            count++;
        }
        return count <= 1;
    }
}
