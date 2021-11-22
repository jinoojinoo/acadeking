using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

public enum ShopType
{
    None = -1,
    BasketBall,
    BaseBall,
    BeachBall,
    DodgeBall,
    SoccerBall,
    TennisBall,
    Volleyball,
    GOLD,
    Max,
    DisableAD,
    FootBall,
}

[System.Serializable]
public class Shop_DataProperty : BaseDataProperty
{
//    ENG_Encryption m_encryption = new ENG_Encryption("62306084");

    [XmlElement("ShopType")]
    public ShopType Shop_Type;

    [XmlElement("Index")]
    public int Index;

    [XmlElement("TYPE")]
    public int TYPE;

    [XmlElement("SpriteName")]
    public string SpriteName;

    [XmlElement("ItemName")]
    public string ItemName;

    [XmlIgnore]
    private DelegateSecrueProperty<int> m_encryption_Gold = null;
    [XmlElement("Gold")]
    public int Gold
    {
        get
        {
            return m_encryption_Gold.Value;
        }
        set
        {
            if (m_encryption_Gold == null)
            {
                m_encryption_Gold = new DelegateSecrueProperty<int>();
                m_encryption_Gold.InitSecrue();
            }
            m_encryption_Gold.Value = value;
        }
    }

    [XmlIgnore]
    private DelegateSecrueProperty<int> m_encryption_Price = null;
    [XmlElement("Price")]
    public int Price
    {
        get
        {
            return m_encryption_Price.Value;
        }
        set
        {
            if (m_encryption_Price == null)
            {
                m_encryption_Price = new DelegateSecrueProperty<int>();
                m_encryption_Price.InitSecrue();
            }
            m_encryption_Price.Value = value;
        }
    }

    [XmlElement("ProductID")]
    public string ProductID;

    [XmlElement("ObjectPath")]
    public string ObjectPath = string.Empty;

    public bool IsProduct()
    {
        return string.IsNullOrEmpty(ProductID) == false;
    }
         
    public bool IsBall()
    {
        switch (Shop_Type)
        {

            case ShopType.BasketBall:
            case ShopType.BaseBall:
            case ShopType.BeachBall:
            case ShopType.DodgeBall:
            case ShopType.SoccerBall:
            case ShopType.TennisBall:
            case ShopType.Volleyball:
            case ShopType.FootBall:
                return true;

            default:
                return false;
        }
    }
}

[XmlRoot("TABLE")]
public class Shop_Table : BaseDataTable<Shop_Table, Shop_DataProperty>
{
    private static Dictionary<ShopType, List<Shop_DataProperty>> m_shopitemList = new Dictionary<ShopType, List<Shop_DataProperty>>();
    [XmlElement("ShopItem")]
    public Shop_DataProperty[] DataList
    {
        get
        {
            return TableDataList;
        }
        set
        {
            TableDataList = value;
            foreach(Shop_DataProperty data in value)
            {
                if (m_shopitemList.ContainsKey(data.Shop_Type))
                {
                    m_shopitemList[data.Shop_Type].Add(data);
                }
                else
                {
                    List<Shop_DataProperty> list = new List<Shop_DataProperty>();
                    list.Add(data);
                    m_shopitemList.Add(data.Shop_Type, list);
                }
            }
        }
    }

    public List<Shop_DataProperty> GetShopItemList(ShopType type)
    {
        if (m_shopitemList.ContainsKey(type) == false)
            return new List<Shop_DataProperty>();

        return m_shopitemList[type];
    }

    public Shop_DataProperty GetShopItemByProductID(string id)
    {
        return TableList.Find(x => string.Compare(x.ProductID, id) == 0);
    }
}