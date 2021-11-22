using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class UI_LobbyScene : InGameUIScene
{
    private Transform m_ballpositionTrans = null;
    private Transform BallPositionTrans
    {
        get
        {
            if (m_ballpositionTrans == null)
            {
                BallPosition position = FindObjectOfType(typeof(BallPosition)) as BallPosition;
                m_ballpositionTrans = position.transform;
            }
            return m_ballpositionTrans;
        }
    }

    public UILabel SoloPlayLabel;
    public Arcade LobbyAracade;

    public UI_LobbyScene()
    {
        IsRootUI = true;
    }

    void Start()
    {
        GameUIManager.Instance.PushSequence(this.gameObject, GAME_UI_MODE.UI_Lobby, false);
    }

    private const int BALL_COUNT = 5;
    private List<GameObject> m_ballList = new List<GameObject>();

    public override void StartGameSequence(int option)
    {
        UnityAdsManager.Instance.ShowBanner();
        InGameManager.Instance.CurrentPlayMode = InGameManager.PLAY_MODE.None;
        SoundManager.Instance.PlaySound(UISOUND_ID.BGM_Start, true);
        CreateTestBall();

        SoloPlayLabel.text = string.Format(StringTable.SOLO_STR, AccountManager.Instance.MyAccountInfo.MyScore);

        if (PlayerPrefabsID.IsGameModeSelecting() == false)
        {
            PopupManager.Instance.ShowPopup(POPUP_TYPE.GameModeSelect, SelectModeTouch, SelectModeGauge);
        }
    }

    private void SelectModeGauge(params object[] parameters)
    {
        PlayerPrefabsID.SelectGameMode();
        AccountManager.Instance.IsMouseMode = false;
        AccountManager.Instance.MouseStrength = GlobalValue_Table.Instance.ThrowMouseWeight;
        AccountManager.Instance.SaveGameOption();
    }

    private void SelectModeTouch(params object[] parameters)
    {
        PlayerPrefabsID.SelectGameMode();
        AccountManager.Instance.IsMouseMode = true;
        AccountManager.Instance.MouseStrength = GlobalValue_Table.Instance.ThrowMouseWeight;
        AccountManager.Instance.SaveGameOption();
    }

    private void CreateTestBall()
    {
        if (m_ballList.Count > 0)
            return;

        List<Ball> balls = new List<Ball>();
        for (int i = 0; i < BALL_COUNT; ++i)
        {
            BallType_DataProperty info = BallType_Table.Instance.GetBallTypePropertyRandom();

            GameObject obj = ResourceManager.Instance.LoadResourceObject(info.Path);
            Ball ball = obj.GetComponent<Ball>();
            if (ball == null)
                ball = obj.AddComponent<Ball>();

            ball.BallType = info.TYPE;
            ball.TestBallNumber = i;
            ball.BallTypeProperty = info;

            obj.transform.position = BallPositionTrans.position;
            m_ballList.Add(obj);

            balls.Add(ball);
        }

        LobbyAracade.InitClothCollider(balls);
    }

    public override void EndGameSequenceForRoot()
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach(GameObject obj in m_ballList)
        {
            Destroy(obj);
        }
        m_ballList.Clear();
    }

    protected override void ActionAndroidBackKey()
    {
        PopupManager.Instance.ShowPopup(POPUP_TYPE.GameEnd, ActionAndroidBackKey_OK, null);
    }

    private void ActionAndroidBackKey_OK(params object[] parameters)
    {
        Application.Quit();
    }

    public void OnClick_Leader1()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.beep2);

        if (GoogleGamesManager.Instance.IsSignIn() == false)
        {
            PopupManager.Instance.ShowPopup(POPUP_TYPE.GoogleLogin, OK_SIGNIN);
            return;
        }

        GoogleGamesManager.Instance.ShowLeaderboardUI();
    }

    private void OK_SIGNIN(params object[] parameters)
    {
//        SetUICameraEnable(false);
        GoogleGamesManager.Instance.SignInAuto();
    }

    public void OnClick_Solo()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.whistle);
        InGameManager.Instance.CurrentPlayMode = InGameManager.PLAY_MODE.Single;
        UnityAdsManager.Instance.CheckAdCount(ChangeSceneToArcade);        
    }

    private void ChangeSceneToArcade()
    {
        ChangeScene(SceneState.Arcade);
    }
}