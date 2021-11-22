using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rim : MonoBehaviour
{
    private Mesh m_myMesh = null;
    private const float COLLIDER_SIZE = 0.003f;
    private List<GameObject> m_listObject = new List<GameObject>();

    private const float m_Scale = 1.0f;

    void Start()
    {
        m_myMesh = this.GetComponent<MeshFilter>().mesh;
        Vector3 vec = m_myMesh.bounds.size;

        float radius = vec.x * 0.5f;
        Vector3 scale = transform.lossyScale;

        ClearObject();
        for (int theta = 0; theta < 360; theta += 10)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta) * scale.x, 0.0f, radius * Mathf.Sin(theta) * scale.z) * 0.95f;

            GameObject obj = new GameObject(theta.ToString());
            obj.transform.SetParent(this.transform, false);

            BoxCollider boxcollider = obj.AddComponent<BoxCollider>();
            m_listObject.Add(obj);
            boxcollider.size = new Vector3(COLLIDER_SIZE, COLLIDER_SIZE, COLLIDER_SIZE);

            Vector3 worldPt = transform.TransformPoint(m_myMesh.bounds.center);
            obj.transform.position = pos + worldPt;
            obj.tag = "rim";
        }

        transform.localScale *= m_Scale;
    }

    void ClearObject()
    {
        foreach (GameObject obj in m_listObject)
        {
            Destroy(obj);
        }
        m_listObject.Clear();
    }
}
