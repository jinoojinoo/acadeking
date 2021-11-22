using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class UI_MemberList_Item : MonoBehaviour
{
    public GameObject[] MyTypeObjects;
    public UILabel NameLabel;
    public UILabel ReadyLabel;

    public Transform TargetTrnas;

    private MyNetworkLobbyPlayer m_lobbyPlayer = null;
    public void Init(MyNetworkLobbyPlayer player)
    {
        m_lobbyPlayer = player;

        m_lobbyPlayer.RefreshLobbyPlayerFunc -= Refresh;
        m_lobbyPlayer.RefreshLobbyPlayerFunc += Refresh;
    }

    public void Refresh()
    {
        MyTypeObjects[0].SetActive(m_lobbyPlayer.isLocalPlayer);
        MyTypeObjects[1].SetActive(!m_lobbyPlayer.isLocalPlayer);

        NameLabel.text = string.Format("{0}\n최고기록 {1}", m_lobbyPlayer.UserName, m_lobbyPlayer.RecordScore);
        string ready = m_lobbyPlayer.m_readyToBegin ? "[00FF00]Ready[-]" : "[FF0000]Wait[-]";
        ReadyLabel.text = ready;

//        NameLabel.text = string.Format("local : " + m_lobbyPlayer.isLocalPlayer + "\n netId " + m_lobbyPlayer.netId + "\n controllerid : " + m_lobbyPlayer.playerControllerId);
    }
}