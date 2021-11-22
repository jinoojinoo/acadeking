using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_MessageBox : PopupBase
{
    public enum PopupType
    {
        OneButton = 1,
        TwoButton,
    }
    public UITable SortTable;

    protected enum ButtonType
    {
        OK,
        Cancel,
        Max,
    }

    public GameObject[] Buttons;
    public UILabel[] ButtonLabel;
    public GameObject CloseButtonObject;

    private void Awake()
    {
        InGameManager.GAME_PAUSE = true;
    }

    private void OnDestroy()
    {
        InGameManager.GAME_PAUSE = false;
    }

    public override void Init(UIPopup_DataProperty property)
    {
        base.Init(property);

        int max = (int)property.GetPopupType();
        InitButtonLabel(property, ButtonLabel, max);
        for(int i = 0; i < Buttons.Length;++i)
        {
            Buttons[i].SetActive(i < max ? true : false);
        }
        SortTable.Reposition();

        CloseButtonObject.SetActive(property.GetPopupType() == PopupType.OneButton ? false : true);
        if (property.GetPopupType() == PopupType.OneButton)
            CancelFunc = OkFunc;
    }

    protected override string GetStringOption(int count)
    {
        if (DataProperty.TYPE == POPUP_TYPE.InBattle_Esc && count == 0)
            return InGameManager.Instance.CurrentScore.ToString();

        return string.Empty;
    }
}
