using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Facebook.Unity;

//페이스북 SNS 연결
public class FaceBookManager : SingletonMono<FaceBookManager>
{
    private System.Action<bool> m_loginFunc = null;
    public System.Action<bool> LoginFunc
    {
        get
        {
            return m_loginFunc;
        }
        set
        {
            m_loginFunc = value;
        }
    }
    private void Awake()
    {
        if (!FB.IsInitialized)
            FB.Init(InitCallBack, OnHideUnity);
        else
            FB.ActivateApp();
    }

    public void Init()
    {

    }

    public void FaceBookLogin(bool login)
    {
        if (login)
            OnClick_FaceBookLogin();
        else
            LogOut();
    }

    public bool IsFaceBookLogin()
    {
        return FB.IsLoggedIn;
    }

    private void LogOut()
    {
        FB.LogOut();
    }

    private void InitCallBack()
    {
        if (FB.IsInitialized)
            FB.ActivateApp();
        else
            Debug.LogError("Failed to initialize the facbook sdk");
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            GlobalValue_Table.Instance.LoadEnvironment();
        }
    }

    public void OnClick_FaceBookLogin()
    {
        List<string> list = new List<string>() { "public_profile" };
        FB.LogInWithReadPermissions(list, AuthCallBack);
    }

    private void AuthCallBack(ILoginResult result)
    {
        if (result.Error != null)
        {
            Debug.LogError("face login error : " + result.Error);
            if (m_loginFunc != null)
                m_loginFunc(false);
            return;
        }

        if (FB.IsLoggedIn)
        {
            AccessToken token = Facebook.Unity.AccessToken.CurrentAccessToken;
            foreach (string str in token.Permissions)
            {
                Debug.LogError("permissions : " + str);
            }
        }
        else
        {
            Debug.LogError("User cancelled login");
        }

        DealWithFBMenus(FB.IsLoggedIn);
    }

    //페이스북 프로파일 이미지, 사용자 이름 가져오기
    void DealWithFBMenus(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            // Get Username Code
            FB.API("/me?fields=id,first_name", HttpMethod.GET, DisplayUserID);
        }
        else
        {
            Debug.LogError("DealWithFBMenus : " + isLoggedIn);
        }

        if (m_loginFunc != null)
            m_loginFunc(isLoggedIn);
    }

    private string m_myUserID = string.Empty;

    private void DisplayUserID(IResult result)
    {
        if (result.Error != null)
        {
            Debug.LogError("DisplayUserID error : " + result.Error);
            return;
        }

        m_myUserID = result.ResultDictionary["first_name"] as string;
    }
}