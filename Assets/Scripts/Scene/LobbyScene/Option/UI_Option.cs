using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Option : GameUISequence
{
    public UIGrid Grid;
    private UI_OptionButton[] m_optionButtons = null;

    private void Start()
    {
        m_optionButtons = MyObject.GetComponentsInChildren<UI_OptionButton>();
        ResetData();
    }

    private void ResetData()
    {
        for (int i = 0; i < (int)m_optionButtons.Length; ++i)
        {
            m_optionButtons[i].Init(i);
        }

        Grid.Reposition();
    }

    public override void StartGameSequence(int option)
    {
        FaceBookManager.Instance.LoginFunc = FaceBookLogin;
    }

    private void FaceBookLogin(bool login)
    {
        int index = (int)OptionType.FaceBookConnect;
        m_optionButtons[index].Init(index);
    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        FaceBookManager.Instance.LoginFunc = null;

        SoundManager.Instance.SaveOptionValue();
        AccountManager.Instance.SaveGameOption();
        return 0;
    }
}
