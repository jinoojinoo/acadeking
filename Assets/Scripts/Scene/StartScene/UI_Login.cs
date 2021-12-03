using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LOGIN_TYPE
{
    GUEST,
    GOOGLE,
    FACEBOOK,
}

public class UI_Login : MonoBehaviour
{
    public UIButton GoogleButton;

    public UIInput UserNameInput;
    public UIInput PasswordInput;
    public UIToggle MyToggle;

    private string m_userName = string.Empty;
    private string m_passWord = string.Empty;

    private int m_loginType = 0;
    private void Start()
    {
        UserNameInput.value = AccountManager.Instance.GetUserName();
        PasswordInput.value = AccountManager.Instance.GetPassword();
        MyToggle.value = PlayerPrefs.GetInt(PlayerPrefabsID.LOGIN_Remember, 0) == 0 ? false : true;

        GoogleGamesManager.Instance.Init();
        FaceBookManager.Instance.Init();
        UnityAdsManager.Instance.Init();
        IAPManager.Instance.Init();

        m_loginType = (int)PlayerPrefabsID.GetLoginType();
        if ((m_loginType & (1 << (int)LOGIN_TYPE.GOOGLE)) != 0)
        {
            InitSignInAuto();
        }

        if ((m_loginType & (1 << (int)LOGIN_TYPE.FACEBOOK)) != 0)
        {
            FaceBookManager.Instance.FaceBookLogin(true);
        }
    }

    private void InitSignInAuto()
    {
        InGameManager.Instance.SignInAuto(() =>
        {
            m_userName = GoogleGamesManager.Instance.UserName;
            UserNameInput.Set(m_userName);
            OnClick_WarningOK();
        });
    }

    private void SignInOk(bool sign)
    {
        UserNameInput.value = AccountManager.Instance.GetUserName();
    }

    public void OnClick_Login()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);

        if (string.Compare(UserNameInput.value, UserNameInput.defaultText) == 0)
        {
            PopupManager.Instance.ShowPopup(POPUP_TYPE.GuestLogin_Name);
            return;
        }

        if (GoogleGamesManager.Instance.IsSignIn() == false &&
            PlayerPrefs.GetInt(PlayerPrefabsID.LOGIN_Remember, 0) == 0)
        {
            PopupManager.Instance.ShowPopup(POPUP_TYPE.GuestLogin_Warning, OnClick_WarningOK, null);
            return;
        }

        OnClick_WarningOK(null);
    }

    private void OnClick_WarningOK(params object[] parameters)
    {
        PlayerPrefs.SetString(PlayerPrefabsID.UserName, UserNameInput.value);
        PlayerPrefs.Save();
        InGameUIScene.Instance.ChangeScene(SceneState.Lobby);
    }

    public void OnClick_Cancel()
    {
        InGameUIScene.Instance.PopSequence();
    }

    public void OnSummit_UserName()
    {
        m_userName = NGUIText.StripSymbols(UserNameInput.value);
    }

    public void OnSummit_Password()
    {
        m_passWord = NGUIText.StripSymbols(PasswordInput.value);
        PlayerPrefs.SetString(PlayerPrefabsID.PassWord, m_passWord);
    }

    public void OnClick_Google()
    {
        PlayerPrefabsID.SetLoginType(LOGIN_TYPE.GOOGLE, true);
        InitSignInAuto();
    }

/*    private void InitGoogle(bool islogin)
    {
        if (islogin == false)
            return;

        GoogleButton.isEnabled = false;
        BoxCollider collider = UserNameInput.GetComponent<BoxCollider>();
        collider.enabled = false;

        if (GoogleGamesManager.Instance.IsSignIn() == false)
        {
            StopCoroutine("WaitForSignIn");
            StartCoroutine("WaitForSignIn");
        }
        else
        {
            m_userName = GoogleGamesManager.Instance.UserName;
            UserNameInput.Set(m_userName);
            OnClick_WarningOK();
        }
    }*/

    public void OnClick_Leader1()
    {
        if (GoogleGamesManager.Instance.IsSignIn() == false)
            return;
        GoogleGamesManager.Instance.ShowLeaderboardUI(0);
    }

    public void OnClick_Leader2()
    {
        if (GoogleGamesManager.Instance.IsSignIn() == false)
            return;
        GoogleGamesManager.Instance.ShowLeaderboardUI(1);
    }

    public void OnClick_Archevement()
    {
        if (GoogleGamesManager.Instance.IsSignIn() == false)
            return;
        GoogleGamesManager.Instance.ShowAchievementUI();
    }
}
