using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform TargetTrans;
    void Start()
    {
        Quaternion rotation = Quaternion.LookRotation(TargetTrans.position, Vector3.up);
        transform.rotation = rotation;
    }
}
