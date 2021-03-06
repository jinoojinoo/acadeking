using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScore : ResetUIComponent
{
    public override int Order
    {
        get
        {
            return 0;
        }
    }

    public override InGameManager.GAME_STATE AdjustGameState
    {
        get
        {
            return InGameManager.GAME_STATE.PlayCount;
        }
    }

    public override void ResetComponent(bool reset)
    {
        if (reset == false)
            return;

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
            SetScore(ArcadeKingManager.Instance.GameScore, false);
    }

    private const string SCORE_STR = "Score {0}/{1}";
    private const string ROUND_STR = "ROUND {0}/{1}";
    public UILabel[] Labels;
    public UIProgressBar ProgressBar;

    private enum LabelType
    {
        Round,
        Score,
        Time,
    }

    protected void Start()
    {
        ArcadeKingManager.Instance.GameRoundFunc -= SetRound;
        ArcadeKingManager.Instance.GameRoundFunc += SetRound;

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
        {
            ArcadeKingManager.Instance.GameScoreFunc -= SetScore;
            ArcadeKingManager.Instance.GameScoreFunc += SetScore;
        }
        else
        {
            ArcadeKingManager.Instance.MultieGameScoreFunc -= SetMultieScore;
            ArcadeKingManager.Instance.MultieGameScoreFunc += SetMultieScore;
        }
    }

    public void SetScore(int score, bool isclean)
    {
        Labels[(int)LabelType.Score].text = string.Format(SCORE_STR, score, ArcadeKingManager.Instance.GetRoundMaxScore());
        if (ArcadeKingManager.Instance.SettingProperty == null)
            return;

        if (ArcadeKingManager.Instance.SettingProperty.Score == 0)
            ProgressBar.value = (float)score / 100.0f;
        else
            ProgressBar.value = (float)score / (float)ArcadeKingManager.Instance.SettingProperty.Score;
    }

    private void SetMultieScore()
    {
        int sum = 0;
        int my = 0;
        int other = 0;

        if (MyNetworkManager.Instance != null)
            MyNetworkManager.Instance.GetMultieplayCurrentScore(ref sum, ref my, ref other);

        Labels[(int)LabelType.Score].text = string.Format("{0} : {1}", my, other);
        if (sum == 0)
            ProgressBar.value = 0.5f;
        else
            ProgressBar.value = (float)my / (float)sum;
    }

    public void SetRound(int round)
    {
        if (round > ArcadeKingManager.Instance.GetMaxRound())
            round = ArcadeKingManager.Instance.GetMaxRound();

        Labels[(int)LabelType.Round].text = string.Format(ROUND_STR, round + 1, ArcadeKingManager.Instance.GetMaxRound() + 1);
    }

    public void OnClick_Close()
    {
        InGameUIScene.Instance.Close();
    }
}
