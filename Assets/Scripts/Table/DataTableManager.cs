using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataTableManager
{
    private const string DEFAULT_FILE_EXTENSION = ".xml";
    private const string DEFUALT_FILE_FOLDER = "/Resources/XML/";
    private const string BINARY_FILE_FOLDER = "BIN/";
    private const string BINARY_FILE_EXTENSION = ".bin";

    private static DataTableManager m_instance = null;
    public static DataTableManager Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new DataTableManager();

            return m_instance;
        }
    }

    public string CheckBinaryFilePath(string tablename)
    {
        string path = GetResourceFolder();
        string filename = string.Format("{0}{1}{2}{3}", path, BINARY_FILE_FOLDER, tablename, BINARY_FILE_EXTENSION);
        if (File.Exists(filename) == false)
            return string.Empty;

        return filename;
    }

    public T LoadTable<T>(string strTableName_) where T : class
    {
        string path = CheckBinaryFilePath(strTableName_);
        bool loadbinary = false;
#if LOAD_BINARY
        loadbinary = Application.isPlaying;
#endif

        if (string.IsNullOrEmpty(path) || loadbinary == false)
        {
            return LoadTable(strTableName_) as T;
        }

        return InitializeBinary<T>(path) as T;
    }

    public object LoadTable(string strTableName_)
    {
        string filename = string.Format("{0}{1}", "Table/XML/", strTableName_);
        TextAsset asset = null;

        if (Application.isPlaying)
        {
            asset = ResourceManager.Instance.LoadResource<TextAsset>(filename);
        }
        else
        {
#if UNITY_EDITOR
            if (AssetBundles.AssetBundleManager.SimulateAssetBundleInEditor)
                asset = ResourceManager.LoadResourceFromPC<TextAsset>(filename);
            else
                asset = Resources.Load<TextAsset>(filename);
#endif
        }

        try
        {
            return InitializeTable(asset.ToString(), strTableName_);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogError("FileName:" + filename);
        }

        return null;
    }

    public static object InitializeTable(string data, string typename)
    {
        string[] filename = typename.Split('/');
        typename = filename[filename.Length - 1];

        object xmlobj = null;
        System.Reflection.Assembly assem = System.Reflection.Assembly.GetExecutingAssembly();

        Type type = assem.GetType(typename);

        XmlSerializer serializer = new XmlSerializer(type);
        StringReader xmlstream = new StringReader(data);
        XmlTextReader xmlReader = new XmlTextReader(xmlstream);

        try
        {
            xmlobj = serializer.Deserialize(xmlReader);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("ErrorTable :" + typename);
            Debug.LogError("Error Message : " + ex.Message);
        }

        xmlReader.Close();
        xmlstream.Close();
        return xmlobj;
    }


    string GetResourceFolder()
    {
        string path = Application.dataPath + DEFUALT_FILE_FOLDER;
        return path;
    }

    private object InitializeBinary<T>(string path)
    {
        try
        {
            Type type = typeof(T);
            object obj = Activator.CreateInstance(type);
            System.Reflection.PropertyInfo info = type.GetProperty("DataList");

            StreamReader sr = new StreamReader(path);
            BinaryFormatter bin = new BinaryFormatter();
            object value = bin.Deserialize(sr.BaseStream);
            info.SetValue(obj, value, null);

            sr.Close();

            return value;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }

        return null;
    }

    private object InitializeTable(Stream stream, string type)
    {
        object xmlobj = null;
        StreamReader streamReader = new StreamReader(stream);

        XmlSerializer toData = new XmlSerializer(Type.GetType(type));

        try
        {
            xmlobj = toData.Deserialize(streamReader);
        }
        catch (FormatException ex)
        {
            Debug.Log("Error :" + ex.Data);
        }

        streamReader.Close();
        return xmlobj;
    }

    public void SaveSampleXML(string classname)
    {
        System.Type type = System.Type.GetType(classname);
        
        object obj = Activator.CreateInstance(type);
        System.Reflection.MethodInfo info = type.GetMethod("SetSampleData", System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        info.Invoke(obj, null);

        SaveSampleXML(type, obj);

        info = type.GetMethod("ResetSampleData", System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        info.Invoke(obj, null);
    }

    private void SaveSampleXML(Type type, object xmlobj)
    {
        string xmlfilename = string.Format("{0}{1}{2}{3}",
            Application.dataPath,
//            "/Resources/XML/Sample_",
            "/",
             type.Name,
            ".xml");

        var serializer = new XmlSerializer(type);
        var stream = new FileStream(xmlfilename, FileMode.Create);
        serializer.Serialize(stream, xmlobj);
        stream.Close();
    }    

    public void MakeBinaryFile(string filename)
    {
        System.Type type = System.Type.GetType(filename);
        if (type == null)
        {
            Debug.LogError("Not found file type : " + filename);
            return;
        }

        Debug.Log("INIT Make Binary File : " + filename);

        object obj = Activator.CreateInstance(type);
        System.Reflection.MethodInfo info = type.GetMethod("MakeBinaryFile", System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        info.Invoke(obj, null);

        Debug.Log("Make Binary File : " + filename);
    }

    public void MakeBinaryFile(System.Type type, BaseDataProperty[] list)
    {
        string path = string.Format("{0}{1}{2}{3}", GetResourceFolder(), BINARY_FILE_FOLDER, type.Name, BINARY_FILE_EXTENSION);

        StreamWriter sWriter = new StreamWriter(path);
        BinaryFormatter bin = new BinaryFormatter();

        bin.Serialize(sWriter.BaseStream, list);
        sWriter.Close();
    }

    public void ClearBinaryFile()
    {
        Debug.Log("Clear Binary File");

        string path = string.Format("{0}{1}", GetResourceFolder(), BINARY_FILE_FOLDER);
        string[] files = Directory.GetFiles(path);
        foreach (String file in files)
        {
            File.Delete(file);
        }
    }
}
