using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public partial class MyNetworkManager : NetworkManager
{
    private int m_connectCount = 1;
    public int ConnectCount
    {
        get
        {
            return m_connectCount;
        }
    }
    public void SetConnectCount()
    {
        m_connectCount = InGamePlayerList.Count;
    }
}
