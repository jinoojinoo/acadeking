using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UI_InGameScene : InGameUIScene
{
//     public GameObject MyBasketballAracade;
//     public GameObject OtherBasketballAracade;

    public UI_InGameScene()
    {
        IsRootUI = true;
    }

    private void Start()
    {
//        MyBasketballAracade.gameObject.SetActive(false);
//        OtherBasketballAracade.gameObject.SetActive(false);

        GameUIManager.Instance.PushSequence(GAME_UI_MODE.InGame, MyObject);
        GlobalValue_Table.Instance.LoadEnvironment();
    }

    public override void StartGameSequence(int option)
    {
        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
        {
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayReady;
            StopCoroutine("WaitForLoading");
            StartCoroutine("WaitForLoading");
        }
    }

    IEnumerator WaitForLoading()
    {
        while (UIGameLoading.IsCreateInstance() &&
            UIGameLoading.Instance.CurrentLoadingState < UIGameLoading.LoadingState.End)
        {
            yield return null;
            continue;
        }

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
        {
            ArcadeKingManager.Instance.CreateBasketBall(0, AccountManager.Instance.MyBallList.m_ballList, null, true);
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayCount;
        }
        else if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.TEST)
        {
            ArcadeKingManager.Instance.CreateBasketBall(0, AccountManager.Instance.MyBallList.m_ballList, null, true);
            ArcadeKingManager.Instance.CreateBasketBall(1, AccountManager.Instance.MyBallList.m_ballList, null, false);
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.PlayCount;
        }

        yield break;
    }

    public override void EndGameSequenceForRoot()
    {
    }

    protected override void ActionAndroidBackKey()
    {
        if (InGameManager.Instance.GameState != InGameManager.GAME_STATE.Play)
            return;

        if (GameUIManager.Instance.GetCurrentUISequence().MyGameUIMode == GAME_UI_MODE.UI_GameResult)
            return;

        PopupManager.Instance.ShowPopup(POPUP_TYPE.InBattle_Esc, ActionAndroidBackKey_OK, RestartGame);
        InGameManager.GAME_PAUSE = true;
    }

    private void RestartGame(params object[] parameters)
    {
        InGameManager.GAME_PAUSE = false;
    }

    private void ActionAndroidBackKey_OK(params object[] parameters)
    {
        if (InGameManager.Instance.CurrentPlayMode== InGameManager.PLAY_MODE.Single)
        {
            InGameUIScene.Instance.ChangeScene(SceneState.Lobby);
            return;
        }
        MyNetworkManager.Instance.NoneReward = true;
        MyNetworkManager.Instance.ExitInGame();
    }

    protected override void LoadDoneFunc()
    {
        base.LoadDoneFunc();
        ArcadeKingManager.Instance.Reset();
    }
}
