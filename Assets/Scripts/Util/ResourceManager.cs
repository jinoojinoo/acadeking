using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;

public class ResourceManager : SingletonMono<ResourceManager>
{
    public Dictionary<string, Object> protect_resources = new Dictionary<string, Object>(20);
    #region Private Variable
    private Dictionary<string, Object> resources = new Dictionary<string, Object>(20);
    private class RscObjCategory
    {
        public RscObjCategory(int _cat, string strPath)
        {
            Category = _cat; resourcePath = strPath;
        }
        public int Category = 0;
        public string resourcePath;
    }
    private List<RscObjCategory> rscCategoryList = new List<RscObjCategory>(20);

    #endregion


    #region Load Resource

    private T LoadAsset<T>(string resourcePath) where T : Object
    {
        T rsc_new = null;

        string[] divides = resourcePath.Split('/');
        int total = divides.Length;

        if (total > 0)
            rsc_new = AssetBundleManager.Instance.GetAsset<T>(divides[total - 1]);
        else
            rsc_new = AssetBundleManager.Instance.GetAsset<T>(resourcePath);

        return rsc_new;
    }

    private T[] LoadSubAssets<T>(string resourcePath) where T : Object
    {
        T[] rsc_new = null;

        string[] divides = resourcePath.Split('/');
        int total = divides.Length;

        if (total > 0)
            rsc_new = AssetBundleManager.Instance.GetSubAssets<T>(divides[total - 1]);
        else
            rsc_new = AssetBundleManager.Instance.GetSubAssets<T>(resourcePath);

        return rsc_new;
    }

#if UNITY_EDITOR

    public Object LoadResource(string resourcePath)
    {
        Object obj = null;

        obj = Resources.Load(resourcePath);

        if (obj == null)
            obj = LoadAsset<Object>(resourcePath);

        return obj;
    }

#endif

    public T LoadResource<T>(string resourcePath) where T : Object
    {
        T rsc_new = null;

        rsc_new = Resources.Load<T>(resourcePath);

        if (rsc_new == null)
            rsc_new = LoadAsset<T>(resourcePath);

        return rsc_new;
    }

#if UNITY_EDITOR
    public static T LoadResourceFromPC<T>(string resourcePath) where T : Object
    {
        T rsc_new = Resources.Load<T>(resourcePath);
        if (rsc_new != null)
            return rsc_new;

        string[] divides = resourcePath.Split('/');
        int total = divides.Length;

        return AssetBundleManager.GetAssetInSimulateMode<T>(divides[total - 1]);
    }
#endif

    public T[] LoadSubResources<T>(string resourcePath) where T : Object
    {
        T[] rsc_new = null;

        rsc_new = Resources.LoadAll<T>(resourcePath);

        if (rsc_new == null || rsc_new.Length < 1)
            rsc_new = LoadSubAssets<T>(resourcePath);

        return rsc_new;
    }

    public T LoadResourceFromCache<T>(string resourcePath, int ResourceCategory = 0, bool isprotect = false) where T : Object
    {
        Object rsc = null;

        if (protect_resources.TryGetValue(resourcePath, out rsc))
        {
            return rsc as T;
        }

        if (resources.TryGetValue(resourcePath, out rsc))
        {
            if (isprotect && protect_resources.ContainsKey(resourcePath) == false)
            {
                protect_resources.Add(resourcePath, rsc);
            }

            return rsc as T;
        }

        Debug.LogError("load : " + resourcePath);
        T rsc_new = null;

        rsc_new = Resources.Load<T>(resourcePath);

        if (rsc_new == null)
            rsc_new = LoadAsset<T>(resourcePath);

        if (rsc_new != null)
        {
            resources.Add(resourcePath, rsc_new);
            if (isprotect)
                protect_resources.Add(resourcePath, rsc_new);

            if (ResourceCategory > 0)
            {
                rscCategoryList.Add(new RscObjCategory(ResourceCategory, resourcePath));
            }
        }

        return rsc_new;
    }

    public GameObject LoadResourceObject(string resourcePath, bool isprotect = false)
    {
        GameObject obj = LoadResourceFromCache<GameObject>(resourcePath, 0, isprotect);

        if (obj == null)
            return null;

#if UNITY_EDITOR
        //if (Debug.isDebugBuild)
        {
            MonoBehaviour[] monoBehaviours = obj.GetComponentsInChildren<MonoBehaviour>();
            int count = monoBehaviours.Length;

            for (int i = 0; i < count; ++i)
            {
                if (null == monoBehaviours[i])
                {
                    Debug.LogError("Have a missing script in gameobject. gameobject name is '" + obj.name + "'");

                }
            }
        }
#endif

        return GameObject.Instantiate(obj);
    }

    public SpriteRenderer InstantiateSpritePrefab(string resourcePath, int ResourceCategory = 0)
    {
        GameObject prefabSprite = ResourceManager.Instance.LoadResourceFromCache<GameObject>(resourcePath);
        if (prefabSprite)
        {
            GameObject spriteObj = GameObject.Instantiate<GameObject>(prefabSprite);
            if (spriteObj)
            {
                return spriteObj.GetComponent<SpriteRenderer>();
            }
        }
        return null;
    }

    #endregion


    #region Remove Resource

    private void RemoveCategoryData(string resourcePath)
    {
        for (int i = 0; i < rscCategoryList.Count; i++)
        {
            if (rscCategoryList[i].resourcePath.Equals(resourcePath))
            {
                rscCategoryList.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveFromCache(string resourcePath, bool isUnloadImmediate = false)
    {
        Object rsc = null;

        if (resources.TryGetValue(resourcePath, out rsc))
        {
            resources.Remove(resourcePath);
            RemoveCategoryData(resourcePath);
            if (isUnloadImmediate)
                Resources.UnloadAsset(rsc);
        }
    }

    public void RemoveCategoryFromCache(int ResourceCategory, bool isUnloadImmediate = false)
    {

        for (int i = rscCategoryList.Count - 1; i >= 0; i--)
        {
            if (rscCategoryList[i].Category == ResourceCategory)
            {
                rscCategoryList.RemoveAt(i);
                if (resources.ContainsKey(rscCategoryList[i].resourcePath))
                {
                    resources.Remove(rscCategoryList[i].resourcePath);
                }
            }
        }

        if (isUnloadImmediate)
            Resources.UnloadUnusedAssets();
    }

    #endregion


    #region Clear Resource

    public void ClearCache()
    {
        //        Spine.Unity.SkeletonDataAsset.PreLoadClear();
        resources.Clear();
        rscCategoryList.Clear();
        Resources.UnloadUnusedAssets();
    }

    private void OnDestroy()
    {
        protect_resources.Clear();
        ClearCache();
    }

    public GameObject GetGameObjectByTable(Transform parent, UIGAMEOBJECT_TYPE type, int addindex = -1, bool isprotect = false)
    {
        UIGameObject_DataProperty property = UIGameObject_Table.Instance.GetGameObjectProperty(type);

        string path = property.Path;
        if (addindex != -1)
        {
            path = string.Format(path, addindex);
        }

        GameObject obj = ResourceManager.Instance.LoadResourceObject(path, isprotect);
        if (obj == null)
            return null;

        obj.transform.parent = parent;
        obj.transform.localScale = property.Scale;
        obj.transform.localPosition = property.Position;
        if (string.IsNullOrEmpty(property.LAYER) == false)
            obj.transform.ChangeLayerReculsively(property.LAYER);

        return obj;
    }

    public void PreLoadGameObjectByTable(Transform parent, UIGAMEOBJECT_TYPE type, int addindex = -1, bool isprotect = false)
    {
        GameObject obj = GetGameObjectByTable(parent, type, addindex, isprotect);
        if (obj == null)
            return;

        Destroy(obj);
    }

    public void PreLoadGameObjectBySoundTable(UISOUND_ID id)
    {
        UISound_DataProperty property = UISound_Table.Instance.GetUISoundDataProperty(id);
        if (property == null)
            return;

        ResourceManager.Instance.LoadResourceFromCache<AudioClip>(property.Path);
    }

    //     public GameObject GetTargetObject(Transform parent, int kind)
    //     {
    //         string charactgerPath = string.Format("Prefabs/Character/CharaRef_{0:00}", kind);
    //         GameObject targetobject = ResourceManager.Instance.LoadResourceObject(charactgerPath);
    // 
    //         targetobject.transform.SetParent(parent);
    //         targetobject.transform.localScale = new Vector3(100.0f, 100.0f, 1.0f);
    //         targetobject.transform.localPosition = Vector3.zero;
    // 
    //         return targetobject;
    //     }
    #endregion
}