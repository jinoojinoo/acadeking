using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PredictingRoot : SingletonMono<PredictingRoot>
{
    public GameObject item;
//	public Vector3 position = Vector3.zero;
//	public Vector3 velocity = Vector3.zero;
	public float time = 0f;
	public float interval = 0.1f;

	public float beginingTime = 1.0f;
	public float endingTime = 2.0f;

    public Transform BallTrans;
    public Transform TargetTrans;

    private List<GameObject> m_listobject = new List<GameObject>();

	float currentInterval = 0f;
	void Start ()
    {
		currentInterval = beginingTime;
	}

    private void ClearObject()
    {
        foreach (GameObject obj in m_listobject)
        {
            Destroy(obj);
        }

        m_listobject.Clear();
    }

    public void CreatePridicting(Vector3 startpos, Vector3 endpos, Vector3 velocity, float flighttime)
    {
        ClearObject();

        Debug.LogError("start : " + velocity + " , gravity :  " + GlobalValue_Table.Instance.GravityVector + " , position : " + startpos);
        float time = 0.0f;
        float increate_time = (flighttime / 10.0f);
        bool downvel = false;
        Vector3 pos = startpos;
        for (int i = 0; i < 10; ++i)
        {
            time += increate_time;
            // 1            pos += (velocity * increate_time);
            pos = CreatePridictingRate(startpos, endpos, velocity, flighttime, time);
            Create(time, pos);
//            Debug.LogError("i : " + i + " , time : " + time + " , pos : " + pos + " , velocity : " + velocity);
            
//  1           velocity.y += (GlobalValue_Table.Instance.GravityVector.y * increate_time * GlobalValue_Table.Instance.TimeScale);
//  1           if (downvel == false && velocity.y < 0.0f)
//  1           {
//  1               downvel = true;
//  1
//  1               float diff = velocity.y;
//  1               velocity.y = 0.0f;
//  1               float difftime = Mathf.Abs(diff) / Mathf.Abs(GlobalValue_Table.Instance.GravityVector.y * GlobalValue_Table.Instance.TimeScale);
//  1               time -= (increate_time - difftime);
//  1           }
        }
    }

    private Vector3 CreatePridictingRate(Vector3 startpos, Vector3 endpos, Vector3 velocity, float flighttime, float currenttime)
    {
        Vector3 current_vel = velocity;
        Vector3 gravity = GlobalValue_Table.Instance.GravityVector;
        return PredictProjectileAtTime(currenttime , velocity, startpos, gravity);
    }

    /*	void Update()
        {
            Vector3 velocity = Ball.findInitialVelocity(BallTrans.position, TargetTrans.position, 2.0f);

            if (Time.time >= beginingTime && Time.time <= endingTime)
            {
                if(Time.time >= currentInterval)
                {
                    currentInterval += interval;
                    Vector3 newPosition = PredictProjectileAtTime (currentInterval, velocity, BallTrans.position, gravity); 
                    Create(currentInterval, newPosition);
                }
            }
        }*/

    void Create(float time, Vector3 position)
    {
        if (item == null)
            return;

        GameObject go = Instantiate(item, position, Quaternion.identity) as GameObject;
        m_listobject.Add(go);

        Rigidbody rigidbody = go.GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        Collider collider = go.GetComponent<Collider>();
        collider.enabled = false;

        go.name = "Time - " + time.ToString() + " second";
        go.transform.parent = transform;
    }

	Vector3 PredictProjectileAtTime(float t, Vector3 v0, Vector3 x0, Vector3 g)
	{
		return g * (0.5f * t * t) + v0 * t + x0;
	}
}
