using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

public enum UIGAMEOBJECT_TYPE
{
    None = -1,
    SoundManager,
    BasketBall,

    NetworkManager,
    Network_InGamePlayer,
    MemberItem,

    Arcade,
    ArcadeNew,
    Arcade_Camera,
    ArcadeManager,
    Network_TimeManager,
    UI_Guide,
}

[System.Serializable]
public class UIGameObject_DataProperty : BaseDataProperty
{
    [XmlElement("TYPE")]
    public UIGAMEOBJECT_TYPE TYPE;

    [XmlElement("PATH")]
    public string Path;

    [XmlIgnore]
    public TableHelper.TableCommaValue<float> POSITION_LIST = new TableHelper.TableCommaValue<float>();
    [XmlIgnore]
    public Vector3 Position
    {
        get
        {
            return TableHelper.ListToVector3(POSITION_LIST, Vector3.zero);
        }
    }
    [XmlElement("POSITION")]
    public string Position_Str
    {
        get
        {
            return POSITION_LIST;
        }
        set
        {
            POSITION_LIST = value;
        }
    }

    [XmlIgnore]
    public TableHelper.TableCommaValue<float> SCALE_LIST = new TableHelper.TableCommaValue<float>();
    [XmlIgnore]
    public Vector3 Scale
    {
        get
        {
            return TableHelper.ListToVector3(SCALE_LIST, Vector3.one);
        }
    }
    [XmlElement("SCALE")]
    public string SCALE_Str
    {
        get
        {
            return SCALE_LIST;
        }
        set
        {
            SCALE_LIST = value;
        }
    }

    [XmlElement("LAYER")]
    public string LAYER;    
}

[XmlRoot("TABLE")]
public class UIGameObject_Table : BaseDataTable<UIGameObject_Table, UIGameObject_DataProperty>
{
    [XmlElement("UIGameObject")]
    public UIGameObject_DataProperty[] DataList
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

    public UIGameObject_DataProperty GetGameObjectProperty(UIGAMEOBJECT_TYPE type)
    {
        return TableList.Find(x => x.TYPE == type);
    }

    public string GetGameObjectPath(UIGAMEOBJECT_TYPE type)
    {
        return GetGameObjectProperty(type).Path;
    }
}