using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Arcade : MonoBehaviour
{
    public int ArcadeIndex;
    public BallPosition CreateBallPosition;

    public TMP_Text GoalTextPro;
    public TMP_Text RecordTextPro;
    public TMP_Text ScoreTextPro;
    public TMP_Text TimeTextPro;
    public GameObject ClearShotObject;

    public Transform CameraTarget;

    public Cloth m_netCloth;         

    private int m_record = -1;
    private int MyRecord
    {
        set
        {
            if (m_record >= value)
                return;

            m_record = value;
            SetDisitalRecord(RecordTextPro, value);
        }
    }

    private int m_currentScore = 0;
    private int CurrentScore
    {
        set
        {
            if (m_currentScore > value)
                return;

//            Goal = value - m_currentScore;
            m_currentScore = value;
            MyRecord = value;
            SetDisitalRecord(ScoreTextPro, value);
        }
        get
        {
            return m_currentScore;
        }
    }

    private void SetDisitalRecord(TMP_Text textpro, int count)
    {
        textpro.text = string.Format("{0:000}", count > 999 ? 999 : count);
    }

    IEnumerator ViewGoaltextPro()
    {
        yield return new WaitForSeconds(1.0f);
        GoalTextPro.gameObject.SetActive(false);
        yield break;
    }

    private void Awake()
    {
        GoalTextPro.color = Color.red;
        RecordTextPro.color = Color.red;
        ScoreTextPro.color = Color.red;
        TimeTextPro.color = Color.green;
    }

    private void Start()
    {
        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.None)
        {
            SetGameScore(0, false);
            MyRecord = AccountManager.Instance.MyAccountInfo.MyScore;
            return;
        }

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single ||
            InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.TEST)
        {
            ArcadeKingManager.Instance.GameScoreFunc -= SetGameScore;
            ArcadeKingManager.Instance.GameScoreFunc += SetGameScore;
            MyRecord = AccountManager.Instance.MyAccountInfo.MyScore;
        }
        else
        {
            ArcadeKingManager.Instance.MultieGameScoreFunc -= SetMultieScore;
            ArcadeKingManager.Instance.MultieGameScoreFunc += SetMultieScore;

            MyNetworkLobbyPlayer player = MyNetworkManager.Instance.ConnectedPlayerList.Find(x => x.PlayerID == (uint)ArcadeIndex);
            MyRecord = player.RecordScore;
        }

        SetTime(0);
    }    

    private void OnDestroy()
    {
        if (ArcadeKingManager.IsCreate() == false)
            return;

        Debug.LogError("destroy arcade");
        ArcadeKingManager.Instance.GameScoreFunc -= SetGameScore;
        ArcadeKingManager.Instance.MultieGameScoreFunc -= SetMultieScore;
    }

    private void SetGameScore(int score, bool isclean)
    {
        SetGoal(score - CurrentScore, isclean);
        CurrentScore = score;
    }

    private void SetGoal(int goal, bool clearshot)
    {
        GoalTextPro.gameObject.SetActive(goal > 0);
        ClearShotObject.SetActive(goal > 0 && clearshot);

        SetDisitalRecord(GoalTextPro, goal);
        StopCoroutine("ViewGoaltextPro");
        StartCoroutine("ViewGoaltextPro");
    }

    private void SetMultieScore()
    {
        if (MyNetworkManager.Instance.InGamePlayerList.ContainsKey((uint)ArcadeIndex) == false)
            return;

        int score = MyNetworkManager.Instance.InGamePlayerList[(uint)ArcadeIndex].GameScore;
        bool isclean = MyNetworkManager.Instance.InGamePlayerList[(uint)ArcadeIndex].IsClean;

        SetGoal(score - CurrentScore, isclean);
        CurrentScore = score;
    }

    private int m_oldtime = -1;
    private void LateUpdate()
    {
        if (InGameManager.Instance.GameState != InGameManager.GAME_STATE.Play &&
            InGameManager.Instance.GameState != InGameManager.GAME_STATE.PlayCount)
            return;

        int time = (int)(ArcadeKingManager.Instance.RoundProgressTime);
        if (m_oldtime == time)
            return;

        m_oldtime = time;
        SetTime(time);
    }

    private void SetTime(float time)
    {
        time = ArcadeKingManager.Instance.GetRoundMaxTime() - time;
        if (time < 0)
            return;

        TimeTextPro.text = string.Format("{0:00}", (int)(time));
        TimeTextPro.color = GetTimeColor(time);
    }

    private Color GetTimeColor(float time)
    {
        return time > ArcadeKingManager.Instance.SettingProperty.Time * 0.1f ? Color.green : Color.red;
    }

    [ContextMenu("CastOn")]
    public void OnCast()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer render in renderers)
        {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            render.receiveShadows = false;
            render.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            render.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            render.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }
    }

    private void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    public void InitClothCollider(List<Ball> balls)
    {
        ClothSphereColliderPair[] pairs = new ClothSphereColliderPair[balls.Count];
        for (int i = 0; i < balls.Count;++i)
        {
            SphereCollider collider = balls[i].MyCollider as SphereCollider;
            pairs[i] = new ClothSphereColliderPair(collider);
        }

        m_netCloth.sphereColliders = pairs;
    }
}
