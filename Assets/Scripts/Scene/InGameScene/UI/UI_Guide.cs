using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Guide : ObjectBase
{
    public Transform BackTrans;
    public Transform GaugeTrans;
    public UIProgressBar ProgressBar;
    public UISprite ProgressSprite;
    public GameObject ViewObject;
    public UILabel GaugeLabel;

    private Vector3 m_leftClickPos;
    private Vector3 m_clickpos;
    private Ball m_targetBall;
    public Ball TargetBall
    {
        set
        {
            if (AccountManager.Instance.IsMouseMode)
            {
                ViewObject.SetActive(false);
                return;
            }

            m_targetBall = value;

            ViewObject.SetActive(value != null);
            if (value == null)
                return;

            m_targetTrans = value.transform;

            m_clickpos = Input.mousePosition;
            m_leftClickPos = ArcadeKingManager.Instance.MyCamera.WorldToScreenPoint(TargetTrans.position);
            Debug.LogError("m_leftClickPos : " + m_leftClickPos + " , mouse input : " + Input.mousePosition);
            UpdatePosition();

            GaugeLabelTrans.rotation = Quaternion.identity;
            GaugeLabelTrans.localPosition = m_gaugelabelPosition;
        }
        private get
        {
            return m_targetBall;
        }
    }

    private Transform m_targetTrans = null;
    private Transform TargetTrans
    {
        get
        {
            return m_targetTrans;
        }
    }

    private Camera m_myCamera = null;
    private Camera MyCamera
    {
        get
        {
            if (m_myCamera == null)
                m_myCamera = InGameUIScene.Instance.MyCamera;
            return m_myCamera;
        }
    }

    private Vector3 m_gaugelabelPosition = new Vector3(-30, 70, 0);
    private Transform m_gaugelabelTrans = null;
    private Transform GaugeLabelTrans
    {
        get
        {
            if (m_gaugelabelTrans == null)
                m_gaugelabelTrans = GaugeLabel.transform;
            return m_gaugelabelTrans;
        }
    }

    void LateUpdate()
    {
//        Debug.LogError("m_leftClickPos : " + m_leftClickPos + " , Input.mousePosition: " + Input.mousePosition);

        if (TargetBall == null || AccountManager.Instance.IsMouseMode)
            return;

        UpdatePosition();

        Vector3 dir = TargetBall.TargetPosition - MyTrans.localPosition;
        dir.Normalize();
        float atan = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        BackTrans.eulerAngles = new Vector3(0, 0, atan);

        Vector3 mousedir = Input.mousePosition - m_clickpos;
        float atan2 = Mathf.Atan2(mousedir.y, mousedir.x) * Mathf.Rad2Deg;
        GaugeTrans.eulerAngles = new Vector3(0, 0, atan2);

        float value = TargetBall.TargetDistance(Input.mousePosition, m_clickpos);

        float adjustvalue = value * 0.8f;
        ProgressBar.value = adjustvalue;
        ProgressSprite.color = adjustvalue <= 0.8f ? Color.green : Color.Lerp(Color.green, Color.red, (adjustvalue - 0.8f) * 5.0f);

        float perpower = (adjustvalue / 0.8f) * 100.0f;
        if (perpower < 0.0f)
            perpower = 0.0f;
        if (perpower > 120.0f)
            perpower = 120.0f;
        GaugeLabel.text = string.Format("{0}%", (int)perpower);
        GaugeLabel.color = ProgressSprite.color;

        Vector3 defaultpos = m_gaugelabelPosition;
        defaultpos.y *= (atan2 < atan && atan2 > -atan) ? -1 : 1;

        GaugeLabelTrans.rotation = Quaternion.identity;
        GaugeLabelTrans.localPosition = defaultpos;
//         if (GaugeTrans.position.y >= GaugeLabelTrans.position.y)
//         {
//             defaultpos = GaugeLabelTrans.position;
//             defaultpos.y = GaugeTrans.position.y;
//             GaugeLabelTrans.position = defaultpos;
//         }
    }

    private void UpdatePosition()
    {
        if (TargetBall == null)
            return;

//         if (ArcadeKingManager.Instance.CurrentRound >= 2)
//         {
//             MyObject.SetActive(false);
//             return;
//         }

        Vector3 pos = ArcadeKingManager.Instance.MyCamera.WorldToScreenPoint(TargetTrans.position);
        pos.x *= UIRoot.FIT_WIDTH / Screen.width;
        pos.y *= UIRoot.FIT_HEIGHT / Screen.height;
        pos.x -= UIRoot.FIT_WIDTH / 2;
        pos.y -= UIRoot.FIT_HEIGHT / 2;
        pos.z = -1;
        MyTrans.localPosition = pos;
//         Vector3 pos = UI_InGameScene.Instance.MyCamera.ScreenToWorldPoint(m_leftClickPos);
//         pos.z = 0;
//         MyTrans.position = pos;
    }
}
