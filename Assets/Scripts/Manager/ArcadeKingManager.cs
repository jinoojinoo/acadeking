using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArcadeKingManager : MonoBehaviour
{
    private static ArcadeKingManager m_instance = null;

    public static ArcadeKingManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject obj = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.ArcadeManager);
                m_instance = obj.GetComponent<ArcadeKingManager>();
            }

            return m_instance;
        }
    }

    public static bool IsCreate()
    {
        return m_instance != null;
    }
    private int m_round = 0;
    public int CurrentRound
    {
        get
        {
            return m_round;
        }
        private set
        {
            Debug.LogError("value : " + value);
            m_round = value;
            if (GameRoundFunc != null)
                GameRoundFunc(value);

            m_settingproperty = null;
            m_gamestartTime = -1;
            m_gameprogressTime = -1;
            m_roundStartTime = -1;
            m_roundprogressTime = 0;
            m_roundpauseTime = 0;

            if (m_round != 0)
                InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayWaitForNextRound;
        }
    }

    public void StartNextRound(double time)
    {
        Debug.LogError("time : " + time + " , NetworkTimeManager.Instance.GameDeletaTime : " + NetworkTimeManager.Instance.GameDeletaTime);
        m_gamestartTime = time;
        if (m_round > GetMaxRound())
        {
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.EndGame;
            return;
        }
        InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayCount;
    }

    public void Reset()
    {
        CurrentRound = 0;
    }

    private System.Action<int, bool> m_gamescoreFunc = null;
    public System.Action<int, bool> GameScoreFunc
    {
        get
        {
            return m_gamescoreFunc;
        }
        set
        {
            m_gamescoreFunc = value;
            if (m_gamescoreFunc != null)
                m_gamescoreFunc(GameScore, false);
        }
    }

    private System.Action m_multiegamescoreFunc = null;
    public System.Action MultieGameScoreFunc
    {
        get
        {
            return m_multiegamescoreFunc;
        }
        set
        {
            m_multiegamescoreFunc = value;
            if (m_multiegamescoreFunc != null)
                m_multiegamescoreFunc();
        }
    }

    private System.Action<int> m_gameroundFunc = null;
    public System.Action<int> GameRoundFunc
    {
        get
        {
            return m_gameroundFunc;
        }
        set
        {
            m_gameroundFunc = value;
            if (m_gameroundFunc != null)
                m_gameroundFunc(m_round);
        }
    }

    DelegateSecrueProperty<int> m_encryption_score = new DelegateSecrueProperty<int>();
//    private int m_score = 0;
    public int GameScore
    {
        get
        {
            return m_encryption_score.Value;
        }
    }

    public void AddScore(int score, bool isclean)
    {
        m_encryption_score.Value += score * (isclean ? 2 : 1);
        if (GameScoreFunc != null)
            GameScoreFunc(m_encryption_score.Value, isclean);
    }

/*    private void Start()
    {
        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
        {
            CreateBasketBall(0, AccountManager.Instance.MyBallList, true);
        }
        else
        {
            foreach (MyNetworkGamePlayer player in MyNetworkManager.Instance.InGamePlayerList.Values)
                CreateBasketBall((int)player.LobbyNetID, null, player == MyNetworkManager.Instance.MyGamePlayer);
        }
    }*/

    private Dictionary<int, GameObject> m_arcadeobjList = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> m_arcadecameraobjList = new Dictionary<int, GameObject>();
    private Dictionary<int, List<GameObject>> m_aracdeballobjList = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, RenderTexture> m_rendertextureList = new Dictionary<int, RenderTexture>();

    private Dictionary<KeyValuePair<int, int>, Ball> m_balllist = new Dictionary<KeyValuePair<int, int>, Ball>();
    public Ball GetBall(int netid, int ballindex)
    {
        KeyValuePair<int, int> key = new KeyValuePair<int, int>(netid, ballindex);
        if (m_balllist.ContainsKey(key) == false)
            return null;

        return m_balllist[key];
    }

    private Shader m_outlineballShader = null;
    private Shader OutLineBallShader
    {
        get
        {
            return
                null;
        }
    }

    private Arcade m_myArcade = null;
    public Arcade MyAracde
    {
        get
        {
            return m_myArcade;
        }
    }
    public void CreateBasketBall(int netid, List<BallInfos> list, List<float> ballpositions, bool islocal)
    {
        if (list == null || list.Count <= 0)
        {
            list = new List<BallInfos>();
            list.Add(new BallInfos(0));
            list.Add(new BallInfos(1));
            list.Add(new BallInfos(2));
        }

//        GameObject target = null;
//         UI_InGameScene scene = UI_InGameScene.Instance as UI_InGameScene;
//         target = islocal ? scene.MyBasketballAracade : scene.OtherBasketballAracade;
//         target.SetActive(true);

        GameObject target = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.ArcadeNew);
        target.transform.position = new Vector3(50.0f * (islocal ? 0 : 1), -5.7f, 0.0f);
        m_arcadeobjList.Add(netid, target);

        Arcade myarcade = target.GetComponent<Arcade>();
        myarcade.ArcadeIndex = netid;
        if (islocal)
            m_myArcade = myarcade;

        int count = 0;
        foreach (BallInfos info in list)
        {
            BallType_DataProperty property = BallType_Table.Instance.GetBallTypeProperty(info.BallType);
            GameObject obj = ResourceManager.Instance.LoadResourceObject(property.Path);
            Ball ball = obj.GetComponent<Ball>();
            if (ball == null)
                ball = obj.AddComponent<Ball>();

            ball.MyNetId = (uint)netid;
            ball.BallType = info.BallType;
            ball.BallScore = info.Score;
            ball.BallNumber = count;
            ball.IsLocalBall = islocal;
            ball.BallTypeProperty = property;

            if (m_aracdeballobjList.ContainsKey(netid))
            {
                m_aracdeballobjList[netid].Add(obj);
            }
            else
            {
                List<GameObject> objlist = new List<GameObject>();
                objlist.Add(obj);
                m_aracdeballobjList.Add(netid, objlist);
            }

            Vector3 pos = Vector3.zero;
            if (ballpositions == null)
                pos.x += UnityEngine.Random.Range(-1.0f, 1.0f);
            else
            {
                int ballpos = count % ballpositions.Count;
                pos.x += ballpositions[ballpos];
            }

            obj.transform.SetParent(myarcade.CreateBallPosition.transform);
            obj.transform.localPosition = pos;

            m_balllist.Add(new KeyValuePair<int, int>(netid, count), ball);
            count++;
        }

        if (islocal)
            m_myArcade.InitClothCollider(m_balllist.Values.ToList<Ball>());

        GameObject cameratarget = null;
        if (islocal)
        {
            cameratarget = Camera.main.gameObject;
//             CameraRevolution revoloution = cameratarget.GetComponent<CameraRevolution>();
//             if (revoloution == null)
//                 cameratarget.AddComponent<CameraRevolution>();
        }
        else
        {
            cameratarget = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.Arcade_Camera);
            m_arcadecameraobjList.Add(netid, cameratarget);
        }

//         MobileMaxCamera maxcamera = cameratarget.GetComponent<MobileMaxCamera>();
//         maxcamera.InitTarget(myarcade.CameraTarget);

        Camera cam = cameratarget.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(95.0f / 255.0f, 105.0f / 255.0f, 115.0f / 255.0f);

        if (islocal == false)
        {
            cam.targetTexture = GetRenderTexture(netid);
        }
        else
        {
            MyCamera = cam;
        }

        cameratarget.transform.position = target.transform.position + GlobalValue_Table.Instance.DefaultCameraPositionVec;
    }

    private Camera m_myCamera = null;
    public Camera MyCamera
    {
        get
        {
            return m_myCamera;
        }
        private set
        {
            m_myCamera = value;
        }
    }
    public RenderTexture GetRenderTexture(int index)
    {
        if (m_rendertextureList.ContainsKey(index))
            return m_rendertextureList[index];

        RenderTexture texture = new RenderTexture(256, 256, 10);
        m_rendertextureList.Add(index, texture);
        return texture;
    }

    private void ClearObjectIndex(int index = -1)
    {
        m_balllist.Clear();
        ClearObjectIndex(m_arcadeobjList, index);
        ClearObjectIndex(m_arcadecameraobjList, index);
        ClearObjectIndex(m_aracdeballobjList, index);
        ClearObjectIndex(m_rendertextureList, index);
    }

    private void ClearObjectIndex<T>(Dictionary<int, T> list, int index) where T : Object
    {
        foreach (int key in list.Keys)
        {
            if (key == index || index == -1)
            {
                T obj = list[key];
                if (obj == null)
                    continue;

                Destroy(obj);
            }
        }
        if (index == -1)
            list.Clear();
        else
            list.Remove(index);
    }

    private void ClearObjectIndex<T>(Dictionary<int, List<T>> list, int index) where T : Object
    {
        foreach (int key in list.Keys)
        {
            if (key == index || index == -1)
            {
                foreach(T obj in list[key])
                {
                    if (obj == null)
                        continue;

                    Destroy(obj);
                }

                list[key].Clear();
            }
        }

        if (index == -1)
            list.Clear();
        else
            list.Remove(index);
    }

    private double m_gamestartTime = -1;
    public double GameStartTime
    {
        get
        {
            return m_gamestartTime;
        }
    }

    private double m_gameprogressTime = -1;
    public double GameProgressTime
    {
        get
        {
            return m_gameprogressTime;
        }
    }

    private double m_roundStartTime = -1;
    private double m_roundprogressTime = 0;
    public double RoundProgressTime
    {
        get
        {
            return m_roundprogressTime;
        }
    }

    private double m_roundpauseTime = 0;

    private RoundSetting_DataProperty m_settingproperty = null;
    public RoundSetting_DataProperty SettingProperty
    {
        get
        {
            if (m_settingproperty == null)
                m_settingproperty = RoundSetting_Table.Instance.GetRoundSettingProperty(CurrentRound);

            return m_settingproperty;
        }
    }

    private void LateUpdate()
    {
        if (UI_InGameScene.Instance != null &&
            UI_InGameScene.Instance.MyGameUIMode != GAME_UI_MODE.InGame)
        {
            return;
        }

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.None)
            return;

        if (InGameManager.Instance.GameState < InGameManager.GAME_STATE.PlayReady ||
            InGameManager.Instance.GameState >= InGameManager.GAME_STATE.PlayWaitForNextRound)
            return;

        if (m_gamestartTime == -1)
        {
            if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Multiple)
            {
                m_gamestartTime = NetworkTimeManager.Instance.GameDeletaTime;
            }
            else
            {
                m_gamestartTime = RealTime.time;
            }
        }

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Multiple)
        {
            m_gameprogressTime = NetworkTimeManager.Instance.GameDeletaTime;
        }
        else
        {
            m_gameprogressTime = RealTime.time;
        }

        if (InGameManager.Instance.GameState == InGameManager.GAME_STATE.Play)
        {
            if (m_roundStartTime == -1)
                m_roundStartTime = m_gameprogressTime;

            m_roundprogressTime = m_gameprogressTime - m_roundStartTime;
        }
        else
            return;

        //        Debug.LogError("m_gamestartTime : " + m_gamestartTime + " , m_gameprogressTime : " + m_gameprogressTime );

        float maxtime = GetRoundMaxTime();
        if (maxtime == 0.0f)
            return;

        if (maxtime + 1 > m_roundprogressTime)
            return;

        if (CheckGameScore() == false)
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.WaitForLastBall;
    }

    public bool CheckGameScore()
    {
        if (GameScore < GetRoundMaxScore())
            return false;

        Debug.LogError("CurrentRound : " + CurrentRound);
        CurrentRound++;
        return true;
    }

    public float GetRoundMaxTime()
    {
        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
            return SettingProperty.Time;

        return GlobalValue_Table.Instance.GetRoundTime(MyNetworkManager.Instance.CreateRoomInfo.MaxTime);
    }

    public float GetRoundMaxScore()
    {
        if (InGameManager.IsCreateInstance() == false)
            return 0;

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
            return SettingProperty.Score;

        return 0;
    }

    private void OnDestroy()
    {
        ClearObjectIndex();
    }

    public int GetMaxRound()
    {
        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
            return RoundSetting_Table.Instance.MaxRound;
        else
            return MyNetworkManager.Instance.CreateRoomInfo.MaxRound;
    }
}
