using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : GameUISequence
{
    public override void StartGameSequence(int option)
    {
        
    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        return -1;
    }

    public UI_InventoryScrollView ScrollView;
    public UILabel MyGoldLabel;

    private void Start()
    {
        ScrollView.Initialize();
        AccountManager.Instance.MyAccountInfo.MyGoldFunc += SetMyGold;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        AccountManager.Instance.MyAccountInfo.MyGoldFunc -= SetMyGold;
        AccountManager.Instance.CurrentSelectBallInfo = null;
    }

    public void SetMyGold(int gold)
    {
        if (DestoryObject)
            return;

        MyGoldLabel.text = string.Format("{0:#,0}", gold);
        ScrollView.ResetData();
    }

    public void OnClick_Subject()
    {
#if UNITY_EDITOR
        AccountManager.Instance.MyAccountInfo.Gold += 1000;
#endif
    }
}
