using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameResult : GameUISequence
{
    public UI_GameResult()
    {
        IsPopupUI = true;
    }

    private System.Action m_servererrorOK = null;
    public System.Action ServerErrorOK
    {
        set
        {
            m_servererrorOK = value;
        }
    }

    private bool m_disconnentReward = false;
    public bool DisconnectReward
    {
        set
        {
            m_disconnentReward = value;
        }
    }

    private const int MAX_STAR = 3;
    public GameObject[] StarObjects;
    public UILabel CoinLabel;
    public GameObject OKButton;

    public GameObject Winobject;
    public GameObject LoseObject;

    private List<TweenScale> m_tweenList = new List<TweenScale>();

    private int m_checkCount = 0;

    private void Awake()
    {
        m_tweenList.Clear();
        foreach (GameObject obj in StarObjects)
        {
            TweenScale scale = obj.GetComponent<TweenScale>();

            obj.SetActive(false);
            m_tweenList.Add(scale);
        }
        OKButton.SetActive(false);
    }

    private const float DEFAULT_SCALE = 50.0f;

    private const int MAX_STAR_COUNT = 2;

    public override void StartGameSequence(int option)
    {
        SoundManager.Instance.StopBGM();
    }

    public void ShowObject()
    {
        MyObject.SetActive(true);
        CheckStartCount();
    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        return 0;
    }

    public override void PopSequence(GAME_UI_MODE mode = GAME_UI_MODE.None)
    {
        //block backkey
    }

    public void CheckStartCount()
    {
        bool iswin = true;
        m_checkCount = 3;

        if (m_servererrorOK != null)
        {
            m_checkCount = 2;
        }
        if (m_disconnentReward)
        {
            m_checkCount = 0;
        }

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
        {
            iswin = AccountManager.Instance.MyAccountInfo.MyScore <= ArcadeKingManager.Instance.GameScore;
            m_checkCount = ArcadeKingManager.Instance.CurrentRound;
            if (m_checkCount > 2)
                m_checkCount = 2;
        }
        else
        {
            if (m_servererrorOK != null)
                iswin = true;
            else if (m_disconnentReward)
                iswin = true;
            else
            {
                iswin = MyNetworkManager.Instance.GetGameResult();
                if (iswin)
                    m_checkCount = 3;
                else
                    m_checkCount = -1;
            }
        }

        SoundManager.Instance.PlaySound(iswin ? UISOUND_ID.Winning : UISOUND_ID.Fault);
        Winobject.SetActive(m_checkCount >= 0);
        LoseObject.SetActive(m_checkCount < 0);

        CoinLabel.text = "-";

        if (m_checkCount >= 0)
        {
            StartStarObject(0);
        }
        else
        {
            Finish();
        }

        CheckLeaderBoard(ArcadeKingManager.Instance.GameScore);
    }

    private void CheckLeaderBoard(int score)
    {
        if (InGameManager.Instance.CurrentPlayMode != InGameManager.PLAY_MODE.Single)
            return;

        if (score <= AccountManager.Instance.MyAccountInfo.MyScore)
            return;

#if UNITY_EDITOR
        AccountManager.Instance.MyAccountInfo.MyScore = score;
#else
        GoogleGamesManager.Instance.ReportWinning(score);
#endif

    }

    private void StartStarObject(int index)
    {
        if (m_checkCount < index)
        {
            Finish();
            return;
        }

        StopCoroutine("StartStarCoroutine");
        StartCoroutine("StartStarCoroutine", index);
    }

    IEnumerator StartStarCoroutine(int index)
    {
        yield return null;

        StarObjects[index].SetActive(true);
        m_tweenList[index].enabled = true;
        m_tweenList[index].ResetToBeginning();
    }

    public void OnFinish_Start0()
    {
        StartStarObject(1);
    }
    public void OnFinish_Start1()
    {
        StartStarObject(2);
    }
    public void OnFinish_Start2()
    {
        Finish();
    }

    private void Finish()
    { 
        // finish
        OKButton.SetActive(true);
        CoinLabel.text = string.Format("{0}", GetCoin());
    }

    private int GetCoin()
    {
        int result = 0;
        float coin_rate = 0.5f + (m_checkCount * 0.25f);
        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
            result = (int)(ArcadeKingManager.Instance.GameScore * coin_rate);
        else
            result = (int)(MyNetworkManager.Instance.MyGamePlayer.GameScore * 0.5f);

        if (result < 0)
            result = 1;

        return result;
    }

    public void OnClickOK()
    {
        AccountManager.Instance.MyAccountInfo.Gold += GetCoin();

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
        {
            InGameUIScene.Instance.ChangeScene(SceneState.Lobby);
            return;
        }

        if (m_servererrorOK == null)
        {
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.ReturnToLobbyFromResult;
            MyNetworkManager.Instance.ExitInGame();
        }
        else
        {
            m_servererrorOK();
            m_servererrorOK = null;
        }
    }

    public static void ShowGameResult(bool disconnectreward = false, System.Action servererror = null, bool timeover = false)
    {
        GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
        if (sequence == null)
            return;

        if (sequence.MyGameUIMode == GAME_UI_MODE.UI_GameResult)
            return;

        UI_GameResult result = GameUIManager.Instance.PushSequence(GAME_UI_MODE.UI_GameResult) as UI_GameResult;
        result.DisconnectReward = disconnectreward;
        result.ServerErrorOK = servererror;
        result.CheckStartCount();

        if (disconnectreward == false && servererror == null && timeover == false)
        {
//            InGameManager.Instance.GameState = InGameManager.GAME_STATE.EndGameDelay;
//            result.gameObject.SetActive(false);
        }
        else
        {
//            InGameManager.Instance.GameState = InGameManager.GAME_STATE.EndGame;
        }

        InGameManager.Instance.GameState = InGameManager.GAME_STATE.EndGame;
    }
}
