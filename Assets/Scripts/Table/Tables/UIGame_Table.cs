using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

[System.Serializable]
public class UIGame_DataProperty : BaseDataProperty
{
    [XmlElement("UI_TYPE")]
    public GAME_UI_MODE MODE_ID;

    [XmlElement("PATH")]
    public string Path;
}

[XmlRoot("TABLE")]
public class UIGame_Table : BaseDataTable<UIGame_Table, UIGame_DataProperty>
{
    [XmlElement("UIGame")]
    public UIGame_DataProperty[] DataList
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

    public UIGame_DataProperty GetUIGameDataProperty(GAME_UI_MODE mode)
    {
        return TableList.Find(x => x.MODE_ID == mode);
    }
}