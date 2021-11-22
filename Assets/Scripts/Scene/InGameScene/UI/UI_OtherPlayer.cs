using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_OtherPlayer : ResetUIComponent
{
    public UITexture TargetTexture;
    public UILabel OnOffLabel;

    private bool m_onoffTexture = true;
    private bool OnOffTexture
    {
        set
        {
            m_onoffTexture = value;
            TargetTexture.gameObject.SetActive(value);
            OnOffLabel.text = value ? "[00FF00]ON[-]" : "[FF0000]OFF[-]";
        }
        get
        {
            return m_onoffTexture;
        }
    }

    public override InGameManager.GAME_STATE AdjustGameState
    {
        get
        {
            return InGameManager.GAME_STATE.PlayReady;
        }
    }

    public override int Order
    {
        get
        {
            return 0;
        }
    }

    private void Start()
    {
        OnOffTexture = OnOffTexture;
    }

    public override void ResetComponent(bool reset)
    {
        if (reset == false)
        {
            if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
                MyObject.SetActive(false);
            else if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.TEST)
            {
                TargetTexture.mainTexture = ArcadeKingManager.Instance.GetRenderTexture((int)1);
            }
            return;
        }

        MyObject.SetActive(InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Multiple &&
                            MyNetworkManager.Instance.InGamePlayerList.Count >= 2
                            );

        if (InGameManager.Instance.CurrentPlayMode == InGameManager.PLAY_MODE.Single)
            return;

        foreach (MyNetworkGamePlayer player in MyNetworkManager.Instance.InGamePlayerList.Values)
        {
            if (player.IsLocalGamePlayer)
                continue;

            TargetTexture.mainTexture = ArcadeKingManager.Instance.GetRenderTexture((int)player.LobbyNetID);
        }
    }

    public void OnClick_OnoffTexture()
    {
        OnOffTexture = !OnOffTexture;
    }
}
