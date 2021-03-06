using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class NetworkTimeManager : NetworkBehaviour
{
    private static NetworkTimeManager m_instance = null;
    public static NetworkTimeManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    public static bool IsCreateInstance()
    {
        return m_instance != null;
    }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
        {
            m_instance = null;
        }
    }

    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////

    private Popup_Notice_WaitForIngame m_noticePopup;
    //    [ServerCallback]
    private void Start()
    {
        ResetTimerInfo(0);

        m_noticePopup = PopupManager.Instance.ShowPopup(POPUP_TYPE.Notice_WaitInGame) as Popup_Notice_WaitForIngame;
        m_noticePopup.InitPopup(isServer);
        if (isServer)
        {
            m_noticePopup.OkFunc = OnClickOK_InGame;
        }

        MyNetworkManager.Instance.MyGamePlayer.Cmd_ReadyForGame();
        InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayReady;
    }

    private void OnClickOK_InGame(params object[] paramters)
    {
        m_serverstartgameTime = MyNetworkManager.Instance.SyncServerTime;
    }

    public void NextRound()
    {
        if (isServer == false)
            return;

        m_servernextroundTime = GameDeletaTime;
    }

    [SyncVar(hook = "OnServerNextRoundTime")]
    public double m_servernextroundTime = double.MinValue;
    private void OnServerNextRoundTime(double time)
    {
        m_servernextroundTime = time;
        ArcadeKingManager.Instance.StartNextRound(time);
    }

    [SyncVar(hook = "OnServerStartGameTime")]
    public double m_serverstartgameTime = double.MinValue;
    private void OnServerStartGameTime(double time)
    {
        m_serverstartgameTime = time;
        if (m_noticePopup != null)
            m_noticePopup.OnClick_Close();

        ResetTimerInfo(m_serverstartgameTime);
//        Debug.LogError("time : " + time + " , MyNetworkManager.Instance.SyncServerTime : " + MyNetworkManager.Instance.SyncServerTime);
        InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayCount;
    }

    private void ResetTimerInfo(double time)
    {
        m_lastupdateTime = time;
        m_dayCount = 0;
        m_dailyTime = 8;
        m_progressTime = m_dailyTime;
    }

    public const float GAME_TOTAL_TIME = 120.0f;

    private float m_gamedeltaTime = float.MinValue;
    public float GameDeletaTime
    {
        get
        {
            if (m_serverstartgameTime == double.MinValue)
                return -1;

            double servertime = MyNetworkManager.Instance.SyncServerTime;
            m_gamedeltaTime = (float)(servertime - m_serverstartgameTime);
            return m_gamedeltaTime;
        }
    }

    public float RemainGamePlayTime
    {
        get
        {
            return GAME_TOTAL_TIME - GameDeletaTime + m_gameBonusTime;
        }
    }

    private float m_dailyTime = 8;
    public float DailyTime
    {
        private set
        {
            m_dailyTime = value;
            if (m_dailyTime > DAILY_TOTAL_TIME)
                m_dailyTime -= DAILY_TOTAL_TIME;
        }
        get
        {
            return m_dailyTime;
        }
    }

    private int m_dayCount = 0;
    public int DayCount
    {
        get
        {
            return m_dayCount;
        }
        private set
        {
            m_dayCount = value;
        }
    }

    private const float NIGHT_TIME_START = 22;
    private const float NIGHT_TIME_END = 8;
    private const float DAILY_TOTAL_TIME = 24;
    private float NIGHT_TIME
    {
        get
        {
            float time = NIGHT_TIME_END - NIGHT_TIME_START;
            if (time < 0.0f)
                time = DAILY_TOTAL_TIME + time;

            return time;
        }
    }

    private const float DailyAlpha_Speed = 10.0f;
    public float DailyAlpha
    {
        get
        {
            if (m_dailyTime >= NIGHT_TIME_START || m_dailyTime <= NIGHT_TIME_END)
            {
                float duration = m_dailyTime - NIGHT_TIME_START;
                if (duration < 0.0f)
                {
                    duration = DAILY_TOTAL_TIME + duration;
                }

                float nightalpha = 0.0f;
                if (duration < NIGHT_TIME * 0.5f)
                    nightalpha = duration / (NIGHT_TIME * 0.5f) * DailyAlpha_Speed;
                else
                    nightalpha = Mathf.Abs(NIGHT_TIME - duration) / (NIGHT_TIME * 0.5f) * DailyAlpha_Speed;

                if (m_dailyTime <= NIGHT_TIME_END && nightalpha < 1.0f)
                    IsDay = true;
                else
                    IsDay = false;
                //                Debug.LogError("nightalpha : " + nightalpha);
                return nightalpha;
            }

            IsDay = true;
            return 0.0f;
        }
    }

    private float m_gameBonusTime = 0.0f;

    public static System.Action<bool> InstanceDayFuns = null;

    private System.Action<bool> m_dayFunc = null;
    public System.Action<bool> DayFunc
    {
        get
        {
            return m_dayFunc;
        }
        set
        {
            m_dayFunc += value;
            if (m_dayFunc != null)
                m_dayFunc(IsDay);
        }
    }

    private bool m_isDay = false;
    public bool IsDay
    {
        get
        {
            return m_isDay;
        }
        private set
        {
            if (m_isDay == value)
                return;

            m_isDay = value;
            if (DayFunc != null)
                DayFunc(value);
        }
    }


    private float m_daytimeSpeed = 0.5f;
    private double m_lastupdateTime = -1;
    private double m_progressTime = 0;
    public double ProgressTime
    {
        get
        {
            return m_progressTime;
        }
    }

    private void LateUpdate()
    {
        if (m_serverstartgameTime == double.MinValue ||
            InGameManager.Instance.GameState != InGameManager.GAME_STATE.Play)
            return;

        double servertime = MyNetworkManager.Instance.SyncServerTime;
        double progresstime = servertime - m_lastupdateTime;
        progresstime *= m_daytimeSpeed;
        m_lastupdateTime = servertime;

        m_progressTime += progresstime;
        DailyTime += (float)(progresstime);
        DayCount = (int)(m_progressTime / 24);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (InstanceDayFuns != null)
        {
            DayFunc += InstanceDayFuns;
            InstanceDayFuns = null;
        }

        m_dayCount = 0;
        DailyTime = 0;
    }
}
