using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Notice_WaitForIngame : PopupBase
{
    public GameObject[] Buttons;
    public UILabel[] ButtonLabel;
    public UIButton StartButton;

    private void Awake()
    {
        InGameManager.GAME_PAUSE = true;
    }

    private void OnDestroy()
    {
        InGameManager.GAME_PAUSE = false;
    }

    public override void Init(UIPopup_DataProperty property)
    {
        base.Init(property);
        InitButtonLabel(property, ButtonLabel, 1);
    }

    private bool m_isserver = false;
    public void InitPopup(bool isserver)
    {
        m_isserver = isserver;
        StartButton.gameObject.SetActive(isserver);
        StartButton.isEnabled = false;
    }

    void Update ()
    {
        if (m_isserver == false)
            return;

        if (StartButton.isEnabled)
            return;

        if (MyNetworkManager.Instance.IsInitComplete() == false)
            return;

        foreach (MyNetworkGamePlayer player in MyNetworkManager.Instance.InGamePlayerList.Values)
        {
            if (player.ReadyForGame == false)
                return;

        }

        StartButton.isEnabled = true;
        StopCoroutine("AutoStart");
        StartCoroutine("AutoStart");
    }

    IEnumerator AutoStart()
    {
        yield return new WaitForSeconds(3.0f);
        OnClick_OK();
        yield break;             
    }
}
