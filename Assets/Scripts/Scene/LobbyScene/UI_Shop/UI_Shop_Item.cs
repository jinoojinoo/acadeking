using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class UI_Shop_Item : InfiniteObjectInfo
{
    protected const int DEFAULT_LAYER = 50;

    public UISprite SelectSprite;
    public UISprite ItemTexture;

    public UILabel PriceLabel;
    public UILabel ItemNameLabel;

    public GameObject EquipObject;
    public GameObject UnEquipObject;
    public GameObject PriceObject;

    private Shop_DataProperty m_mydataProperty = null;
    public Shop_DataProperty MyDataProperty
    {
        get
        {
            return m_mydataProperty;
        }
    }

    private GameObject m_itemObject;
    private GameObject ItemObject
    {
        set
        {
            if (m_itemObject != null)
            {
                Destroy(m_itemObject);
                m_itemObject = null;
            }
            m_itemObject = value;
        }
        get
        {
            return m_itemObject;
        }
    }
    
    public void Init(Shop_DataProperty property, bool isselect)
    {
        m_mydataProperty = property;

        if (property.IsProduct())
        {
            if (IAPManager.Instance.HadPurchased(property.ProductID))
                MyItemState = UI_Shop.ItemState.Buyed;
            else
                MyItemState = UI_Shop.ItemState.Buy;
        }
        else
        {
            if (AccountManager.Instance.CurrentSelectBallInfo != null)
            {
                if (AccountManager.Instance.CurrentSelectBallInfo.BallType == property.TYPE)
                {
                    MyItemState = UI_Shop.ItemState.UnEquip;
                }
                else
                {
                    if (AccountManager.Instance.GetAvailableSkinCount((int)property.TYPE) <= 0)
                        MyItemState = UI_Shop.ItemState.Buy;
                    else
                        MyItemState = UI_Shop.ItemState.Equip;
                }
            }
            else
                MyItemState = UI_Shop.ItemState.Buy;
        }

        OnSelectItem(isselect);
        if (isselect)
            MessageEventObject.SendMessageUpwards("OnClickItemSelect", this);

        ItemTexture.spriteName = property.SpriteName;
        ItemTexture.enabled = !string.IsNullOrEmpty(property.SpriteName);
        CreateObject(m_mydataProperty);
        InitPrice();
    }

    private UI_Shop.ItemState m_myitemState = UI_Shop.ItemState.Buy;
    public UI_Shop.ItemState MyItemState
    {
        private set
        {
            m_myitemState = value;
            EquipObject.SetActive(value == UI_Shop.ItemState.Equip);
            UnEquipObject.SetActive(value == UI_Shop.ItemState.UnEquip);
            PriceObject.SetActive(value == UI_Shop.ItemState.Buy);
        }
        get
        {
            return m_myitemState;
        }
    }


    private void CreateObject(Shop_DataProperty property)
    {
        if (string.IsNullOrEmpty(property.ObjectPath))
        {
            ItemObject = null;
            return;
        }

        Transform trans = ItemTexture.transform;
        ItemObject = ResourceManager.Instance.LoadResourceObject(property.ObjectPath);
        ItemObject.transform.parent = trans;
        ItemObject.transform.localPosition = Vector3.zero;

        if (property.IsBall())
        {
            GameUtil.InitUIBallSize(ItemObject, property.Shop_Type, this.gameObject.layer);
        }
        else
        {
            ItemObject.transform.localScale = Vector3.one;
        }

        GameUtil.SetGameObjectLayer(ItemObject, this.gameObject.layer);        
    }

    private void InitPrice()
    {
        if (m_mydataProperty.IsProduct())
        {
            NumberFormatInfo numberFormat = new CultureInfo("ko-KR", false).NumberFormat;
            PriceLabel.text = m_mydataProperty.Price.ToString("c", numberFormat);
        }
        else
        {
            PriceLabel.text = string.Format("{0} Gold",
                GameUtil.GetColorLabel(m_mydataProperty.Gold, AccountManager.Instance.MyAccountInfo.Gold));
        }

        ItemNameLabel.text = m_mydataProperty.ItemName;
        if (m_mydataProperty.IsProduct() == false)
        {
            int count = AccountManager.Instance.GetAvailableSkinCount((int)m_mydataProperty.TYPE);
            if (count > 0)
                ItemNameLabel.text += string.Format(StringTable.UNEQUIP_COUNT, count);
            else
            {
                int equipcount = AccountManager.Instance.GetEquipSkinCount((int)m_mydataProperty.TYPE);
                if (equipcount > 0 && m_mydataProperty.TYPE != AccountManager.Instance.CurrentSelectBallInfo.BallType)
                    ItemNameLabel.text += string.Format(StringTable.EQUIP_COUNT, 0, equipcount);
            }
        }
    }

    private void OnClick()
    {
        this.MessageEventObject.SendMessage("OnClickItemSelect", this);
    }

    public void OnSelectItem(bool isselect)
    {
        SelectSprite.gameObject.SetActive(isselect);
    }

    public void OnClick_Buy()
    {
        Debug.LogError("OnClick_Buy");
        this.MessageEventObject.SendMessage("OnClickItem", this);
    }

    private void OnDestroy()
    {
        ItemObject = null;
    }

    public void OnClick_Equip()
    {
        Debug.LogError("OnClick_Equip");
        this.MessageEventObject.SendMessage("OnClickItem", this);
    }

    public void OnClick_UnEquip()
    {
        Debug.LogError("OnClick_UnEquip");
        this.MessageEventObject.SendMessage("OnClickItem", this);
    }
}