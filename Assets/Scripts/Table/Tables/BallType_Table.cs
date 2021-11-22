using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

using System.Linq;

[System.Serializable]
public class BallType_DataProperty : BaseDataProperty
{
    [XmlElement("ShopType")]
    public ShopType Shop_Type = ShopType.BasketBall;

    [XmlElement("TYPE")]
    public int TYPE;

    [XmlElement("PATH")]
    public string Path;

    [XmlElement("BallName")]
    public string BallName;

    [XmlElement("VIEW")]
    public bool View = true;
}

[XmlRoot("TABLE")]
public class BallType_Table : BaseDataTable<BallType_Table, BallType_DataProperty>
{
    private static Dictionary<int, BallType_DataProperty> m_ballList = null;
    public static Dictionary<int, BallType_DataProperty> BallList
    {
        get
        {
            if (m_ballList == null)
            {
                BallType_Table.Instance.LoadTable();
            }
            return m_ballList;
        }
    }

    [XmlElement("BallType")]
    public BallType_DataProperty[] DataList
    {
        get
        {
            return TableDataList;
        }
        set
        {
            TableDataList = value;

            if (m_ballList == null)
                m_ballList = new Dictionary<int, BallType_DataProperty>();
            else
                m_ballList.Clear();

            foreach (BallType_DataProperty data in value)
            {
                if (data == null ||
                    data.View == false)
                {
                    continue;
                }

                m_ballList.Add(data.TYPE, data);
            }
        }
    }

    public BallType_DataProperty GetBallTypeProperty(int type)
    {
        return TableList.Find(x => x.TYPE == type);
    }

    public BallType_DataProperty GetBallTypePropertyRandom()
    {
        List<BallType_DataProperty> list = BallList.Values.ToList();
        int random = UnityEngine.Random.Range(0, list.Count);
        return list[random];
    }
}