using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

public enum OptionType
{
    WholeSound,
    BGMSound,
    SFXSound,
    GoogleConnect,
    FaceBookConnect,
    MouseStrength,
}

[System.Serializable]
public class UIOption_DataProperty : BaseDataProperty
{
    [XmlElement("OptionType")]
    public OptionType Option_Type;

    [XmlElement("OptionValueName")]
    public string OptionValueName;

    [XmlElement("OptionOnOFFName")]
    public string OptionOnOFFName;

    [XmlElement("View")]
    public bool IsView = true;

    [XmlElement("IsReverse")]
    public bool IsReverse = false;
}

[XmlRoot("TABLE")]
public class UIOption_Table : BaseDataTable<UIOption_Table, UIOption_DataProperty>
{
    [XmlElement("UIOption")]
    public UIOption_DataProperty[] DataList
    {
        get
        {
            return TableDataList;
        }
        set
        {
            TableDataList = value;
        }
    }

    public UIOption_DataProperty GetOptionDataProperty(OptionType mode)
    {
        return TableList.Find(x => x.Option_Type == mode);
    }
}