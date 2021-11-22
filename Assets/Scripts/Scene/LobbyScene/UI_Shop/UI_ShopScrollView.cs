using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ShopScrollView : CustomInfiniteScrollViewMono<UI_Shop_Item>
{
    private const int MAX_VIEW_ITEMCOUNT = 2;

    private ShopType m_currentTab = ShopType.None;
    public ShopType CurrentTab
    {
        set
        {
            if (m_currentTab == value)
                return;

            m_currentTab = value;
            InitData();

            SelectIndex = 0;
        }
    }

    private List<int> m_myInventoryList = new List<int>();
    private List<Shop_DataProperty> m_gameitemList = null;

    private int m_maxCount = 0;
    protected override void InitData()
    {
        if (m_currentTab ==  ShopType.None)
        {
            ScrollView.DataNum = 0;
            ScrollView.Init();
            return;
        }

        m_maxCount = 0;

        m_gameitemList = Shop_Table.Instance.GetShopItemList(m_currentTab);
        m_maxCount = m_gameitemList.Count;

        ScrollView.DataNum = m_gameitemList.Count;
        ScrollView.Init();
        UIScrollView view = m_ScrollViewObject.GetComponent<UIScrollView>();
        view.enabled = m_gameitemList.Count <= MAX_VIEW_ITEMCOUNT ? false : true;

        ResetPosition();
    }

    protected override UI_Shop_Item InitObject(GameObject obj, int dataPos)
    {
        UI_Shop_Item item = base.InitObject(obj, dataPos);
        if (item == null)
        {
            obj.SetActive(false);
            return null;
        }

        if (dataPos >= m_maxCount)
        {
            obj.SetActive(false);
            return null;
        }

        if (m_gameitemList.Count <= dataPos)
            return null;

        item.Init(m_gameitemList[dataPos], dataPos == m_selectIndex);
        return item;
    }

    private int m_selectIndex = -1;
    private int SelectIndex
    {
        set
        {
            UI_Shop_Item olditem = GetItem(m_selectIndex);
            if (olditem != null)
                olditem.OnSelectItem(false);

            m_selectIndex = value;
            UI_Shop_Item selectitem = GetItem(m_selectIndex);
            if (selectitem != null)
                selectitem.OnSelectItem(true);
        }
    }

    private void OnClickItemSelect(UI_Shop_Item item)
    {
        m_selectItem = item;
        SelectIndex = item.DataPos;
    }

    private void OnClickItem(UI_Shop_Item item)
    {
        OnClickItemSelect(item);

        if (item.MyItemState == UI_Shop.ItemState.Buy)
            PopupItemBuy(item);
        else if (item.MyItemState == UI_Shop.ItemState.Equip)
            PopupItemEquip(item);
        else if (item.MyItemState == UI_Shop.ItemState.UnEquip)
            PopupItemUnEquip(item);
    }

    /// <summary>
    /// 
    /// </summary>
    private UI_Shop_Item m_selectItem = null;
    private void PopupItemBuy(UI_Shop_Item item)
    {
        if (item.MyDataProperty.IsProduct() == false)
        {
            PopupBase popupbase = PopupManager.Instance.ShowPopup(POPUP_TYPE.Item_Buy, OnClick_ItemBuy);
            popupbase.MsgLabel.text = string.Format(StringTable.ASK_BUY_ITEM, item.MyDataProperty.ItemName);
        }
        else
        {
            IAPManager.Instance.Purchase(item.MyDataProperty.ProductID);
        }
    }

    private void OnClick_ItemBuy(params object[] paramters)
    {
        if (GameUtil.CheckMyGold(m_selectItem.MyDataProperty.Gold) == false)
            return;

        AccountManager.Instance.MyAccountInfo.Gold -= m_selectItem.MyDataProperty.Gold;
        AccountManager.Instance.CurrentSelectBallInfo.UpdateBallType(m_selectItem.MyDataProperty.TYPE);
        AccountManager.Instance.SetSkin(m_selectItem.MyDataProperty.TYPE);
        AccountManager.Instance.UpdateBallInfo();
        ResetData();
    }

    private void PopupItemEquip(UI_Shop_Item item)
    {
        PopupBase popupbase = PopupManager.Instance.ShowPopup(POPUP_TYPE.Item_Equip, OnClick_ItemEquip);
        popupbase.MsgLabel.text = string.Format(StringTable.ASK_EQUIP_ITEM, item.MyDataProperty.ItemName);
    }

    private void OnClick_ItemEquip(params object[] paramters)
    {
        AccountManager.Instance.CurrentSelectBallInfo.UpdateBallType(m_selectItem.MyDataProperty.TYPE);
        AccountManager.Instance.UpdateBallInfo();
        ResetData();
    }

    private void PopupItemUnEquip(UI_Shop_Item item)
    {
        PopupBase popupbase = PopupManager.Instance.ShowPopup(POPUP_TYPE.Item_UnEquip, OnClick_ItemUnEquip);
        popupbase.MsgLabel.text = string.Format(StringTable.ASK_UNEQUIP_ITEM, item.MyDataProperty.ItemName);
    }

    private void OnClick_ItemUnEquip(params object[] paramters)
    {
        AccountManager.Instance.CurrentSelectBallInfo.UpdateBallType(0);
        AccountManager.Instance.UpdateBallInfo();
        ResetData();
    }
}
