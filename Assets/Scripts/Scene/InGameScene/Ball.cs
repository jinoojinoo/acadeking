using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBase:MonoBehaviour
{
    private Transform m_myTrans = null;
    protected Transform MyTrans
    {
        get
        {
            if (m_myTrans == null)
                m_myTrans = this.transform;
            return m_myTrans;
        }
    }

    private GameObject m_myObject = null;
    protected GameObject MyObject
    {
        get
        {
            if (m_myObject == null)
                m_myObject = this.gameObject;
            return m_myObject;
        }
    }
}
public class Ball : ObjectBase
{
    private Transform m_targetTrans = null;
    private Transform TargetTrans
    {
        get
        {
            if (m_targetTrans == null)
                m_targetTrans = GameObject.FindObjectOfType<BallTarget>().transform;

            return m_targetTrans;
        }
    }

    private BallType_DataProperty m_dataProperty = null;
    public int BallType
    {
        set
        {
            m_dataProperty = BallType_Table.Instance.GetBallTypeProperty(value);
        }
    }

    DelegateSecrueProperty<int> m_encryption_score = new DelegateSecrueProperty<int>();
//    private int m_ballScore = 2;
    public int BallScore
    {
        set
        {
            m_encryption_score.Value = value;
        }
        private get
        {
            return m_encryption_score.Value;
        }
    }

    private int m_myballNumber = 0;
    public int BallNumber
    {
        set
        {
            m_myballNumber = value;
        }
        private get
        {
            return m_myballNumber;
        }
    }

    private bool m_localBall = false;
    public bool IsLocalBall
    {
        set
        {
            m_localBall = value;
        }
        get
        {
            return m_localBall;
        }
    }

    private uint m_mynetid = 0;
    public uint MyNetId
    {
        set
        {
            m_mynetid = value;
        }
        private get
        {
            return m_mynetid;
        }
    }

    private int m_testballNumber = 0;
    public int TestBallNumber
    {
        set
        {
            m_testballNumber = value;
            StopCoroutine("AutoThrowBall");
            StartCoroutine("AutoThrowBall", value);
        }
    }

    private BallType_DataProperty m_balltypeProperty = null;
    public BallType_DataProperty BallTypeProperty
    {
        set
        {
            m_balltypeProperty = value;
            InitBallSize(MyObject, value.Shop_Type, false);
        }
    }

    private static Shader m_stencilShader = null;
    private static Shader StencilShader
    {
        get
        {
            if (m_stencilShader == null)
                m_stencilShader = Shader.Find("Custom/UIStencil");

            return m_stencilShader;
        }
    }

    public static void InitBallSize(GameObject obj, ShopType type, bool ui)
    {
        float size = GetBallSize(type);
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        Vector3 vec = mesh.bounds.size;
        obj.transform.localScale = Vector3.one * (1 / vec.x) * (size / GameUtil.BasketBall_SIZE);

        if (ui == false)
            return;

        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if (rigidbody != null)
            Destroy(rigidbody);

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Ball ball = obj.GetComponent<Ball>();
        if (ball != null)
            Destroy(ball);

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.material.shader = StencilShader;
        renderer.material.renderQueue = 3000 + 1;
    }

    public static float GetBallSize(ShopType type)
    {
        switch (type)
        {
            case ShopType.BasketBall:
                return GameUtil.BasketBall_SIZE;
            case ShopType.BaseBall:
                return GameUtil.BaseBall_SIZE;
            case ShopType.BeachBall:
                return GameUtil.BeachBall_SIZE;
            case ShopType.DodgeBall:
                return GameUtil.DodgeBall_SIZE;
            case ShopType.SoccerBall:
                return GameUtil.SoccerBall_SIZE;
            case ShopType.TennisBall:
                return GameUtil.TennisBall_SIZE;
            case ShopType.Volleyball:
                return GameUtil.Volleyball_SIZE;
            default:
                return GameUtil.BasketBall_SIZE;
        }
    }

    public void PlayBallBounceSound()
    {
        switch (m_balltypeProperty.Shop_Type)
        {
            case ShopType.BasketBall:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;

            case ShopType.BaseBall:
                SoundManager.Instance.PlaySound(UISOUND_ID.baseball_bounce);
                break;

            case ShopType.BeachBall:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;

            case ShopType.DodgeBall:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;

            case ShopType.SoccerBall:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;

            case ShopType.TennisBall:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;

            case ShopType.Volleyball:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;

            default:
                SoundManager.Instance.PlaySound(UISOUND_ID.basketball_bounce);
                break;
        }
    }

    private Vector3 m_defaultPosition = Vector3.zero;

    private Collider m_myCollidr = null;
    public Collider MyCollider
    {
        get
        {
            if (m_myCollidr == null)
                m_myCollidr = this.GetComponent<Collider>();

            return m_myCollidr;
        }
    }

    private PhysicMaterial PhysicMat
    {
        get
        {
            return MyCollider.material;
        }
    }

    private Material m_myMaterial = null;
    public Material MyMaterial
    {
        get
        {
            if (m_myMaterial == null)
            {
                MeshRenderer renderer = GetComponent<MeshRenderer>();
                m_myMaterial = renderer.material;
            }

            return m_myMaterial;
        }
    }

    private Rigidbody m_myRigidBody = null;
    public Rigidbody MyRigidBody
    {
        get
        {
            if (m_myRigidBody == null)
                m_myRigidBody = this.GetComponent<Rigidbody>();
            return m_myRigidBody;
        }
    }

    private Vector3 m_defaulttargetPosition;

    private void Awake()
    {
        MyObject.layer = LayerMask.NameToLayer(TAG_ID.TAG_Ball);
        MyObject.tag = TAG_ID.TAG_Ball;
    }

    private void Start()
    {
        m_defaultPosition = MyTrans.position;
        m_defaulttargetPosition = TargetTrans.position;
        Reset();

//         if (m_testballNumber == 0)
//         {
//             StopCoroutine("AutoThrowBall");
//             StartCoroutine("AutoThrowBall", -1);
//         }
    }

    private void Reset()
    {
        PhysicMat.dynamicFriction = GlobalValue_Table.Instance.dynamicFriction;
        PhysicMat.staticFriction = GlobalValue_Table.Instance.staticFriction;
        PhysicMat.bounciness = GlobalValue_Table.Instance.bounciness;
    }

//    private Vector3 m_click;
//    bool m_strenghtClick = false;
    Vector3 m_throwDir;
//    float m_power;
    Vector3 m_leftClickPos;

//    private float m_timeScale = 1.0f;

    void LateUpdate()
    {
        if (MyTrans.position.y < -10.0f)
        {
            ResetBall();
        }
        Quaternion deltaRotation = Quaternion.Euler(MyRigidBody.velocity);
        MyRigidBody.MoveRotation(MyRigidBody.rotation * deltaRotation);
    }

    private void ResetBall()
    {
        MyTrans.position = m_defaultPosition;
        InitVelocity(Vector3.zero);
        SendMessage(Vector3.zero);
        if (CatchBall.Instance.CurrentCatchBall == this)
            CatchBall.Instance.ResetBall();
    }

    private Camera m_myCamera = null;
    private Camera MyCamera
    {
        get
        {
            if (m_myCamera == null)
                m_myCamera = ArcadeKingManager.Instance.MyCamera;

            return m_myCamera;
        }
    }

    private Vector3 GetWorldToScreen(Vector3 position)
    {
        Vector3 pos = MyCamera.WorldToScreenPoint(position);
        pos.x *= UIRoot.FIT_WIDTH / Screen.width;
        pos.y *= UIRoot.FIT_HEIGHT / Screen.height;
        pos.x -= UIRoot.FIT_WIDTH / 2;
        pos.y -= UIRoot.FIT_HEIGHT / 2;
        pos.z = -1;

        return pos;
    }

    public void ThrowBall2(Vector3 mousepos, Vector3 lastmouse)
    {

    }

    private float GetAngle(Vector3 a, Vector3 b)
    {
        float angle = Vector3.Angle(a, b);
        Vector3 cross = Vector3.Cross(a, b);
        if (cross.y < 0)
            angle = -angle;

        return angle;
    }

    //     public void ThrowBall(Vector3 mousepos, Vector3 firstmouse, Vector3 lastmouse)
    //     {
    //         Debug.LogError("strenth : " + (lastmouse - mousepos).magnitude + " , 2: " + (lastmouse - mousepos).sqrMagnitude);
    //     }

    private void CheckAdvantage(ref float target, float range)
    {
        float min_target = 0.95f;
        float max_target = 1.05f;

        if (target < min_target && target > min_target - range)
        {
            Debug.LogError("Advantage min");
            target = min_target;
        }
        if (target > max_target && target < max_target + range)
        {
            Debug.LogError("Advantage max");
            target = max_target;
        }
    }
    public void ThrowBall(Vector3 mousepos, Vector3 lastmouse, float strength = float.MinValue)
    {
        Vector3 velocity = Vector3.zero;

        Vector3 world_mouse = MyCamera.ScreenToWorldPoint(new Vector3(mousepos.x, mousepos.y, MyCamera.nearClipPlane));
        Vector3 world_last = MyCamera.ScreenToWorldPoint(new Vector3(lastmouse.x, lastmouse.y, MyCamera.nearClipPlane));
        Debug.LogError("world_mouse : " + world_mouse);
        Debug.LogError("world_last : " + world_last);

        bool throw_right = world_mouse.x >= world_last.x;
        bool realdir_right = MyTrans.position.x < TargetTrans.position.x;
        Debug.LogError("throw_right : " + throw_right + " , realdir_right : " + realdir_right);

        Vector3 mypos = GetWorldToScreen(MyTrans.position);
        Vector3 targetpos = GetWorldToScreen(TargetTrans.position);
        float screenheight = Mathf.Abs(mypos.y - targetpos.y);
        float screenwidth = targetpos.x - mypos.x;

        

        Debug.LogError("mypos : " + mypos + " , targetpos : " + targetpos + " , screenheight : " + screenheight);

        // type 1 마우스 클릭 대비 움직임 만큼 도착 타켓을 움직여서 방향을 트는 방법
        //         float distance_x = (world_mouse - world_last).x * 50.0f;
        //         Vector3 target = TargetTrans.position;
        //         target.x += distance_x;
        //velocity = findInitialVelocity(MyTrans.position, target, 2.0f);

        // type 1 마우스 움직임에 실제 방향 설정
        // real
        //float distance_z = (mousepos.y - lastmouse.y) / screenheight;


        Debug.LogError("strenth : " + strength);
        // 설정
        float distance_z = 0.0f;
        if (AccountManager.Instance.IsMouseMode == false)
            distance_z = (mousepos.y - lastmouse.y) / GlobalValue_Table.Instance.ThrowMouseHeight;
        else
        {
            float real_dis = Vector3.Magnitude(targetpos - mypos);
            float mousestrength = AccountManager.Instance.MouseStrength;
            float weight = GlobalValue_Table.Instance.ThrowMouseStrength * (1.0f + mousestrength / 100.0f);
            distance_z = ((strength * weight) / real_dis);
            Debug.LogError("real_dis : " + real_dis + ", strength : " + strength + " , distance_z : " + distance_z);
        }

        if (ArcadeKingManager.Instance.CurrentRound == 0)
            CheckAdvantage(ref distance_z, 0.2f);
        else if (ArcadeKingManager.Instance.CurrentRound == 1)
            CheckAdvantage(ref distance_z, 0.1f);

        Debug.LogError("realposy : " + (mousepos.y - lastmouse.y) + " , distance_z : " + distance_z);

        float startz = MyTrans.position.z;
        float endz = TargetTrans.position.z;
        float targetz = Mathf.Lerp(startz, endz, distance_z);
        int count = (int)distance_z;
        if (count > 0)
        {
            float overrate = distance_z - count;
            float overz = Mathf.Lerp(startz, endz, overrate);
            overz -= startz;
            targetz += overz;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // x ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // real
        //        float distance_x = (lastmouse.x - mousepos.x) / screenwidth;
        // 설정

        Vector3 screen_dotpos = new Vector3(mypos.x, targetpos.y, mypos.z);
//        Debug.LogError("screen_dotpos : " + screen_dotpos);
        Vector3 mousedir = (mousepos - lastmouse).normalized;

        Vector3 realdir = (targetpos - mypos).normalized;
        Vector3 dotdir = (screen_dotpos - mypos).normalized;

        float dotangle = GetAngle(dotdir, realdir);
        float angle = GetAngle(mousedir, realdir);
        float testangle = GetAngle(dotdir, mousedir);

//        Debug.LogError("dotangle : " + dotangle + " , angle : " + angle + " , testangle : " + testangle);

        float distance_rate = 1.0f - (angle / dotangle);
//        Debug.LogError("distance_rate : " + distance_rate);
        // over
        if (dotangle < angle)
        {
            if (throw_right == realdir_right)
            {
                distance_rate = 1.0f + Mathf.Abs(distance_rate);
                Debug.LogError("over distance_rate  : " + distance_rate);
            }
            //             float realdot = Vector3.Dot(dotdir, realdir);
            //             float mousedot = Vector3.Dot(dotdir, mousedir);
            //             Debug.LogError("realdot : " + realdot + " , mousedot : " + mousedot);
            //             if (realdot*mousedot > 0.0f && realdot > mousedot)
            //             {
            //                 Debug.LogError("over");
            // //                distance_rate *= -1;
            //             }
        }

        float startx = MyTrans.position.x;
        float endx = TargetTrans.position.x;
        float targetx = startx + ((endx - startx) * distance_rate);

//        Debug.LogError("startz : " + startz + " , endz : " + endz + " , distance : " + distance_z + " , targetz : " + targetz);
        Vector3 target = TargetTrans.position;
        target.z = targetz;
        target.x = targetx;

        float adjustheight = GlobalValue_Table.Instance.ThrowHeigh * Mathf.Clamp01(distance_z);
        Vector3 ori = findInitialVelocity(MyTrans.position, TargetTrans.position, adjustheight);
        velocity = findInitialVelocity(MyTrans.position, target, adjustheight);
        m_throwDir = velocity;
        InitVelocity(m_throwDir);

//        Debug.LogError("ori : " + ori + ", velocity : " + velocity + " , TargetTrans.position : " + TargetTrans.position + " , target : " + target);
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(MyTrans.position, MyTrans.position + (m_throwDir * m_power));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(MyTrans.position, TargetTrans.position);

        Gizmos.color = Color.green;
        Vector3 point = GetPoint(TargetTrans.position, MyTrans.position);
        Gizmos.DrawLine(MyTrans.position, MyTrans.position + point);
    }*/

        private Vector3 GetPoint(Vector3 target, Vector3 ori)
    {
        Vector3 point = (target - ori) * 0.3f;
        point += Vector3.up * 4.0f;

        return point;
    }

    public Vector3 findInitialVelocity(Vector3 startPosition, Vector3 finalPosition, float maxHeightOffset = 0.6f, float rangeOffset = 0.11f)
    {
        Vector3 direction = finalPosition - startPosition;
        return findInitialVelocity(direction, startPosition, finalPosition, maxHeightOffset, rangeOffset);
    }

    private float m_totalFlightTime = 0.0f;
    private float m_startFlightTime = 0.0f;

    private Vector3 findInitialVelocity(Vector3 direction, Vector3 startPosition, Vector3 finalPosition, float maxHeightOffset = 0.6f, float rangeOffset = 0.11f)
    {
        float range = direction.magnitude;
        range += rangeOffset;
        Vector3 unitDirection = direction.normalized;

        float maxYPos = finalPosition.y + maxHeightOffset;
        if (range / 2f > maxYPos)
            maxYPos = range / 2f;
        Vector3 newVel = new Vector3();
        newVel.y = Mathf.Sqrt(-2.0f * Physics.gravity.y * (maxYPos - startPosition.y));

        float timeToMax = Mathf.Sqrt(-2.0f * (maxYPos - startPosition.y) / Physics.gravity.y);
        float timeToTargetY = Mathf.Sqrt(-2.0f * (maxYPos - finalPosition.y) / Physics.gravity.y);
        m_totalFlightTime = timeToMax + timeToTargetY;
        m_startFlightTime = Time.realtimeSinceStartup;
        float horizontalVelocityMagnitude = range / m_totalFlightTime;

        newVel.x = horizontalVelocityMagnitude * unitDirection.x;
        newVel.z = horizontalVelocityMagnitude * unitDirection.z;

//        Debug.LogError("time : " + 0 + " position :  " + startPosition + " , velocity : " + newVel);

        return newVel;
    }

    private bool m_checkScore = false;
    private bool CheckScore
    {
        set
        {
            m_checkScore = value;
            if (value == false)
            {
                MyRigidBody.velocity = Vector3.zero;
                MyRigidBody.Sleep();
            }
        }
    }

    private const string CHECK_UPPER = "Upper";
    private const string CHECK_LOWER = "Lower";
    private const string CHECK_BALL = "Ball";
    private const string CHECK_ClickBall = "CheckClickBall";

    private enum BALL_STATE
    {
        None,
        UPPER_IN,
        UPPER_OUT,
        LOWER_IN,
        LOWER_OUT,
    }
    private BALL_STATE m_ballState = BALL_STATE.None;
    private BALL_STATE MyBallState
    {
        set
        {
            m_ballState = value;
        }
        get
        {
            return m_ballState;
        }
    }

    private enum CheckClearShot
    {
        Throw,
        Collider,
        In,
    }
    private CheckClearShot m_clearShot = CheckClearShot.Throw;

    private void OnTriggerEnter(Collider other)
    {
        if (string.Compare(other.tag, CHECK_UPPER) == 0)
        {
            if (MyRigidBody.velocity.y <= 0)
            {
                if (m_clearShot == CheckClearShot.Throw)
                    m_clearShot = CheckClearShot.In;

                MyBallState = BALL_STATE.UPPER_IN;
            }
        }
        else if (string.Compare(other.tag, CHECK_LOWER) == 0)
        {
            if (MyRigidBody.velocity.y <= 0 && MyBallState == BALL_STATE.UPPER_IN)
                MyBallState = BALL_STATE.LOWER_IN;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_clearShot = CheckClearShot.Collider;

        GameObject obj = collision.gameObject;
        TAG_ID.TAG_TYPE type = TAG_ID.GetTagType(obj);
        switch (type)
        {
            case TAG_ID.TAG_TYPE.Backboard:
                SoundManager.Instance.PlaySound(UISOUND_ID.backboard);
                break;

            case TAG_ID.TAG_TYPE.Rim:
                SoundManager.Instance.PlaySound(UISOUND_ID.rimhit);
                break;

            case TAG_ID.TAG_TYPE.Chain:
                SoundManager.Instance.PlaySound(UISOUND_ID.hit_chain);
                break;

            case TAG_ID.TAG_TYPE.Bounce:
                if (MyRigidBody.velocity.y > 2.5)
                    PlayBallBounceSound();
                break;

            default:
                return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (string.Compare(other.tag, CHECK_UPPER) == 0)
        {
            if (MyBallState != BALL_STATE.LOWER_IN)
            {
                MyBallState = BALL_STATE.None;
            }
        }
        else if (string.Compare(other.tag, CHECK_LOWER) == 0)
        {
            if (MyBallState == BALL_STATE.LOWER_IN)
            {
                MyBallState = BALL_STATE.LOWER_OUT;
                SoundManager.Instance.PlaySound(UISOUND_ID.net_wire);
  //              Debug.LogError("bound speed : " + MyRigidBody.velocity);
                if (IsLocalBall)
                {
                    if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
                    {
                        ArcadeKingManager.Instance.AddScore(BallScore, m_clearShot == CheckClearShot.In);
                    }
                    else
                    {
                        MyNetworkManager.Instance.MyGamePlayer.CmdAddScore(BallScore, m_clearShot == CheckClearShot.In);
                        SendMessage(Vector3.zero);
                    }
                }
            }
        }
    }

    IEnumerator AutoThrowBall(int number)
    {
        bool istestball = false;
        if (number < 0)
            istestball = true;

        if (istestball)
            yield return new WaitForSeconds(2); 
        else
            yield return new WaitForSeconds(2 + number);

        float waittime = -1;
        while (true)
        {
            if (istestball && InGameManager.Instance.GameState != InGameManager.GAME_STATE.Play)
            {
                yield return null;
                continue;
            }

            if (CatchBall.Instance.UnAvailableList.Contains(this))
            {
                waittime = -1;
                yield return null;
                continue;
            }

            if (waittime == -1)
            {
                waittime = Time.realtimeSinceStartup + UnityEngine.Random.Range(1.0f, 5.0f);
                continue;
            }

            if (waittime > Time.realtimeSinceStartup)
            {
                yield return null;
                continue;
            }

            waittime = -1;

            InitVelocity(findInitialVelocity(MyTrans.position, TargetTrans.position, GlobalValue_Table.Instance.ThrowHeigh));

            yield return new WaitForSeconds(2.0f);
            continue;
        }
    }

    public void InitVelocity(Vector3 velocity)
    {
        if (velocity == Vector3.zero)
        {
            MyRigidBody.velocity = velocity;
            MyRigidBody.Sleep();
            MyObject.layer = LayerMask.NameToLayer("Ball");
            return;
        }

        m_clearShot = CheckClearShot.Throw;
        MyRigidBody.velocity = velocity;
        MyRigidBody.WakeUp();
        
        MyObject.layer = LayerMask.NameToLayer("ThrowBall");
        StopCoroutine("ChangeLayer");
        StartCoroutine("ChangeLayer");
        SendMessage(velocity);
    }

    IEnumerator ChangeLayer()
    {
        yield return new WaitForSeconds(0.3f);
        MyObject.layer = LayerMask.NameToLayer("Ball");
        yield break;
    }

    private void SendMessage(Vector3 velocity)
    {
        if (InGameManager.Instance.CurrentPlayMode != InGameManager.PLAY_MODE.Multiple)
            return;
        if (IsLocalBall == false)
            return;

        NetworkMessageManager.Instance.SendMessage(Network_ID.CS_ThrowBall,
            MyNetId,
            BallNumber,
            ArcadeKingManager.Instance.GameProgressTime,
            MyTrans.localPosition,
            velocity);
    }

    public Vector3 TargetDir
    {
        get
        {
            Vector3 mypos = MyCamera.WorldToScreenPoint(MyTrans.position);
            Vector3 targetpos = MyCamera.WorldToScreenPoint(TargetTrans.position);
            return targetpos - mypos;
        }
    }
    public Vector3 TargetPosition
    {
        get
        {
            return GetWorldToScreen(TargetTrans.position);
        }
    }

    public float TargetDistance(Vector3 mousepos, Vector3 lastmouse)
    {
        Vector3 mypos = MyCamera.WorldToScreenPoint(MyTrans.position);
        Vector3 targetpos = MyCamera.WorldToScreenPoint(TargetTrans.position);
        float distance_z = (mousepos.y - lastmouse.y) / GlobalValue_Table.Instance.ThrowMouseHeight;

        return distance_z;
    }

}
