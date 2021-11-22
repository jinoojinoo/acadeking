using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

public enum POPUP_TYPE
{
    None = -1,
    GameEnd,
    InBattle_Esc,
    Create_Room,
    Leave_Room,
    NotConnectNetwork,
    Item_Buy,
    Item_Equip,
    Item_UnEquip,
    GoldError,
    Create_Error_Name,
    Ball_Point,
    Ball_Skin,
    Ball_AddSlot,
    BuyProduct,
    BuyAD,

    Not_Available_BackKey,
    Notice_WaitInGame,
    ReturnLobby,
    GuestLogin_Warning,
    GuestLogin_Name,

    GoogleLogin,
    GameModeSelect,
    Notice_WaitForLogin,
}

[System.Serializable]
public class UIPopup_DataProperty : BaseDataProperty
{
    [XmlElement("POPUP_TYPE")]
    public POPUP_TYPE TYPE;

    [XmlElement("PATH")]
    public string Path;

    [XmlElement("Title_String")]
    public string Title_String;

    [XmlElement("MSG_String")]
    public string MSG_String;

    [XmlElement("MSG_Option")]
    public int MSG_Option = 0;    

    [XmlElement("Button1_String")]
    public string Button1String;

    [XmlElement("Button2_String")]
    public string Button2String;

    [XmlElement("Button3_String")]
    public string Button3String;

    [XmlElement("Button4_String")]
    public string Button4String;

    [XmlElement("FontSize")]
    public int FontSize = -1;

    public void SetUIPopupButtonLabel(int index, UILabel label)
    {
        string buttonstring = string.Empty;
        switch (index)
        {
            case 0:
                buttonstring = Button1String;
                break;

            case 1:
                buttonstring = Button2String;
                break;

            case 2:
                buttonstring = Button3String;
                break;

            case 3:
                buttonstring = Button4String;
                break;

            default:
                return;
        }

        if (string.IsNullOrEmpty(buttonstring))
            return;

        label.text = buttonstring;
    }

    public Popup_MessageBox.PopupType GetPopupType()
    {
        if (string.IsNullOrEmpty(Button2String))
            return Popup_MessageBox.PopupType.OneButton;

        return Popup_MessageBox.PopupType.TwoButton;
    }
}

[XmlRoot("TABLE")]
public class UIPopup_Table : BaseDataTable<UIPopup_Table, UIPopup_DataProperty>
{
    [XmlElement("UIPopup")]
    public UIPopup_DataProperty[] DataList
    {
        get
        {
            return TableDataList;
        }
        set
        {
            TableDataList = value;
        }
    }

    public UIPopup_DataProperty GetUIPopupDataProperty(POPUP_TYPE type)
    {
        return TableList.Find(x => x.TYPE == type);
    }
}