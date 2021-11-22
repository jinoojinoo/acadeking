using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchBall : SingletonManager<CatchBall>
{
    public UI_Guide m_uiGuide = null;

    private Ball m_catchBall = null;
    public Ball CurrentCatchBall
    {
        get
        {
            return m_catchBall;
        }
        private set
        {
            if (m_catchBall != null)
                m_catchBall.MyMaterial.SetColor("_OutlineColor", Color.white);

            if (m_catchBall != value)
                m_strenghtClick = false;

            m_catchBall = value;

            if (m_uiGuide != null)
                m_uiGuide.TargetBall = value;

            if (value != null)
            {
                value.gameObject.layer = LayerMask.NameToLayer("ThrowBall");
                value.MyMaterial.SetColor("_OutlineColor", Color.green);
            }

//             StopCoroutine("CheckMousePos");
//             if (value != null)
//                 StartCoroutine("CheckMousePos");
        }
    }

    public List<Ball> m_leftballList = new List<Ball>();
    public List<Ball> LeftBallList
    {
        get
        {
            m_leftballList.RemoveAll(x => x == null);
            return m_leftballList;
        }
    }

    public List<Ball> m_unavailableList = new List<Ball>();
    public List<Ball> UnAvailableList
    {
        get
        {
            m_unavailableList.RemoveAll(x => x == null);
            return m_unavailableList;
        }
    }

    public void ResetBall()
    {
        CurrentCatchBall = null;
        m_strenghtClick = false;
    }


    bool m_strenghtClick = false;
    Vector3 m_leftClickPos;
    public Vector3 LeftClickPos
    {
        get
        {
            return m_leftClickPos;
        }
    }

    private Vector3 m_lastmousePos;
    private Vector3 m_drawmousePos;

    private Camera m_mainCamera = null;
    private Camera MainCamera
    {
        get
        {
            if (m_mainCamera == null)
                m_mainCamera = ArcadeKingManager.Instance.MyCamera;

            return m_mainCamera;
        }
    }

    private void Start()
    {
        ResetBall();
    }

    private void Update()
    {
        if (InGameManager.Instance.GameState != InGameManager.GAME_STATE.Play)
            return;
        if (MainCamera == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            int layerMask = 1 << LayerMask.NameToLayer("Ball");
            if (Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue, layerMask) && hitInfo.transform.tag == "Ball")
            {
                Ball clickball = hitInfo.collider.GetComponent<Ball>();
                if (UnAvailableList.Contains(clickball) == false &&
                    clickball != CurrentCatchBall)
                {
                    CurrentCatchBall = clickball;
                    Vector3 pos = CurrentCatchBall.transform.position;
                    pos.y += 0.1f;
                    CurrentCatchBall.transform.position = pos;

                    m_strenghtClick = true;
                    m_leftClickPos = Input.mousePosition;
                    CurrentCatchBall.MyRigidBody.Sleep();
                    CurrentCatchBall.MyRigidBody.velocity = Vector3.zero;
                    CurrentCatchBall.MyRigidBody.useGravity = false;
                }
            }
        }
         
        if (CurrentCatchBall)
        {
            //             if (Input.GetMouseButtonDown(0))
            //             {
            //                 m_strenghtClick = true;
            //                 m_leftClickPos = Input.mousePosition;
            //             }

            if (Input.GetMouseButtonUp(0) && m_strenghtClick)
            {
                m_strenghtClick = false;
                CurrentCatchBall.MyRigidBody.WakeUp();
                CurrentCatchBall.MyRigidBody.useGravity = true;

                float strengthx = Input.mousePosition.x - m_lastmousePos.x;
                strengthx = Mathf.Pow(strengthx, 2);
                float strengthy = (Input.mousePosition.y - m_lastmousePos.y) * 2;
                strengthy = Mathf.Pow(strengthy, 2);
                float strength = Mathf.Sqrt(strengthx + strengthy);

                strength = Vector3.Magnitude(Input.mousePosition - m_lastmousePos);

                m_drawmousePos = Input.mousePosition;
//                UnityEngine.Debug.LogError("!!!!!!!!!!!!!!!!!!!! ");
//                UnityEngine.Debug.LogError("!!!!!!!!!!!!!!!!!!!! strength : " + strength);
//                UnityEngine.Debug.LogError("!!!!!!!!!!!!!!!!!!!! Input.mousePosition : " + Input.mousePosition);
//                UnityEngine.Debug.LogError("!!!!!!!!!!!!!!!!!!!! m_lastmousePos : " + m_lastmousePos);
//                UnityEngine.Debug.LogError("!!!!!!!!!!!!!!!!!!!! ");
                CurrentCatchBall.ThrowBall(Input.mousePosition, m_leftClickPos, strength);
                CurrentCatchBall = null;
            }
        }

        if (Input.GetMouseButtonDown(0) && m_strenghtClick)
        {
            m_lastmousePos = Input.mousePosition;
        }
    }

    public void ThrowBall(Ball target)
    {
        if (InGameManager.Instance.GameState != InGameManager.GAME_STATE.Play)
            return;

        target.MyRigidBody.WakeUp();
        target.ThrowBall(Input.mousePosition, m_leftClickPos);
    }

    IEnumerator CheckMousePos()
    {
//        UnityEngine.Debug.LogError("set mousepos ");
        m_lastmousePos = Input.mousePosition;

        while(true)
        {
            yield return new WaitForSeconds(0.3f);
//            UnityEngine.Debug.LogError("reset mousepos ");
            m_lastmousePos = Input.mousePosition;
            continue;
        }
    }
/*
    private void OnDrawGizmos()
    {
        Vector3 a1 = Camera.main.ScreenToViewportPoint(m_lastmousePos);
        Vector3 a2 = Camera.main.ScreenToViewportPoint(m_drawmousePos);

        Gizmos.DrawLine(a1, a2);
    }*/
}
