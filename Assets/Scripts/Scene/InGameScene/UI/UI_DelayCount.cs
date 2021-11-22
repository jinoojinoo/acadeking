using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DelayCount : ResetUIComponent
{
    public int StartCount = 0;
    private int m_playCount = 0;
    public UILabel CountLabel;

    private double m_startTime = 0;

    public override InGameManager.GAME_STATE AdjustGameState
    {
        get
        {
            return InGameManager.GAME_STATE.PlayCount;
        }
    }

    public override int Order
    {
        get
        {
            return 0;
        }
    }

    public override void ResetComponent(bool reset)
    {
        MyObject.SetActive(reset);
        if (reset == false)
            return;

        m_startTime = ArcadeKingManager.Instance.GameStartTime;
        m_playCount = -1;
        CountLabel.text = m_playCount.ToString();
    }

    private void SetCount(int count)
    {
        if (count == 0)
            SoundManager.Instance.PlaySound(UISOUND_ID.whistle);
        else
            SoundManager.Instance.PlaySound(UISOUND_ID.beep3);

        CountLabel.text = string.Format("{0}", count);
    }

    private void LateUpdate()
    {
        if (m_startTime == -1)
            m_startTime = ArcadeKingManager.Instance.GameStartTime;

        double time = ArcadeKingManager.Instance.GameProgressTime - m_startTime;
//        Debug.LogError("m_playCount : " + m_playCount + " , time  : " + time + " , " + ArcadeKingManager.Instance.GameProgressTime + " , " + ArcadeKingManager.Instance.GameStartTime);

        if (time > StartCount + 1)
        {
            InGameManager.Instance.GameState = InGameManager.GAME_STATE.Play;
            return;
        }

        int count = StartCount - (int)time;
        if (count < 0)
            return;

        if (m_playCount != count)
        {
            m_playCount = count;
            SetCount(count);
        }

        time -= (int)time;
        float section1 = 0.3f;
        float section2 = 1.0f - section1;

        if (time < section1)
        {
            MyTrans.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5, (float)time / section1);
        }
        else
        {
            MyTrans.localScale = Vector3.Lerp(Vector3.one * 5, Vector3.zero, ((float)time - section1) / section2);
        }

        
    }
}
