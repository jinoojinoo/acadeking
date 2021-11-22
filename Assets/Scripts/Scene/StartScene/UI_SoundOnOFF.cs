using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SoundOnOFF : MonoBehaviour
{
    public UISprite SoundOnOff;

    private const string SOUND_ON = "sound_on";
    private const string SOUND_OFF = "sound_off";

    private bool SoundOnOFF
    {
        set
        {
            SoundManager.Instance.MutedWhole = value;
            SoundOnOff.spriteName = SoundManager.Instance.MutedWhole ? SOUND_OFF : SOUND_ON;
            if (value)
                SoundManager.Instance.DisableSound();
            else
                SoundManager.Instance.EnableSound();
        }
    }
    void Start()
    {
        SoundOnOFF = SoundManager.Instance.MutedWhole;
    }

    public void OnClick_SoundOnOff()
    {
        SoundOnOFF = !SoundManager.Instance.MutedWhole;
    }
}
