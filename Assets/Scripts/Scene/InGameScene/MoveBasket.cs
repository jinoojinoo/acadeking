using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBasket : ResetUIComponent
{
    public override InGameManager.GAME_STATE AdjustGameState
    {
        get
        {
            return InGameManager.GAME_STATE.Play;
        }
    }

    public override int Order
    {
        get
        {
            return 0;
        }
    }

    private bool m_ismove = false;
    public override void ResetComponent(bool reset)
    {
        m_ismove = false;
        if (reset == false)
            return;

        m_ismove = ArcadeKingManager.Instance.SettingProperty.MoveRim != 0.0f;
        m_moveSpeed = ArcadeKingManager.Instance.SettingProperty.MoveRim;
    }

    private const float MOVE_RANGE = 1.25f;
    // Update is called once per frame

    private bool m_move_right = false;
    private float m_moveSpeed = 1.0f;

    void LateUpdate()
    {
        if (m_ismove == false)
            return;

        Vector3 pos = MyTrans.localPosition;
        float x = pos.x;

        x += (m_moveSpeed * Time.deltaTime) * (m_move_right ? 1.0f : -1.0f);
        if (Mathf.Abs(x) > MOVE_RANGE)
        {
            x = MOVE_RANGE * (x >= 0.0f ? 1.0f : -1.0f);
            m_move_right = !m_move_right;
        }

        pos.x = x;
        MyTrans.localPosition = pos;
    }
}
