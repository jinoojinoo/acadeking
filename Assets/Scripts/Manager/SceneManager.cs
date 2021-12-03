using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AssetBundles;

#if UNITY_EDITOR
	using UnityEditor;
#endif

public enum SceneState
{
    None = -1, 
    Start,
    Lobby,
    Arcade,
    Empty,
}

public class SceneManager : SingletonMono<SceneManager>
{
    public const string SceneName_Start = "Login";
    public const string SceneName_Lobby = "Lobby";
//    public const string SceneName_Arcade = "Arcade";
    public const string SceneName_InGame = "InGame2";
    public const string SceneName_EMPTY = "GameEmpty";

    public delegate void OnLoadDoneDelegate();
    private OnLoadDoneDelegate m_onloaddoneFunc = null;
    public OnLoadDoneDelegate OnLoadDoneFunc
    {
        set
        {
            m_onloaddoneFunc = value;
        }
        private get
        {
            return m_onloaddoneFunc;
        }
    }

    private Dictionary<string, InGamePrepareSceneLoad> m_preparesceneloadList = new Dictionary<string, InGamePrepareSceneLoad>();

    private string m_sceneName = string.Empty;
    private string m_presceneName = string.Empty;

    private SceneState m_sceneState = SceneState.None;

    private bool inToin = false;

    public string PreSceneName
    {
        get
        {
            return m_presceneName;
        }
    }
    public string SceneName
    {
        get
        {
            return m_sceneName;
        }
        private set
        {
            if (string.IsNullOrEmpty(m_sceneName) == false)
                m_presceneName = m_sceneName;

            m_sceneName = value;
        }
    }

    private string m_addsceneName = "";
    private string AddSceneName
    {
        get
        {
            return m_addsceneName;
        }
        set
        {
            m_addsceneName = value;
        }
    }

    private int sceneIndex = 0;

    void Awake()
    {
        InitPrepareSceneLoad();

        SetLoadingTransition();

        UIGameLoading.Instance.SetMode(TRANSITIONMODE.TRANSITION_CUTOFF);
        //UIGameLoading.Instance.SetMode(TRANSITIONMODE.TRANSITION_NONE);

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void SetLoadingTransition()
    {

    }

    void InitPrepareSceneLoad()
    {
        m_preparesceneloadList.Clear();
        m_preparesceneloadList.Add(SceneName_Lobby, new InGamePrepareSceneLoad_Lobby());
        m_preparesceneloadList.Add(SceneName_InGame, new InGamePrepareSceneLoad_InGame());        
    }

    private void OnDestroy()
    {
        m_preparesceneloadList.Clear();
    }

    private bool IsShowLoadingGuageScene()
    {
        return true;
    }

    // memory clear 를 위한 empty_scene 로드
    private IEnumerator ProcessLoadEmptyScene()
    {
        bool useBundle = true;
        AsyncOperation scene_operation_ingame = null;

#if UNITY_EDITOR
        if (AssetBundleManager.SimulateAssetBundleInEditor)
        {
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName("assetdata_scenes", SceneName_EMPTY);

            if (levelPaths.Length == 0)
                useBundle = false;
            else
            {
                UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);
            }
        }
        else
#endif
        {
//             if (!AssetBundleManager.Instance.BundleTable.CheckInBundleTable(SceneName_EMPTY))
//             {
//                 useBundle = false;
//             }
//             else
//             {
//                 AssetBundleManager.Instance.LoadBundle("assetdata_scenes");
// 
//                 while (!AssetBundleManager.Instance.IsBundleLoaded("assetdata_scenes"))
//                 {
//                     yield return new WaitForEndOfFrame();
//                 }

                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName_EMPTY);
//            }
        }

        if (!useBundle)
        {
            scene_operation_ingame = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName_EMPTY);

            if (scene_operation_ingame != null)
            {
                while (scene_operation_ingame.isDone == false)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        yield break;
    }

    private IEnumerator PrepeardSceneLoad(string scenename)
    {
        if (string.IsNullOrEmpty(scenename))
            yield break;

        if (m_preparesceneloadList.ContainsKey(scenename) == false)
            yield break;

        m_preparesceneloadList[scenename].ActionPrepareState();

        while (true)
        {
            InGamePrepareSceneLoad.PrepareStateType type = m_preparesceneloadList[scenename].GetPrepareState();

            if (type == InGamePrepareSceneLoad.PrepareStateType.Success)
            {
                UIGameLoading.Instance.SetLoadingGuage(1f);
                break;
            }
            else if (type == InGamePrepareSceneLoad.PrepareStateType.Wait)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }
            else if (type == InGamePrepareSceneLoad.PrepareStateType.Fail)
            {
                // 또는 재시작?
                SceneName = SceneName_Start;
                AddSceneName = string.Empty;

                break;
            }
        }

        yield break;
    }

    private IEnumerator ProcessLoadScene(string scenename)
    {
        bool useBundle = true;
        AsyncOperation scene_operation_ingame = null;


        // 실제 사용 background scene load
        if (string.IsNullOrEmpty(scenename))
            yield break;

        if (UIGameLoading.Instance.CurrentLoadingState == UIGameLoading.LoadingState.After_Start)
        {
            UIGameLoading.Instance.SetLoadingGuage(0);
        }

        useBundle = true;

#if UNITY_EDITOR
        if (AssetBundleManager.SimulateAssetBundleInEditor)
        {
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName("assetdata_scenes", scenename);

            if (levelPaths.Length == 0)
                useBundle = false;
            else
            {
                scene_operation_ingame = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);

            }
        }
        else
#endif
        {
//             if (!AssetBundleManager.Instance.BundleTable.CheckInBundleTable(scenename))
//             {
//                 useBundle = false;
//             }
//             else
//             {
//                 AssetBundleManager.Instance.LoadBundle("assetdata_scenes");
// 
//                 while (!AssetBundleManager.Instance.IsBundleLoaded("assetdata_scenes"))
//                 {
//                     yield return new WaitForEndOfFrame();
//                 }

                scene_operation_ingame = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenename);
//            }
        }

        if (!useBundle)
        {
            scene_operation_ingame = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenename);

            //if (scene_operation_ingame != null)
            //{
            //    while (scene_operation_ingame.isDone == false)
            //    {
            //        yield return new WaitForEndOfFrame();
            //    }
            //}
        }

        yield return ProcessLoadingGuageUpdate(scene_operation_ingame);

    }

    private IEnumerator ProcessLoadingGuageUpdate(AsyncOperation aop)
    {
        if (aop != null)
        {
            while (!aop.isDone)
            {
                yield return null;
                {
                    if (UIGameLoading.Instance.CurrentLoadingState == UIGameLoading.LoadingState.After_Start)
                    {
                        UIGameLoading.Instance.SetLoadingGuage(aop.progress >= 0.9f ? 1f : aop.progress);
                    }
                }
            }

            aop = null;
        }
        yield break;
    }


    private IEnumerator ProcessLoadAddScene(string scenename)
    {
        bool useBundle = true;
        AsyncOperation scene_operation_ingame = null;

        // data 용 scene load
        if (string.IsNullOrEmpty(scenename))
            yield break;

        useBundle = true;

#if UNITY_EDITOR
        if (AssetBundleManager.SimulateAssetBundleInEditor)
        {
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName("assetdata_scenes", scenename);

            if (levelPaths.Length == 0)
                useBundle = false;
            else
                scene_operation_ingame = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(levelPaths[0]);
        }
        else
#endif
        {
//             if (!AssetBundleManager.Instance.BundleTable.CheckInBundleTable(scenename))
//             {
//                 useBundle = false;
//             }
//             else
//             {
//                 AssetBundleManager.Instance.LoadBundle("assetdata_scenes");
// 
//                 while (!AssetBundleManager.Instance.IsBundleLoaded("assetdata_scenes"))
//                 {
//                     yield return new WaitForEndOfFrame();
//                 }

                scene_operation_ingame = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Additive);
//            }
        }

        if (!useBundle)
        {
            scene_operation_ingame = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Additive);

            //if (scene_operation_ingame != null)
            //{
            //    while (scene_operation_ingame.isDone == false)
            //    {
            //        yield return new WaitForEndOfFrame();
            //    }
            //}
        }

        yield return ProcessLoadingGuageUpdate(scene_operation_ingame);
    }

    private IEnumerator ProcessLoad(string sceneName, string addsceneName, OnLoadDoneDelegate func = null)
    {
        this.SceneName = sceneName;
        this.AddSceneName = addsceneName;

        // loading 화면 fade in
        yield return StartTransition();
        yield return ProcessLoadEmptyScene();

        // empty_scene 로드 후 memory clear
        if (func != null)
            func();

        yield return PrepeardSceneLoad(sceneName);

        MyNetworkLobbyPlayer player = MyNetworkManager.Instance.MyLobbyPlayer;
        if (player == null || player.m_loadComplete == MyNetworkLobbyPlayer.SceneLoadState.None)
        {
            yield return ProcessLoadScene(sceneName);
        }
        else
        {
            while (true)
            {
                if (player.m_loadComplete == MyNetworkLobbyPlayer.SceneLoadState.Done)
                {
                    break;
                }

                if (player.m_loadComplete == MyNetworkLobbyPlayer.SceneLoadState.Wait)
                {
                    player.Cmd_OnLoadSceneState(MyNetworkLobbyPlayer.SceneLoadState.Load);
                    yield return null;
                }
                else if (player.m_loadComplete == MyNetworkLobbyPlayer.SceneLoadState.Load)
                {
                    if (MyNetworkManager.Instance.IsHost)
                        MyNetworkManager.Instance.ServerChangeScene(SceneName);

                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
        }

        if (!string.IsNullOrEmpty(addsceneName))
        {
            yield return PrepeardSceneLoad(addsceneName);
            yield return ProcessLoadAddScene(addsceneName);
        }

        yield break;
    }

    private IEnumerator ProcessLoadAdd(string sceneName, OnLoadDoneDelegate func = null)
    {
        yield return StartTransition();
        if (func != null)
            func();

        yield return PrepeardSceneLoad(sceneName);
        yield return ProcessLoadAddScene(sceneName);
        yield break;
    }

    public bool LoadScene(string sceneName, string addsceneName = "", OnLoadDoneDelegate func = null, bool isaddscene = false, bool uncondition = false)
    {
        if (uncondition == false &&
            string.Compare(this.SceneName, sceneName) == 0)
        {
            return false;
        }

        if (isaddscene == false)
            StartCoroutine(ProcessLoad(sceneName, addsceneName, func));
        else
            StartCoroutine(ProcessLoadAdd(sceneName, func));

        return true;
    }

    public string GetSceneStateToName(SceneState state)
    {
        switch (state)
        {
            case SceneState.Start:
                return SceneName_Start;

            case SceneState.Lobby:
                return SceneName_Lobby;

            case SceneState.Arcade:
                return SceneName_InGame;

            default:
                return string.Empty;
        }
    }
    /*
        public void InitRoom(string sceneName, string addsceneName = "", OnLoadDoneDelegate func = null)
        {
            this.SceneName = "load_battle_scene";

            LoadScene(sceneName, addsceneName, func);
        }

        public void NextRoom(string sceneName, string addsceneName = "", OnLoadDoneDelegate func = null)
        {
            this.SceneName = "load_battle_scene";

            inToin = true;
            LoadScene(sceneName, addsceneName, func);
        }
        */
    //         public bool ChangeScene(string sceneName, string addsceneName = "", OnLoadDoneDelegate func = null)
    //         {
    //             return LoadScene(sceneName, addsceneName, func);
    //         }

    private void LoadCompleteEmptyScene()
    {
        ResourceManager.Instance.ClearCache();
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        SoundManager.Instance.ClearSound();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name + ", LoadSceneMode:" + mode.ToString());

        if (string.Equals(scene.name, SceneName_EMPTY))
        {
            LoadCompleteEmptyScene();
            return;
        }

        if (OnLoadDoneFunc != null)
        {
            OnLoadDoneFunc();
            OnLoadDoneFunc = null;
        }

        inToin = false;

        StartCoroutine(EndTransition());
    }

    private IEnumerator StartTransition()
    {
        yield return UIGameLoading.Instance.BeforeStartLoading();

        if (IsShowLoadingGuageScene())
        {
            while (true)
            {
                if (UIGameLoading.Instance.CurrentLoadingState != UIGameLoading.LoadingState.Before_Start)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                break;
            }

            if (!inToin)
                UIGameLoading.Instance.StartLoading();

            yield return UIGameLoading.Instance.AfterStartLoading();

            while (true)
            {
                if (UIGameLoading.Instance.CurrentLoadingState != UIGameLoading.LoadingState.After_Start)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                break;
            }

            //if(!inToin)
            //    yield return UIGameLoading.Instance.StartGage();
        }
    }

    private IEnumerator EndTransition()
    {
        yield return new WaitForSeconds(0.1f);

        MyNetworkLobbyPlayer player = MyNetworkManager.Instance.MyLobbyPlayer;
        while (true)
        {
            if (player == null || player.m_loadComplete == MyNetworkLobbyPlayer.SceneLoadState.None)
                break;

            if (player.m_loadComplete != MyNetworkLobbyPlayer.SceneLoadState.Done)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }
            else
            {
                //                player.Cmd_OnLoadSceneState(MyNetworkLobbyPlayer.SceneLoadState.None);
                break;
            }
        }

        if (IsShowLoadingGuageScene())
        {
            yield return UIGameLoading.Instance.BeforeEndLoading();

            while (true)
            {
                if (UIGameLoading.Instance.CurrentLoadingState != UIGameLoading.LoadingState.Before_End)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                break;
            }

            UIGameLoading.Instance.EndLoading();
        }

        yield return UIGameLoading.Instance.AfterEndLoading();

        while (true)
        {
            if (UIGameLoading.Instance.CurrentLoadingState != UIGameLoading.LoadingState.After_End)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            break;
        }
    }
}