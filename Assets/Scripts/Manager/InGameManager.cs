using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public abstract class ResetUIComponent : ObjectBase
{
    public abstract InGameManager.GAME_STATE AdjustGameState
    {
        get;
    }

    public abstract int Order
    {
        get;
    }

    protected virtual void Awake()
    {
        if (InGameManager.Instance == null)
            return;

        InGameManager.Instance.AddResetComponent(this);
    }

    protected virtual void OnDestroy()
    {
        if (InGameManager.IsCreateInstance() == false)
            return;

        InGameManager.Instance.RemoveResetComponent(this);
    }

    public abstract void ResetComponent(bool reset);
}

public class InGameManager : SingletonMono<InGameManager>
{
#if UNITY_EDITOR
    static int m_clientOnlyPlay = -1;
    const string kSimulatorAssetBundles = "SimulateAssetBundles";

    public static bool ClientOnlyPlay
    {
        get
        {
            if (m_clientOnlyPlay == -1)
                m_clientOnlyPlay = UnityEditor.EditorPrefs.GetBool(kSimulatorAssetBundles, true) ? 1 : 0;

            return m_clientOnlyPlay != 0;
        }
        set
        {
            int newValue = value ? 1 : 0;

            if (newValue != m_clientOnlyPlay)
            {
                m_clientOnlyPlay = newValue;
                UnityEditor.EditorPrefs.SetBool(kSimulatorAssetBundles, value);
            }
        }
    }

#endif

    public static bool GAME_PAUSE = false;   
    public void AddScore()
    {
///        m_gameBonusTime += 5.0f;
        CurrentScore += 5.0f;
    }

    public const float MIN_SCALE = 0.5f;
    public const float MAX_SCALE = 5.0f;

    private System.Action<float> m_scoreFunc = null;
    public System.Action<float> ScoreFunc
    {
        set
        {
            m_scoreFunc = value;
            if (m_scoreFunc != null)
                m_scoreFunc(m_currentScore);
        }
        get
        {
            return m_scoreFunc;
        }
    }

    private float m_currentScore = 0.0f;
    public float CurrentScore
    {
        get
        {
            return m_currentScore;
        }
        set
        {
            m_currentScore = value;
            if (m_scoreFunc != null)
                m_scoreFunc(value);
        }
    }

    public enum GAME_STATE
    {
        Start = 1 << 0,
        InGame = 1 << 1,
        Room = 1 << 2,

        PlayReady = 1 << 3,
        PlayCount = 1 << 4,
        Play = 1 << 5,

        PlayWaitForNextRound = 1 << 6,
        WaitForLastBall = 1 << 7,
        EndGame = 1 << 8,

        ReturnToLobbyFromResult = 1 << 9,
        ReturnToLobby = 1 << 10,
        ALL = (1 << 11) - 1,
        Delay,
    }

    private GAME_STATE m_gameState = GAME_STATE.Start;
    public GAME_STATE GameState
    {
        set
        {
            m_gameState = value;
            Debug.LogError("gamestate : " + value);

            if (StateFunc.ContainsKey(value))
                StateFunc[value]();            
            ResetGameComponent();
        }
        get
        {
            return m_gameState;
        }
    }

    private Dictionary<GAME_STATE, System.Action> m_stateFunc = null;
    private Dictionary<GAME_STATE, System.Action> StateFunc
    {
        get
        {
            if (m_stateFunc == null)
            {
                m_stateFunc = new Dictionary<GAME_STATE, System.Action>();

                m_stateFunc.Add(GAME_STATE.Start, Start_State);
                m_stateFunc.Add(GAME_STATE.InGame, InGame_State);
               
                m_stateFunc.Add(GAME_STATE.Play, InGame_Play);
                m_stateFunc.Add(GAME_STATE.PlayCount, InGame_PlayCount);
                m_stateFunc.Add(GAME_STATE.PlayWaitForNextRound, PlayWaitForNextRound_State);
                m_stateFunc.Add(GAME_STATE.WaitForLastBall, WaitForLastBall_State);
                
                m_stateFunc.Add(GAME_STATE.EndGame, EndGame_State);               
            }

            return m_stateFunc;
        }
    }

    private int m_myheroIndex = 0;
    public int MyHeroIndex
    {
        get
        {
            return m_myheroIndex;
        }
    }

    private void Start_State()
    {
//         AdMobManager.Instance.Init();
//         AdMobManager.Instance.ShowBanner();
    }

    private void InGame_State()
    {
        //         m_startgameTime = Time.realtimeSinceStartup;
        //         m_gamedeltaTime = 0.0f;

        InGameManager.GAME_PAUSE = true;
    }

    public enum PLAY_MODE
    {
        None,
        Single,
        Multiple,
        TEST,
    }

    private PLAY_MODE m_currentplayMode = PLAY_MODE.None;
    public PLAY_MODE CurrentPlayMode
    {
        set
        {
            Debug.LogError("m_currentplayMode : " + value);
            m_currentplayMode = value;
        }
        get
        {
            return m_currentplayMode;
        }
    }

    private void InGame_Play()
    {
        InGameManager.GAME_PAUSE = false;
    }

    private void InGame_PlayCount()
    {

    }

    private void WaitForLastBall_State()
    {
        StopCoroutine("WaitForLastBall");
        StartCoroutine(
            WaitForLastBall(() =>
        {
            if (ArcadeKingManager.Instance.CheckGameScore() == false)
                InGameManager.Instance.GameState = GAME_STATE.EndGame;
        }
        ));
    }

    private IEnumerator WaitForLastBall(System.Action func)
    {
        while (CatchBall.Instance.LeftBallList.Count > 0)
        {
            yield return null;
            continue;
        }

        func();
        yield break;
    }

    private void PlayWaitForNextRound_State()
    {
        StopCoroutine("WaitForLastBall");
        StartCoroutine(WaitForLastBall(() =>
        {
            int maxround = ArcadeKingManager.Instance.GetMaxRound();
            int curentround = ArcadeKingManager.Instance.CurrentRound;
            Debug.LogError("maxround : " + maxround + " , currentround : " + curentround);

            if (maxround >= 1 && curentround <= maxround)
            {
                if (CurrentPlayMode == PLAY_MODE.Multiple)
                    MyNetworkManager.Instance.MyGamePlayer.Cmd_NextRound();
                else
                    GameState = GAME_STATE.PlayCount;
            }
            else
            {
                if (CurrentPlayMode == PLAY_MODE.Multiple)
                    MyNetworkManager.Instance.MyGamePlayer.Cmd_NextRound();
                else
                    InGameManager.Instance.GameState = InGameManager.GAME_STATE.EndGame;
            }
        }));
    }

    private void EndGame_State()
    {
        InGameManager.GAME_PAUSE = true;
        PopupManager.Instance.AllClearPopup();
        UI_GameResult.ShowGameResult();
    }

    private void EndGameDelay_State()
    {
        EndGame_State();
    }

    private void Click_OK(params object[] parameters)
    {
        InGameUIScene.Instance.ChangeScene(SceneManager.SceneName_Start);
    }

    private void Click_Continue(params object[] parameters)
    {
        GameState = GAME_STATE.Play;
    }

    private List<ResetUIComponent> resetcomponentList = new List<ResetUIComponent>();
    public void AddResetComponent(ResetUIComponent comp)
    {
        resetcomponentList.Add(comp);
        resetcomponentList.Sort((x, y) => x.Order - y.Order);

        comp.ResetComponent(false);
    }
    public void RemoveResetComponent(ResetUIComponent comp)
    {
        resetcomponentList.Remove(comp);
    }

    private void ResetGameComponent()
    {
        Debug.LogError("resetcomponentList : " + resetcomponentList.Count);
        foreach (ResetUIComponent comp in resetcomponentList)
        {
            bool reset = !((comp.AdjustGameState & GameState) == 0);
            comp.ResetComponent(reset);
        }
    }

    private ScreenBounds m_screenBounds = null;
    public ScreenBounds MyScreenBounds
    {
        get
        {
            if (m_screenBounds == null)
            {
                m_screenBounds = new ScreenBounds();

                float vertExtent = InGameUIScene.Instance.CurrentCamera.orthographicSize;
                float horzExtent = vertExtent * Screen.width / Screen.height;

                float sizex = 0;// GlobalValue_Table.Instance.MAP_SIZE_X;
                float sizey = 0;// GlobalValue_Table.Instance.MAP_SIZE_Y;

                m_screenBounds.leftBound = (float)(horzExtent - sizex / 2.0f);
                m_screenBounds.rightBound = (float)(sizex / 2.0f - horzExtent);
                m_screenBounds.bottomBound = (float)(vertExtent - sizey / 2.0f);
                m_screenBounds.topBound = (float)(sizey / 2.0f - vertExtent);
            }

            return m_screenBounds;
        }
    }

#if UNITY_EDITOR
    private int pickupitemindex = 48;        
    private void LateUpdate()
    {/*
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            pickupitemindex = 48;
            TargetManager.Instance.GetHero().MyNetworkPlayer.ClientMode_Pikupitem(pickupitemindex);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            TargetManager.Instance.GetHero().MyNetworkPlayer.ClientMode_Pikupitem(++pickupitemindex);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            TargetManager.Instance.GetHero().MyNetworkPlayer.ClientMode_Pikupitem(--pickupitemindex);
        }*/
    }
#endif

    public void SignInAuto(System.Action signinok)
    {
        GoogleGamesManager.Instance.SignInAuto();
        StopCoroutine("WaitForSignIn");
        StartCoroutine("WaitForSignIn", signinok);
    }

    IEnumerator WaitForSignIn(System.Action signinok)
    {
        while (GoogleGamesManager.Instance.IsSignIn() == false)
        {
            yield return null;
        }

        bool isloadcloud = false;
        PopupBase popup = PopupManager.Instance.ShowPopup(POPUP_TYPE.Notice_WaitForLogin);
        GoogleGamesManager.Instance.LoadFromCloud((islogin) =>
        {
            popup.OnClick_Close();
            isloadcloud = true;
        });

        while (isloadcloud == false)
        {
            yield return null;
        }

        if (signinok != null)
            signinok();

        yield break;
    }
}


public class ScreenBounds
{
    public float rightBound;
    public float leftBound;
    public float topBound;
    public float bottomBound;
}