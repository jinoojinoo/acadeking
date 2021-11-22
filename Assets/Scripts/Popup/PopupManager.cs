using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Globalization;

public abstract class PopupBase : MonoBehaviour
{
    private UIPopup_DataProperty m_property = null;
    protected UIPopup_DataProperty DataProperty
    {
        get
        {
            return m_property;
        }
    }

    public bool Not_Available_BackKey
    {
        get
        {
            return DataProperty.TYPE > POPUP_TYPE.Not_Available_BackKey;
        }
    }

    public virtual void Init(UIPopup_DataProperty property)
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);

        m_property = property;

        if (TitleLabel != null)
            TitleLabel.text = property.Title_String;

        if (MsgLabel != null)
        {
            MsgLabel.text = InitMsgLabel(property);
            MsgLabel.fontSize = property.FontSize == -1 ? 50 : property.FontSize;
        }
    }

    private string InitMsgLabel(UIPopup_DataProperty property)
    {
        if (property.MSG_Option == 0)
            return property.MSG_String;

        switch (property.MSG_Option)
        {
            case 1:
                return string.Format(property.MSG_String, GetStringOption(0));
            case 2:
                return string.Format(property.MSG_String, GetStringOption(0),
                    GetStringOption(1));
            case 3:
                return string.Format(property.MSG_String, GetStringOption(0),
                    GetStringOption(1),
                    GetStringOption(2));
            case 4:
                return string.Format(property.MSG_String, GetStringOption(0),
                    GetStringOption(1),
                    GetStringOption(2),
                    GetStringOption(3));
            case 5:
                return string.Format(property.MSG_String, GetStringOption(0), 
                    GetStringOption(1),
                    GetStringOption(2),
                    GetStringOption(3),
                    GetStringOption(4));

            default:
                return string.Empty;
        }
    }

    protected virtual string GetStringOption(int count)
    {
        return string.Empty;
    }

    public UILabel TitleLabel;
    public UILabel MsgLabel;

    public delegate void PopupButtonDelegate(params object[] parameters);
    private PopupButtonDelegate m_okFunc = null;
    public PopupButtonDelegate OkFunc
    {
        set
        {
            m_okFunc = value;
        }
        get
        {
            return m_okFunc;
        }
    }
    private PopupButtonDelegate m_cancelFunc = null;
    public PopupButtonDelegate CancelFunc
    {
        set
        {
            m_cancelFunc = value;
        }
        get
        {
            return m_cancelFunc;
        }
    }

    public void OnClick_OK()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);
        PopupManager.Instance.ClosePopup(this);

        if (m_okFunc != null)
            m_okFunc();
    }

    public void OnClick_Close()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);
        PopupManager.Instance.ClosePopup(this);

        if (m_cancelFunc != null)
            m_cancelFunc();
    }

    protected void InitButtonLabel(UIPopup_DataProperty property, UILabel[] labels, int maxcount)
    {
        for (int i = 0; i < labels.Length; ++i)
        {
            if (labels[i] == null)
                continue;

            if (i < maxcount)
            {
                property.SetUIPopupButtonLabel(i, labels[i]);
            }
        }
    }
}

public class PopupManager : MonoBehaviour
{
    private const float DefaultPosX = -5000.0f;
    private const int CameraDepth = 100;

    private const string Popup_Layer = "UI_POPUP";

    private List<PopupBase> m_popupList = new List<PopupBase>();

    private int m_layerNumber = int.MinValue;
    private int LayerNumber
    {
        get
        {
            if (m_layerNumber == int.MinValue)
                m_layerNumber = LayerMask.NameToLayer(Popup_Layer);
            return m_layerNumber;
        }
    }

    private GameObject m_myObject = null;
    private GameObject MyObject
    {
        get
        {
            if (m_myObject == null)
                m_myObject = this.gameObject;
            return m_myObject;
        }
    }

    private Transform m_myTrans = null;
    private Transform MyTrans
    {
        get
        {
            if (m_myTrans == null)
                m_myTrans = this.transform;
            return m_myTrans;
        }
    }

    private static PopupManager m_instance = null;
    public static PopupManager Instance
    {
        get
        {
            if (m_instance != null)
                return m_instance;

            GameObject obj = Instantiate(ResourceManager.Instance.LoadResource<GameObject>(UIResourcesNameDef.Popup_Object));
            DontDestroyOnLoad(obj);
            m_instance = obj.GetComponent<PopupManager>();

            return m_instance;
        }
    }

    public void ShowPopup(GameObject obj)
    {
        PopupBase popupbase = obj.GetComponent<PopupBase>();

        Camera popup_cam = obj.GetComponent<Camera>();
        if (popup_cam == null)
        {
            popup_cam = obj.AddComponent<Camera>();
            popup_cam.clearFlags = CameraClearFlags.Depth;
        }
        popup_cam.cullingMask = 1 << LayerNumber;

        Transform trans = obj.transform;
        trans.ChangeLayerReculsively(LayerNumber);
        trans.parent = MyTrans;
        trans.localScale = Vector3.one;
        HUDScale.SetHUDModifyCamera(popup_cam);

        Vector3 newPos;
        int totalCount = m_popupList.Count;

        if (m_popupList.Count / 10 <= 0)
            newPos = new Vector3(DefaultPosX + (totalCount * 2500.0f), 2000.0f, 0.0f);
        else
            newPos = new Vector3(DefaultPosX + ((totalCount / 10) * 2500.0f), -2000.0f, 0.0f);

        popup_cam.depth = CameraDepth + totalCount;
        trans.localPosition = newPos;

        m_popupList.Add(popupbase);
    }

    public void ClosePopup(PopupBase popupbase)
    {
        bool popuptarget = popupbase == null ? false : true;
        if (popupbase == null)
        {
            if (m_popupList.Count <= 0)
                return;

            popupbase = m_popupList[m_popupList.Count - 1];
        }

        if (popuptarget == false && popupbase.Not_Available_BackKey)
            return;

        m_popupList.Remove(popupbase);
        Destroy(popupbase.gameObject);
    }

    public void AllClearPopup()
    {
        foreach (PopupBase popupbase in m_popupList)
        {
            Destroy(popupbase.gameObject);
        }
        m_popupList.Clear();
    }

    public PopupBase ShowPopup(POPUP_TYPE type, PopupBase.PopupButtonDelegate okfunc = null, PopupBase.PopupButtonDelegate cancelfunc = null)
    {
        UIPopup_DataProperty property = UIPopup_Table.Instance.GetUIPopupDataProperty(type);
        if (property == null)
            return null;

        GameObject obj = ResourceManager.Instance.LoadResourceObject(property.Path);
        if (obj == null)
            return null;

        ShowPopup(obj);
        PopupBase popupbase = obj.GetComponent<PopupBase>();
        obj.SetActive(true);

        popupbase.OkFunc = okfunc;
        popupbase.CancelFunc = cancelfunc;
        popupbase.Init(property);

        return popupbase;
    }

    public bool IsPopupVisible
    {
        get
        {
            return m_popupList.Count > 0;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AllClearPopup();
    }

    public void ShowGoldErrorPopup()
    {
        Popup_MessageBox popupbase = ShowPopup(POPUP_TYPE.GoldError, ShowAD) as Popup_MessageBox;

        string msg = popupbase.ButtonLabel[0].text;
        popupbase.ButtonLabel[0].text = string.Format(msg, GlobalValue_Table.Instance.AD_REWARD_GOLD);
    }

    public void ShowAD(params object[] paramters)
    {
        Debug.LogError("showad");
        UnityAdsManager.Instance.ShowRewardedVideo();
    }

    public void ShowBuyADPopup(PopupBase.PopupButtonDelegate ok, PopupBase.PopupButtonDelegate cancel)
    {
        Popup_MessageBox popupbase = ShowPopup(POPUP_TYPE.BuyAD, ok, cancel) as Popup_MessageBox;
        int money = int.Parse(popupbase.ButtonLabel[0].text);

        NumberFormatInfo numberFormat = new CultureInfo("ko-KR", false).NumberFormat;
        popupbase.ButtonLabel[0].text = money.ToString("c", numberFormat);
    }
}
