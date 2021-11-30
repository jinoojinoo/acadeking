using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StartScene : InGameUIScene
{
    protected override void Awake()
    {
        base.Awake();

        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Debug.LogError("Application.targetFrameRate  :  60");


        int x = (int)(Screen.height * GlobalValue_Table.Instance.ASPECT_X / GlobalValue_Table.Instance.ASPECT_Y);
        int y = (int)(Screen.width * GlobalValue_Table.Instance.ASPECT_Y / GlobalValue_Table.Instance.ASPECT_X);
        Screen.SetResolution(Screen.width, y, true);

        ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;

        AccountManager.DestroyInstance();
    }

    private void Start()
    {
#if UNITY_EDITOR
        InGameManager.ClientOnlyPlay = false;
#endif
        SoundManager.Instance.LoadOptionValue();
        GameUIManager.Instance.PushSequence(GAME_UI_MODE.Start, MyObject);
    }    

    public override void StartGameSequence(int option)
    {
        UnityAdsManager.Instance.ShowBanner();
        SoundManager.Instance.PlaySound(UISOUND_ID.BGM_Start, true);
    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        UnityAdsManager.Instance.HideBanner();
        return 0;
    }

    protected override void ActionAndroidBackKey()
    {
        PopupManager.Instance.ShowPopup(POPUP_TYPE.GameEnd, ActionAndroidBackKey_OK, null);
    }

    private void ActionAndroidBackKey_OK(params object[] parameters)
    {
        Application.Quit();
    }
}

