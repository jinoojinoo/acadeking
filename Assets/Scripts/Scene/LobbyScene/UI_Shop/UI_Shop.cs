using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Shop : GameUISequence
{
    protected const int DEFAULT_LAYER = 50;

    public enum ItemState
    {
        Buy,
        Equip,
        UnEquip,
        Buyed,
    }

    public GameObject TabsObject;
    private Dictionary<ShopType, UISprite> m_tabList = new Dictionary<ShopType, UISprite>();

    public UI_ShopScrollView ScrollView;

    public UILabel MyGoldLabel;

    private ShopType m_currentTab = ShopType.None;
    private ShopType CurrentTab
    {
        set
        {
            if (m_currentTab == value)
                return;

            m_currentTab = value;
            foreach (UISprite sprite in m_tabList.Values)
                sprite.color = Color.white;
            m_tabList[m_currentTab].color = Color.gray;

            ScrollView.CurrentTab = value;
        }
    }

    private void Start()
    {
        ScrollView.Initialize();
        AccountManager.Instance.MyAccountInfo.MyGoldFunc += SetMyGold;

        CheckTab();
    }

    private void CheckTab()
    {
        m_tabList.Clear();

        bool isproduct = AccountManager.Instance.CurrentSelectBallInfo == null;
        if (isproduct)
            CheckTabCoin();
        else
            CheckTabNormal();
        
    }

    private void CheckTabNormal()
    {
        UI_Shop_Tab[] tablist = TabsObject.GetComponentsInChildren<UI_Shop_Tab>();
        for (int i = 0; i < tablist.Length; ++i)
        {
            tablist[i].MyObject.SetActive(i < (int)ShopType.Max);

            if (i < (int)ShopType.Max)
            {
                tablist[i].MyTab = (ShopType)i;
                m_tabList.Add((ShopType)i, tablist[i].GetComponent<UISprite>());
            }
        }

        CurrentTab = ShopType.BasketBall;
    }

    private void CheckTabCoin()
    {
        UI_Shop_Tab[] tablist = TabsObject.GetComponentsInChildren<UI_Shop_Tab>();
        for (int i = 0; i < tablist.Length; ++i)
        {
            tablist[i].MyObject.SetActive(false);
        }

        AddTab(tablist[0], ShopType.GOLD);
        AddTab(tablist[1], ShopType.DisableAD);
        CurrentTab = ShopType.GOLD;
    }

    private void AddTab(UI_Shop_Tab tab, ShopType type)
    {
        tab.MyObject.SetActive(true);
        tab.MyTab = type;
        m_tabList.Add(type, tab.GetComponent<UISprite>());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        AccountManager.Instance.MyAccountInfo.MyGoldFunc -= SetMyGold;
    }

    public void OnClick_Tab(ShopType tab)
    {
        CurrentTab = tab;
    }

    public override void StartGameSequence(int option)
    {
        
    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        return 0;
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

    public void OnClick_Coupon()
    {
        GameUIManager.Instance.PushSequence(GAME_UI_MODE.UI_Coupon);
    }
}
