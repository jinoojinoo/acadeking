using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.Xml.Serialization;
using System;
using System.IO;


public abstract class BaseOnlyDataTable<T1> where T1 : class, new()
{
    private static T1 m_instance = null;
    public static T1 Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = LoadTable();
            }

            return m_instance;
        }
    }

    public static T1 LoadTable()
    {
        if (m_instance != null)
            return m_instance;

        return LoadTable(typeof(T1).Name);
    }

    public static T1 LoadTable(string tablename)
    {
        if (m_instance != null)
            return m_instance;

        T1 instane = DataTableManager.Instance.LoadTable<T1>(tablename);
        return instane;
    }

    public static void ReloadTable()
    {
        m_instance = null;
        LoadTable(typeof(T1).Name);
    }

    public static void Save<T>(string filename, object xmlobj) where T : class
    {
        string xmlfilename = Application.dataPath + "/Resources/Table/XML/" + filename + ".xml";
        //		Debug.Log(xmlfilename);
        TextWriter stream = new StreamWriter(Path.Combine(Application.dataPath, xmlfilename));
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(stream, xmlobj);
        stream.Close();
    }
}

[XmlRoot("TABLE")]
public class GlobalValue_Table : BaseOnlyDataTable<GlobalValue_Table>
{
    ENG_Encryption m_encryption = new ENG_Encryption("62306084");

    [XmlElement("ASPECT_X")]
    public float ASPECT_X;
    [XmlElement("ASPECT_Y")]
    public float ASPECT_Y;
    [XmlElement("dynamicFriction")]
    public float dynamicFriction;
    [XmlElement("staticFriction")]
    public float staticFriction;
    [XmlElement("bounciness")]
    public float bounciness;

    [XmlIgnore]
    public Vector3 GravityVector
    {
        get
        {
            return TableHelper.CommaStringToVector3(graviry);
        }
        set
        {
            graviry = TableHelper.Vector3ToCommaString(value);
        }
    }

    [XmlIgnore]
    private string graviry;
    [XmlElement("gravity")]
    public string gravitystr
    {
        get
        {
            return graviry;
        }
        set
        {
            graviry = value;
        }
    }

    [XmlIgnore]
    public Vector3 ThrowPowerVector
    {
        get
        {
            return TableHelper.CommaStringToVector3(ThrowPower);
        }
        set
        {
            ThrowPower = TableHelper.Vector3ToCommaString(value);
        }
    }

    [XmlIgnore]
    private string throwPowerStr;
    [XmlElement("ThrowPower")]
    public string ThrowPower
    {
        get
        {
            return throwPowerStr;
        }
        set
        {
            throwPowerStr = value;
        }
    }

    [XmlElement("ThrowHeigh")]
    public float ThrowHeigh;

    [XmlElement("ThrowMouseHeight")]
    public float ThrowMouseHeight;

    [XmlElement("ThrowMouseStrength")]
    public float ThrowMouseStrength;

    [XmlElement("ThrowMouseWeight")]
    public int ThrowMouseWeight;

    [XmlElement("ThrowStrength")]
    public float ThrowStrength;

    [XmlElement("TimeScale")]
    public float TimeScale;

    [XmlElement("REWARD_GOLD")]
    public int REWARD_GOLD;

    [XmlElement("MAP_SIZE_X")]
    public float MAP_SIZE_X;
    [XmlElement("MAP_SIZE_Y")]
    public float MAP_SIZE_Y;
    [XmlElement("MOVE_OFFSET_LEFT")]
    public float MOVE_OFFSET_LEFT;
    [XmlElement("MOVE_OFFSET_RIGHT")]
    public float MOVE_OFFSET_RIGHT;
    [XmlElement("MOVE_OFFSET_TOP")]
    public float MOVE_OFFSET_TOP;
    [XmlElement("MOVE_OFFSET_BOTTOM")]
    public float MOVE_OFFSET_BOTTOM;

    [XmlElement("AD_REWARD_GOLD")]
    public int RewardGold
    {
        get
        {
            return m_encryption_Reward.Value;
        }
        set
        {
            if (m_encryption_Reward== null)
            {
                m_encryption_Reward = new DelegateSecrueProperty<int>();
                m_encryption_Reward.InitSecrue();
            }
            m_encryption_Reward.Value = value;
        }
    }

    [XmlElement("DefaultCameraPosition")]
    public string DefaultCameraPosition;
    [XmlIgnore]
    public Vector3 DefaultCameraPositionVec
    {
        get
        {
            return TableHelper.CommaStringToVector3(DefaultCameraPosition);
        }
        set
        {
            DefaultCameraPosition = TableHelper.Vector3ToCommaString(value);
        }
    }

    [XmlElement("RoundTime1")]
    public float RoundTime1
    {
        get
        {
            return m_encryption_RoundTime1.Value;
        }
        set
        {
            if (m_encryption_RoundTime1 == null)
            {
                m_encryption_RoundTime1 = new DelegateSecrueProperty<float>();
                m_encryption_RoundTime1.InitSecrue();
            }
            m_encryption_RoundTime1.Value = value;
        }
    }
    [XmlElement("RoundTime2")]
    public float RoundTime2
    {
        get
        {
            return m_encryption_RoundTime2.Value;
        }
        set
        {
            if (m_encryption_RoundTime2 == null)
            {
                m_encryption_RoundTime2 = new DelegateSecrueProperty<float>();
                m_encryption_RoundTime2.InitSecrue();
            }
            m_encryption_RoundTime2.Value = value;
        }
    }

    [XmlElement("RoundTime3")]
    public float RoundTime3
    {
        get
        {
            return m_encryption_RoundTime3.Value;
        }
        set
        {
            if (m_encryption_RoundTime3 == null)
            {
                m_encryption_RoundTime3 = new DelegateSecrueProperty<float>();
                m_encryption_RoundTime3.InitSecrue();
            }
            m_encryption_RoundTime3.Value = value;
        }
    }

    public float GetRoundTime(int index)
    {
        if (index > 3)
            return RoundTime3;

        switch (index)
        {
            case 0:
                return RoundTime1;
            case 1:
                return RoundTime2;
            case 2:
                return RoundTime3;

            default:
                return RoundTime3;
        }
    }

    [XmlElement("Cost_Point")]
    public int CostPoint
    {
        get
        {
            return m_encryption_CostPoint.Value;
        }
        set
        {
            if (m_encryption_CostPoint == null)
            {
                m_encryption_CostPoint = new DelegateSecrueProperty<int>();
                m_encryption_CostPoint.InitSecrue();
            }
            m_encryption_CostPoint.Value = value;
        }
    }

    [XmlElement("Cost_AddSlot")]
    public int CostAddSlot
    {
        get
        {
            return m_encryption_CostAddSlot.Value;
        }
        set
        {
            if (m_encryption_CostAddSlot == null)
            {
                m_encryption_CostAddSlot = new DelegateSecrueProperty<int>();
                m_encryption_CostAddSlot.InitSecrue();
            }
            m_encryption_CostAddSlot.Value = value;
        }
    }

    public void LoadEnvironment()
    {
        SetTime(TimeScale);
        Physics.gravity = GlobalValue_Table.Instance.GravityVector;
    }

    private void SetTime(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [XmlIgnore]
    private DelegateSecrueProperty<int> m_encryption_Reward = null;
    [XmlIgnore]
    public int AD_REWARD_GOLD
    {
        get
        {
            return m_encryption_Reward.Value;
        }
    }

    [XmlIgnore]
    private DelegateSecrueProperty<int> m_encryption_CostPoint = null;
    [XmlIgnore]
    public int Cost_Point
    {
        get
        {
            return m_encryption_CostPoint.Value;
        }
    }

    [XmlIgnore]
    private DelegateSecrueProperty<int> m_encryption_CostAddSlot = null;
    [XmlIgnore]
    public int Cost_AddSlot
    {
        get
        {
            return m_encryption_CostAddSlot.Value;
        }
    }
    [XmlIgnore]
    private DelegateSecrueProperty<float> m_encryption_RoundTime1 = null;
    [XmlIgnore]
    private DelegateSecrueProperty<float> m_encryption_RoundTime2 = null;
    [XmlIgnore]
    private DelegateSecrueProperty<float> m_encryption_RoundTime3 = null;

}
