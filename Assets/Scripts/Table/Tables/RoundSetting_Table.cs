using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

[System.Serializable]
public class RoundSetting_DataProperty : BaseDataProperty
{
    [XmlElement("ROUND")]
    public int Round;

    [XmlElement("SCORE")]
    public int Score;

    [XmlElement("TIME")]
    public float Time;

    [XmlElement("MOVE_RIM")]
    public float MoveRim;
}

[XmlRoot("TABLE")]
public class RoundSetting_Table : BaseDataTable<RoundSetting_Table, RoundSetting_DataProperty>
{
    [XmlElement("RoundSetting")]
    public RoundSetting_DataProperty[] DataList
    {
        get
        {
            return TableDataList;
        }
        set
        {
            TableDataList = value;

            foreach (RoundSetting_DataProperty data in value)
            {
                if (m_maxRound < data.Round)
                    m_maxRound = data.Round;
            }
        }
    }

    public RoundSetting_DataProperty GetRoundSettingProperty(int round)
    {
        return TableList.Find(x => x.Round == round);
    }

    private static int m_maxRound = -1;
    public int MaxRound
    {
        get
        {
            return m_maxRound;
        }
    }
}