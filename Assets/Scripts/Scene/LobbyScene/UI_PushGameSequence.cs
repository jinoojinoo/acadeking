using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PushGameSequence : MonoBehaviour
{
    public GAME_UI_MODE mode = GAME_UI_MODE.None;

    private static Dictionary<GAME_UI_MODE, ActionClickEvent_Base> m_clickActionList = null;
    private static Dictionary<GAME_UI_MODE, ActionClickEvent_Base> ClickActionList
    {
        get
        {
            if (m_clickActionList == null)
            {
                m_clickActionList = new Dictionary<GAME_UI_MODE, ActionClickEvent_Base>();
//                m_clickActionList.Add(GAME_UI_MODE.UI_RoomList, new ActionClickEvent_RoomList());
            }                

            return m_clickActionList;
        }
    }

    private void OnClick()
    {
        if (mode == GAME_UI_MODE.None)
            return;

        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);

        if (ActionClickEvent() == false)
            return;

        if (GameUIManager.Instance.GetCurrentUISequence().MyGameUIMode != GAME_UI_MODE.UI_Lobby)
            return;

        SoundManager.Instance.PlaySound(UISOUND_ID.beep2);
        GameUIManager.Instance.PushSequence(mode);
    }

    private bool ActionClickEvent()
    {
        if (ClickActionList.ContainsKey(mode) == false)
            return true;

        ClickActionList[mode].ActionClickEvent();
        return false;
    }

    private void OnDestroy()
    {
        ClickActionList.Clear();
    }
}

public abstract class ActionClickEvent_Base 
{
    public abstract void ActionClickEvent();
}

public class ActionClickEvent_RoomList : ActionClickEvent_Base
{
    public ActionClickEvent_RoomList()
    {
        NetworkMessageManager.Instance.AddReceivePacketHandler(Network_ID.ListMatch, ReceivePacket_ListMatch);
    }

    ~ActionClickEvent_RoomList()
    {
        NetworkMessageManager.Instance.RemoveReceivePacketHandler(Network_ID.ListMatch, ReceivePacket_ListMatch);
    }

    public override void ActionClickEvent()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PopupManager.Instance.ShowPopup(POPUP_TYPE.NotConnectNetwork);
            return;
        }

        NetworkMessageManager.Instance.SendMessage(Network_ID.ListMatch, 0, "");
    }

    private void ReceivePacket_ListMatch(bool success)
    {
        if (success == false)
        {
            PopupManager.Instance.ShowPopup(POPUP_TYPE.NotConnectNetwork);
            return;
        }

        GameUIManager.Instance.PushSequence(GAME_UI_MODE.UI_RoomList);
    }
}

