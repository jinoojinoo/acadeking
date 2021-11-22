using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

public enum UISOUND_ID
{
    None = -1,

    Button_Click = 0,
    Roll,
    Wrong,
    Changescene,

    //bgm
    BGM_Start,
    BGM_InGame,

    //ingame
    basketball_bounce,
    baseball_bounce,
    beachball_bounce,
    dodgeball_bounce,
    soccerball_bounce,
    tennisball_bounce,
    volleyball_bounce,

    hit_chain,
    backboard,
    net,
    net_wire,
    whistle,
    rimhit,

    beep2,
    beep3,

    Winning,
    Fault,
}

public enum UISOUND_TYPE
{
    SFX,
    BGM,
    Ambient,
}

[System.Serializable]
public class UISound_DataProperty : BaseDataProperty
{
    [XmlElement("SOUND_ID")]
    public UISOUND_ID SoundID;

    [XmlElement("SOUND_TYPE")]
    public UISOUND_TYPE SoundType = UISOUND_TYPE.SFX;

    [XmlElement("PATH")]
    public string Path;
}

[XmlRoot("TABLE")]
public class UISound_Table : BaseDataTable<UISound_Table, UISound_DataProperty>
{
    [XmlElement("UISound")]
    public UISound_DataProperty[] DataList
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

    public UISound_DataProperty GetUISoundDataProperty(UISOUND_ID id)
    {
        return TableList.Find(x => x.SoundID == id);
    }
}