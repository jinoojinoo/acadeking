using UnityEngine;
using System.Collections;

public class UIGameWaitPacket : MonoBehaviour
{
    private static UIGameWaitPacket m_instance = null;
    public static UIGameWaitPacket Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject obj = ResourceManager.Instance.LoadResourceObject(UIResourcesNameDef.WaitPacket);
                DontDestroyOnLoad(obj);
                m_instance = obj.GetComponent<UIGameWaitPacket>();
            }

            return m_instance;
        }
    }

    private GameObject m_myObject = null;
    private GameObject MyObject
    {
        get
        {
            if (m_myObject == null)
                m_myObject = this.gameObject;
            return m_myObject;
        }
    }

    public void StartWaitPakcet()
    {
        MyObject.SetActive(true);
    }

    public void EndWaitPacket()
    {
        MyObject.SetActive(false);
    }
}
