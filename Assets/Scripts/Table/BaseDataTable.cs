using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class BaseDataProperty
{
    [XmlAttribute("ID")]
    public uint ID;

    [XmlAttribute("Name")]
    public string Name = string.Empty;
}

/*public static class XMLVector3
{
    public static string GetVector3ToStr(Vector3 src)
    {
        return string.Format("{0},{1},{2}", src.x, src.y, src.z);
    }

    public static void GetStrToVector3(string src, ref Vector3 dest)
    {
        if (string.IsNullOrEmpty(src))
            return;

        string[] list = src.Split(',');
        dest.x = float.Parse(list[0]);
        dest.y = float.Parse(list[1]);
        dest.z = float.Parse(list[2]);
    }
}

public static class XMLVector2
{
	public static string GetVector2ToStr(Vector2 src)
	{
		return string.Format("{0},{1}", src.x, src.y);
	}

	public static void GetStrToVector2(string src, ref Vector2 dest)
	{
		if (string.IsNullOrEmpty(src))
			return;

		string[] list = src.Split(',');
		dest.x = float.Parse(list[0]);
		dest.y = float.Parse(list[1]);
	}
}*/

public abstract class BaseDataTable<T1, T2>
    where T1 : BaseDataTable<T1,T2>, new()
    where T2 : BaseDataProperty
{
    private static T1 m_instance = null;
    public static T1 Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new T1();

                Debug.LogError("m_instance : " + m_instance);

#if D_Table_FromFile
                    m_instance.LoadTable_File();
#else
                m_instance.LoadTable();
#endif
            }

            return m_instance;
        }
    }

    public static bool IsLoaded()
    {
        return m_instance != null;
    }

    private static List<T2> m_table = null;
    public T2[] TableDataList
    {
        get
        {
            return m_table.ToArray();
        }
        set
        {
            m_table = new List<T2>(value);
        }
    }

//     public abstract T2[] DataList
//     {
//         get;
//         set;
//     }

    public List<T2> TableList
    {
        get
        {
//             if (m_table == null)
//                 LoadTable();

            return m_table;
        }
    }

    public void LoadTable()
    {
        if (m_table != null)
            return;

        //        LoadTable(GetTableName(typeof(T1)));
        LoadTable(typeof(T1).Name);
    }

    public void LoadTable(string tablename)
    {
        if (m_table != null)
            return;

        DataTableManager.Instance.LoadTable<T1>(tablename);
    }

    public void Clear()
    {
        if (null == m_table)
            return;

        m_table.Clear();
        m_table = null;
    }

    public int GetPropertyCount()
    {
        return TableList.Count;
    }

    public T2 GetPropertyByIndex(int index_)
    {
        return TableList[index_];
    }

    public T2 GetProperty(uint id)
    {
        T2 property = TableList.Find(rhs => rhs.ID == (int)id);

        if (null == property)
        {
            return null;
        }

        return property;
    }
    public T2 GetProperty(int id)
    {
        T2 property = TableList.Find(rhs => rhs.ID == id);

        if (null == property)
        {
            return null;
        }

        return property;
    }

    public T2 GetProperty(string name)
    {
        T2 property = TableList.Find(x => (string.Compare(x.Name, name) == 0));
        if (null == property)
        {
            return default(T2);
        }
        return property;
    }

    public int Count
    {
        get { return TableList.Count; }
    }

    public void Log()
    {
        foreach (T2 element in TableList)
        {
            Debug.Log(string.Format("ID : {0} Name : {1}", element.ID, element.Name));
        }

        Debug.Log(string.Format("TotalCount : {0}", TableList.Count));
    }

    public string GetTableName(Type serializedObjectType)
    {
        XmlRootAttribute theAttrib = Attribute.GetCustomAttribute(serializedObjectType, typeof(XmlRootAttribute)) as XmlRootAttribute;
        return theAttrib.ElementName;
    }

    public void MakeBinaryFile()
    {
        Type type = typeof(T1);
        DataTableManager.Instance.MakeBinaryFile(type, TableDataList);
    }

    public void SetSampleData()
    {
//         m_table = new List<T2>();
//         T2 property = new T2();
//         m_table.Add(property);
     }

    public void ResetSampleData()
    {
        m_table = null;
    }
}