using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public static class MapDataMakerMenu
{
    [MenuItem("Tools/LoginScene")]
    static public void LoginScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Login.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
    }

    [MenuItem("Tools/LobbyScene")]
    static public void LobbyScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
    }

    [MenuItem("Tools/BattleScene")]
    static public void BattleScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/InGame2.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
    }

    [MenuItem("Tools/NewInGame")]
    static public void NewInGame()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/NewInGame.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
    }

    [MenuItem("Tools/testscene")]
    static public void testscene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/testscene.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
    }

    [MenuItem("Tools/ClearPlayerPrefabs")]
    static public void ClearPlayerPrefabs()
    {
        PlayerPrefs.DeleteAll();
    }

    /*    [MenuItem("TestXML/Test_XML_LOAD", false, 10000)]
        static public void TestXMLLoadFromTop()
        {
            TestXMLLoad();
        }

        [MenuItem("Assets/TestXML/Test_XML_LOAD", false)]
        static public void TestXMLLoadFromSelectItem()
        {
            TestXMLLoad();
        }

        private static void TestXMLLoad()
        {
            string name = Selection.activeObject.name;
            DataTableManager.Instance.LoadTable(name);
            Debug.Log("Test XML LOAD : " + name);
        }

        [MenuItem("TestXML/Test_XML_LOAD", true)]
        [MenuItem("Assets/TestXML/Test_XML_LOAD", true)]
        private static bool CheckTestXMLLoadFromSelectItem()
        {
            string assetname = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (assetname.Contains("/Room/") ||
                (assetname.Contains("/xml/") == false && assetname.Contains(".xml") == false))
            {
                return false;
            }

            return true;
        }


        [MenuItem("TestXML/Create_Sample_XML", false, 10001)]
        public static void CreateSampleXmlFromTop()
        {
            CreateSampleXml();
        }

        [MenuItem("Assets/TestXML/Create_Sample_XML", false)]
        public static void CreateSampleXmlFromSelectItem()
        {
            CreateSampleXml();
        }

        private static void CreateSampleXml()
        {
            string classname = Selection.activeObject.name;

            Debug.Log("CreateSampleXml : " + classname);
            DataTableManager.Instance.SaveSampleXML(classname);
        }

        [MenuItem("TestXML/Create_Sample_XML", true)]
        [MenuItem("Assets/TestXML/Create_Sample_XML", true)]
        private static bool CheckCreateSampleXmlFromSelectItem()
        {
            string assetname = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (assetname.Contains("/Tables/") == false)
            {
                return false;
            }

            return true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [MenuItem("TestXML/Create_Binary_File", false, 10002)]
        public static void CreateBinaryFile()
        {
            string path = Application.dataPath + "/Resources/XML/";
            string[] files = Directory.GetFiles(path, "*.xml");

            DataTableManager.Instance.ClearBinaryFile();

            foreach (string file in files)
            {
                if (file.Contains("Sample") == true)
                    continue;

                int lastindex = file.IndexOf(".xml");
                int firstindex = file.LastIndexOf("/") + 1;

                string name = file.Substring(firstindex, lastindex - firstindex);
                DataTableManager.Instance.MakeBinaryFile(name);
            }
        }
        */
}
