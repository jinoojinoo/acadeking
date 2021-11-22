using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class InGameUIScene : GameUISequence
{
    private static List<InGameUIScene> m_uisceneList = new List<InGameUIScene>();
    protected static List<InGameUIScene> UISceneList
    {
        get
        {
            return m_uisceneList;
        }
    }

    private static InGameUIScene m_instance = null;
    public static InGameUIScene Instance
    {
        get
        {
            return m_instance;
        }
        set
        {
            m_instance = value;

            m_uisceneList.Add(value);
            m_uisceneList.RemoveAll(x => x == null);
        }
    }

    private UICamera m_currentUICamera = null;
    public UICamera CurrentUICamera
    {
        get
        {
            if (m_currentUICamera == null)
            {
                m_currentUICamera = FindUICameraParents(MyTrans);
                m_currentUICamera.name = "FindUICamera";
            }

            return m_currentUICamera;
        }
    }

    private Camera m_currentCamera = null;
    public Camera CurrentCamera
    {
        get
        {
            if (m_currentCamera == null)
            {
                m_currentCamera = CurrentUICamera.GetComponent<Camera>();
            }
            return m_currentCamera;
        }
    }

    private Vector3 m_oldposition = Vector3.zero;
    protected Vector3 OldPosition
    {
        get
        {
            return m_oldposition;
        }
    }

    protected virtual void Awake()
    {
        Instance = this;
        //        m_gamesequenceList.Push(this);
        m_oldposition = MyTrans.localPosition;
        HUDScale.SetHUDModifyCamera(MyCamera);
    }

    private UICamera FindUICameraParents(Transform trans)
    {
        if (trans == null)
        {
            return GameObject.FindObjectOfType<UICamera>();
        }

        UICamera uicamera = trans.GetComponentInChildren<UICamera>();
        if (uicamera != null)
        {
            return uicamera;
        }

        uicamera = trans.GetComponentInParent<UICamera>();
        if (uicamera != null)
        {
            return uicamera;
        }

        return FindUICameraParents(trans.parent);
    }

    public bool GetUICameraEnable()
    {
        return CurrentUICamera.enabled;
    }

    public void ChangeScene(SceneState sceneState, string addscene = null, SceneManager.OnLoadDoneDelegate func = null, bool uncondition = false)
    {
        ChangeScene(SceneManager.Instance.GetSceneStateToName(sceneState), addscene, func, uncondition);
    }

    public void ChangeScene(string sceneName, string addscene = null, SceneManager.OnLoadDoneDelegate func = null, bool uncondition = false)
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Changescene);

        if (SceneManager.Instance.LoadScene(sceneName, addscene, func, false, uncondition))
            SetUICameraEnable(false);
    }

    public void ReturnBackScene(string addscene = null, SceneManager.OnLoadDoneDelegate func = null, bool uncondition = false)
    {
        if (SceneManager.Instance.LoadScene(SceneManager.Instance.PreSceneName, addscene, func, false, uncondition))
            SetUICameraEnable(false);
    }

    public void ChangeSceneForNetwork(SceneState sceneState, string addscene = null, SceneManager.OnLoadDoneDelegate func = null, bool uncondition = false)
    {
        ChangeScene(SceneManager.Instance.GetSceneStateToName(sceneState), addscene, func, uncondition);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;
    }

    protected abstract void ActionAndroidBackKey();
    public override void PopSequence(GAME_UI_MODE mode = GAME_UI_MODE.None)
    {
        if (PopupManager.Instance.IsPopupVisible)
        {
            PopupManager.Instance.ClosePopup(null);
            return;
        }

        if (GameUIManager.Instance.IsLastGameUISequence())
        {
            ActionAndroidBackKey();
            return;
        }

        GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
        sequence.PopSequence();
    }

    public override void StartGameSequence(int option)
    {

    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        return 0;
    }

    /*    private Stack<GameUISequence> m_gamesequenceList = new Stack<GameUISequence>();
        private Dictionary<GAME_UI_MODE, GameObject> m_gameobjectList = new Dictionary<GAME_UI_MODE, GameObject>();

        public GameUISequence PushSequence(GameObject obj)
        {
            GameUISequence isequence = obj.GetComponent<GameUISequence>();

            int option = 0;
            if (m_gamesequenceList.Count > 0)
                option = m_gamesequenceList.Peek().EndGameSequence(true);

            isequence.StartGameSequence(option);
            m_gamesequenceList.Push(isequence);

            return isequence;
        }

        public IGameSequence PushSequence(GAME_UI_MODE type)
        {
            if (m_gamesequenceList.Count > 0 && m_gamesequenceList.Peek().MyGameUIMode == type)
                return m_gamesequenceList.Peek();

            GameObject obj = GetGameUIObject(type);
            if (obj == null)
            {
                Debug.LogError("not found sequence path : " + type);
                return null;
            }

            return PushSequence(obj);
        }

        private GameObject GetGameUIObject(GAME_UI_MODE type)
        {
            UIGame_DataProperty info = UIGame_Table.Instance.GetUIGameDataProperty(type);
            if (info == null)
                return null;

            if (m_gameobjectList.ContainsKey(type) && m_gameobjectList[type] != null)
                return m_gameobjectList[type];

            GameObject obj = ResourceManager.Instance.LoadResourceObject(info.Path);
            //        obj = Instantiate(obj) as GameObject;

            obj.transform.parent = MyTrans;

            m_gameobjectList.Remove(type);
            m_gameobjectList.Add(type, obj);

            return obj;
        }


        public void PopSequence()
        {
            if (PopupManager.Instance.IsPopupVisible)
            {
                PopupManager.Instance.ClosePopup(null);
                return;
            }

            if (m_gamesequenceList.Count <= 1)
            {
                ActionAndroidBackKey();
                return;
            }

            IGameSequence sequence = m_gamesequenceList.Peek();
            int option = sequence.EndGameSequence();
            if (option == -1)
                return;

            m_gamesequenceList.Pop();
            if (m_gamesequenceList.Count <= 0)
                return;

            m_gamesequenceList.Peek().StartGameSequence(option);
        }*/

    protected virtual void LateUpdate()
    {
        if (GameModule.AndroidKeyManager.ApplicationIsQuitting)
            return;

        //  back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PopSequence();

            //             if (GameUIManager.Instance.IsLastGameUISequence())
            //             {
            //                 PopSequence();
            //             }
            //             else
            //             {
            //                 GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
            //                 sequence.PopSequence();
            //             }
        }
        // home button
        else if (Input.GetKeyDown(KeyCode.Home))
        {
            GameModule.AndroidKeyManager.Instance.OnClickHomeButton();
        }
        //  menu button
        else if (Input.GetKeyDown(KeyCode.Menu))
        {
            GameModule.AndroidKeyManager.Instance.OnClickMenuButton();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.ScrollLock))
            GameUtil.CaptureScreenshot();
#endif

    }

    protected virtual void LoadDoneFunc()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }
}


