using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public partial class GoogleGamesManager : SingletonMono<GoogleGamesManager>
{
    private System.Action<bool> m_loginFunc = null;
    public System.Action<bool> LoginFunc
    {
        set
        {
            m_loginFunc = value;
        }
    }

    public void Awake()
    {
        if (IsSignIn())
            return;

#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
//            .AddOauthScope("https://www.googleapis.com/auth/drive.file")
            .RequestIdToken()
            .EnableSavedGames()
            .Build();

//         PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
//             .RequestIdToken()
//             .RequestServerAuthCode(false /* Don't force refresh */)
//             .Build(
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.InitializeInstance(config);
#if !PHEI_RELEASE
        PlayGamesPlatform.DebugLogEnabled = true;
#endif
        PlayGamesPlatform.Activate();
#endif
    }
    /*
    private void Start()
    {
        if (InGameUIScene.Instance == null)
            return;

        ShowWaitingPopup(false);
        SignIn(ShowWaitingPopup);
    }*/

    public void Init()
    {
#if UNITY_EDITOR
       LoadFromPlayerPrefab();
#endif
    }

    private PopupBase m_showPopup = null;
    private void ShowWaitingPopup(bool login)
    {
        if (login)
        {
            if (m_showPopup == null)
                return;

            m_showPopup.OnClick_Close();
            m_showPopup = null;

            return;
        }

        if (m_showPopup != null)
        {
            m_showPopup.OnClick_Close();
            m_showPopup = null;
        }
        m_showPopup = PopupManager.Instance.ShowPopup(POPUP_TYPE.Notice_WaitForLogin);
    }

    public void Sign(bool signin)
    {
        if (signin)
            SignIn(ShowWaitingPopup);
        else
            SignOut();

        if (m_loginFunc != null)
            m_loginFunc(signin);

        PlayerPrefabsID.SetLoginType(LOGIN_TYPE.GOOGLE, signin);
    }

    public void SignInAuto()
    {
        if (m_showPopup != null)
        {
            m_showPopup.OnClick_Close();
            m_showPopup = null;
        }
        m_showPopup = PopupManager.Instance.ShowPopup(POPUP_TYPE.Notice_WaitForLogin);

        SignIn(delegate(bool login)
        {
            if (m_showPopup != null)
            {
                m_showPopup.OnClick_Close();
                m_showPopup = null;
            }
        }
        );
    }

    public void SignIn(System.Action<bool> action = null)
    {
        if (IsSignIn())
        {
            if (action != null)
                action(true);

            return;
        }

        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                if (action != null)
                    action(true);

                LoadFromCloud(action);
//                FaceBookManager.Instance.FaceBookLogin(true);
            }
            else
            {
                if (action != null)
                    action(true);
            }
        });
    }

    public void SignOut()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.SignOut();
#endif
    }

    public void ShowAchievementUI()
    {
        Debug.LogError("ShowAchievementUI 0");
        // Sign In 이 되어있지 않은 상태라면
        // Sign In 후 업적 UI 표시 요청할 것
        if (IsSignIn() == false)
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    // Sign In 성공
                    // 바로 업적 UI 표시 요청
                    Social.ShowAchievementsUI();
                    return;
                }
                else
                {
                    // Sign In 실패 처리
                    return;
                }
            });

            return;
        }

        Social.ShowAchievementsUI();
    }

    public void ShowLeaderboardUI(int index = 0)
    {
#if !UNITY_ANDROID
        return;
#endif
        // Sign In 이 되어있지 않은 상태라면
        // Sign In 후 리더보드 UI 표시 요청할 것
        if (IsSignIn() == false)
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    // Sign In 성공
                    // 바로 리더보드 UI 표시 요청
#if UNITY_ANDROID
                    PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard);
#endif
                    return;
                }
                else
                {
                    // Sign In 실패 
                    // 그에 따른 처리
                    return;
                }
            });

            return;
        }
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard);
#endif
    }

    public bool ReportProgress(string key, System.Action<bool> receiveFunc)
    {
        if (IsSignIn() == false)
            return false;

        Social.ReportProgress(key, 100.0f, (bool suc) =>
        {
            if (receiveFunc != null)
                receiveFunc(true);
        });
        return true;
    }

    public bool ReportWinning(int winning, int index = 0, System.Action<bool> receiveFunc = null)
    {
        if (IsSignIn() == false)
            return false;

        Social.ReportScore((long)winning, GPGSIds.leaderboard, (bool success) =>
        {
            if (success)
            {
                // Report 성공
                UnityEngine.Debug.Log("Score report success\n score: " + winning);
                AccountManager.Instance.MyAccountInfo.MyScore = winning;
            }
            else
            {
                // Report 실패
                UnityEngine.Debug.Log("Score report fail");
            }

            if (receiveFunc != null)
                receiveFunc(success);
        });
        return true;
    }

    public bool IsSignIn()
    {
        return Social.localUser.authenticated;
    }

    public string UserName
    {
        get
        {
            return Social.localUser.userName;
        }        
    }
}