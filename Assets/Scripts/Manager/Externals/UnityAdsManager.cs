using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Advertisements;

public class UnityAdsManager : MonoBehaviour, IUnityAdsListener
{
    private static UnityAdsManager m_instance = null;
    public static UnityAdsManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject obj = ResourceManager.Instance.LoadResourceObject(UIResourcesNameDef.UnityAdsManager);
                DontDestroyOnLoad(obj);
                m_instance = obj.GetComponent<UnityAdsManager>();
            }

            return m_instance;
        }
    }

    private const int AD_COUNT = 3;
    private const string android_game_id = "3350624";
    private const string ios_game_id = "3350625";
    private const string rewarded_video_id = "rewardedVideo";
    private const string banner_id = "banner";

    private int m_adCount = 0;

#if PHEI_RELEASE
    bool testMode = false;
#else
    bool testMode = true;
#endif

    void Start()
    {
        Initialize();
    }

    public void Init()
    {

    }

    private void Initialize()
    {
        Advertisement.AddListener(this);
#if UNITY_ANDROID
        Advertisement.Initialize(android_game_id, testMode);
#elif UNITY_IOS
        Advertisements.Initialize(ios_game_id, testMode);
#endif
    }

    public void ShowRewardedVideo()
    {
        if (Advertisement.IsReady(rewarded_video_id) == false)
        {
            ActionCheckADFunc();
            return;
        }

        Advertisement.Show(rewarded_video_id);
    }

    public void ActionCheckADFunc()
    {
        if (m_checkadfunc != null)
            m_checkadfunc();

        m_adCount = 0;
    }

    // Implement IUnityAdsListener interface methods:
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            // Reward the user for watching the ad to completion.
            AccountManager.Instance.AdReward();
            ActionCheckADFunc();
        }
        else if (showResult == ShowResult.Skipped)
        {
            // Do not reward the user for skipping the ad.
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.LogError("The ad did not finish due to an error.");
        }

        m_checkadfunc = null;
    }

    public void OnUnityAdsReady(string placementId)
    {
        Debug.LogError("OnUnityAdsReady : " + placementId);
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
        Debug.LogError("OnUnityAdsDidError : " + message);
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        Debug.LogError("OnUnityAdsDidStart");
        // Optional actions to take when the end-users triggers an ad.
    }

    IEnumerator ShowBannerWhenReady()
    {
        Debug.LogError("ShowBannerWhenReady 1");
        while (!Advertisement.IsReady(banner_id))
        {
            Debug.LogError("ShowBannerWhenReady 2");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.LogError("ShowBannerWhenReady 3");
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        BannerOptions options = new BannerOptions { showCallback = OnShowBanner, hideCallback = OnHideBanner };
        Advertisement.Banner.Show(banner_id, options);        
    }

    private void OnShowBanner()
    {
        Debug.LogError("OnShowBanner");
    }

    private void OnHideBanner()
    {
        Debug.LogError("OnHideBanner");
    }

    private void OnLoadBannerSuccess()
    {
        Debug.LogError("OnLoadBannerSuccess");
    }

    private void OnLoadBannerFail(string message)
    {
        Debug.LogError("OnLoadBannerFail : " + message);
    }

    public void ShowBanner()
    {
//         if (IAPManager.Instance.DisableAD)
//             retur
        StopCoroutine("ShowBannerWhenReady");
        StartCoroutine("ShowBannerWhenReady");
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
    }

    private System.Action m_checkadfunc = null;
    public void CheckAdCount(System.Action action)
    {
        if (IAPManager.Instance.DisableAD)
        {
            action();
            return;
        }

        m_adCount++;
        if (m_adCount >= AD_COUNT)
        {
            m_adCount = 0;

            m_checkadfunc = action;
            PopupManager.Instance.ShowBuyADPopup(OK_ADbuy, Cancel_AdBuy);
            return;
        }

        action();
    }

    private void OK_ADbuy(params object[] paramters)
    {
        m_checkadfunc = null;
        IAPManager.Instance.Purchase(IAPManager.Product_DisableAD);
    }

    private void Cancel_AdBuy(params object[] paramters)
    {
        ShowRewardedVideo();
        m_adCount = 0;
    }
}
