using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CreateRoom : GameUISequence
{
    public const string DEFAULT_NAME = "Default";

//    public UILabel[] ButtonLabel;

    public UIInput RoomNameInput;
    public bool m_inputFocus = true;
    public UI_Toggle RoundToggle;
    public UI_Toggle TimeToggle;

    public override void StartGameSequence(int option)
    {

    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        return 0;
    }

    public void OnClick_Create()
    {
        if (string.Compare(RoomNameInput.value, DEFAULT_NAME) == 0 ||
            string.IsNullOrEmpty(RoomNameInput.value))
        {
            SoundManager.Instance.PlaySound(UISOUND_ID.Wrong);
            PopupManager.Instance.ShowPopup(POPUP_TYPE.Create_Error_Name, ClickErrorName, ClickErrorName);
            return;
        }

        MyNetworkManager.Instance.CreateRoomInfo.Init(RoomNameInput.value, RoundToggle.CurrentIndex, TimeToggle.CurrentIndex);
        string roomname = string.Format("{0},{1},{2}", RoomNameInput.value, RoundToggle.CurrentIndex, TimeToggle.CurrentIndex);
        NetworkMessageManager.Instance.SendMessage(Network_ID.CreateRoom, roomname);
    }
    private void ClickErrorName(params object[] paramters)
    {
        RoomNameInput.isSelected = true;
    }

/*    private void Update()
    {
        if (m_inputFocus == false && CountInput.isSelected)
        {
            m_inputFocus = true;
        }
        else if (m_inputFocus && CountInput.isSelected == false)
        {
            OnSubmit_CountInput();
        }
    }

    public void OnSubmit_CountInput()
    {
        if (string.IsNullOrEmpty(CountInput.value))
        {
            CountInput.value = NetworkMessage_CreateRoom.Min_Member.ToString();
            m_inputFocus = false;
            return;
        }

        uint result = 0;
        if (uint.TryParse(CountInput.value, out result))
        {
            m_inputFocus = false;
            if (result > NetworkMessage_CreateRoom.Max_Member)
            {
                CountInput.value = NetworkMessage_CreateRoom.Max_Member.ToString();
            }
            else if (result < NetworkMessage_CreateRoom.Min_Member)
            {
                CountInput.value = NetworkMessage_CreateRoom.Min_Member.ToString();
            }
        }
        else
        {
            CountInput.value = NetworkMessage_CreateRoom.Min_Member.ToString();
            m_inputFocus = false;
        }
    }*/
}
