using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_OptionButton : MonoBehaviour
{
    public GameObject TargetLeftObject;
    public UISlider ValueSlider;
    public UILabel ValueLabel;

    public GameObject TargetRightObject;
    public UISlider OnOffSlider;
    public UILabel OnOFFlabel;

    private float m_value = 0.0f;
    public float OptionValue
    {
        set
        {
            ValueSlider.value = value;
            m_value = value;
        }
    }

    private bool m_optionOnoff = false;
    public bool OptionOnOff
    {
        set
        {
            m_optionOnoff = value;
            OnOffSlider.value = value ? 1.0f : 0.0f;
        }
        private get
        {
            return m_optionOnoff;
        }
    }

    private System.Action<float> m_changeValueFunc = null;
    public System.Action<float> ChangeValueFunc
    {
        set
        {
            m_changeValueFunc = value;
            ValueSlider.gameObject.SetActive(value != null);
        }
    }

    private System.Action<bool> m_changeOnoffFunc = null;
    public System.Action<bool> ChangeOnoffFunc
    {
        set
        {
            m_changeOnoffFunc = value;
            TargetRightObject.SetActive(value != null);
        }
    }

    public void OnChange_Value()
    {
        m_value = ValueSlider.value;
        if (m_changeValueFunc != null)
            m_changeValueFunc(m_value);
    }

    private bool m_isReverse = false;
    public void OnClickOnOff()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);

        if (m_myindex == (int)OptionType.GoogleConnect)
        {
            if (OptionOnOff)
            {
                PopupManager.Instance.ShowPopup(POPUP_TYPE.GuestLogin_Warning, OnClick_GuestWarning, null);
            }
            else
            {
                PlayerPrefabsID.SetLoginType(LOGIN_TYPE.GOOGLE, true);
                InGameUIScene.Instance.ChangeScene(SceneState.Start);
            }
            return;
        }

        if (m_myindex == (int)OptionType.FaceBookConnect)
        {
            PlayerPrefabsID.SetLoginType(LOGIN_TYPE.FACEBOOK, !OptionOnOff);
        }

        OptionOnOff = !OptionOnOff;
        if (m_changeOnoffFunc != null)
            m_changeOnoffFunc(m_isReverse ? !OptionOnOff : OptionOnOff);

        if (m_myindex == (int)OptionType.WholeSound)
            SendMessageUpwards("ResetData", SendMessageOptions.DontRequireReceiver);
    }

    private void OnClick_GuestWarning(params object[] parameters)
    {
        PlayerPrefabsID.SetLoginType(LOGIN_TYPE.GOOGLE, false);
        GoogleGamesManager.Instance.SignOut();
        InGameUIScene.Instance.ChangeScene(SceneState.Start);
    }

    private int m_myindex = 0;
    private UIOption_DataProperty m_myProperty = null;
    public void Init(int index)
    {
        m_myindex = index;

        UIOption_DataProperty property = UIOption_Table.Instance.GetOptionDataProperty((OptionType)index);
        if (property == null)
        {
            gameObject.SetActive(false);
            return;
        }

        m_myProperty = property;
        gameObject.SetActive(true);

        ValueLabel.text = property.OptionValueName;
        OnOFFlabel.text = property.OptionOnOFFName;
        m_isReverse = property.IsReverse;

        if (index == (int)OptionType.WholeSound)
        {
            ChangeValueFunc = null;
            ChangeOnoffFunc = SoundManager.Instance.MuteWhole;

            OptionValue = 0;
            OptionOnOff = !SoundManager.Instance.MutedWhole;
        }
        else if (index == (int)OptionType.BGMSound)
        {
            ChangeValueFunc = SoundManager.Instance.SetBGMVolume;
            ChangeOnoffFunc = SoundManager.Instance.MuteBGM;
            OptionValue = SoundManager.Instance.GetCurrentBGMVolume();
            OptionOnOff = !SoundManager.Instance.IsMutedBGM;
        }
        else if (index == (int)OptionType.SFXSound)
        {
            ChangeValueFunc = SoundManager.Instance.SetSFXVolume;
            ChangeOnoffFunc = SoundManager.Instance.MuteFX;
            OptionValue = SoundManager.Instance.GetCurrentSFXVolume();
            OptionOnOff = !SoundManager.Instance.IsMutedFX;
        }
        else if (index == (int)OptionType.GoogleConnect)
        {
            ChangeValueFunc = null;
            ChangeOnoffFunc = GoogleGamesManager.Instance.Sign;

            OptionValue = 0;
            OptionOnOff = GoogleGamesManager.Instance.IsSignIn();
        }
        else if (index == (int)OptionType.FaceBookConnect)
        {
            ChangeValueFunc = null;
            ChangeOnoffFunc = FaceBookManager.Instance.FaceBookLogin;

            OptionValue = 0;
            OptionOnOff = FaceBookManager.Instance.IsFaceBookLogin();
        }
        else if (index == (int)OptionType.MouseStrength)
        {
            ChangeValueFunc = SetMouseStrength;
            ChangeOnoffFunc = SetMouseMode;

            SetMouseMode(AccountManager.Instance.IsMouseMode);

            float strength = AccountManager.Instance.MouseStrength / 100.0f;
            SetMouseStrength(strength);

            OptionValue = strength;
            OptionOnOff = AccountManager.Instance.IsMouseMode;
        }
    }

    private void SetMouseMode(bool mode)
    {
        ValueSlider.gameObject.SetActive(mode);

        AccountManager.Instance.IsMouseMode = mode;
        AccountManager.Instance.MouseStrength = AccountManager.Instance.MouseStrength;
    }

    private void SetMouseStrength(float strength)
    {
        AccountManager.Instance.IsMouseMode = AccountManager.Instance.IsMouseMode;
        AccountManager.Instance.MouseStrength = (int)(strength * 100.0f);
        ValueLabel.text = string.Format("{0} {1}", m_myProperty.OptionValueName, (int)(strength * 100.0f));
    }
}
