using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityEngineExtensions
{
    public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    public static bool IntersectionWithRayFromCenter(this BoxCollider2D boxcollider, Vector2 normal, Vector2 offset, out Vector2 intersection)
    {
        Vector2 boxsize = (boxcollider.size * 0.5f);
        boxsize += offset;

        Vector2 p1 = new Vector2(boxsize.x, boxsize.y) + boxcollider.offset;
        Vector2 p2 = new Vector2(boxsize.x, -boxsize.y) + boxcollider.offset;
        Vector2 p3 = new Vector2(-boxsize.x, -boxsize.y) + boxcollider.offset;
        Vector2 p4 = new Vector2(-boxsize.x, boxsize.y) + boxcollider.offset;
        Vector2 center = boxcollider.offset;

        if (LineSegmentsIntersection(center, center + normal * 10.0f, p1, p2, out intersection))
            return true;
        if (LineSegmentsIntersection(center, center + normal * 10.0f, p2, p3, out intersection))
            return true;
        if (LineSegmentsIntersection(center, center + normal * 10.0f, p3, p4, out intersection))
            return true;
        if (LineSegmentsIntersection(center, center + normal * 10.0f, p4, p1, out intersection))
            return true;

        return true;
    }
}

public static class GameUtil
{
    public const float BasketBall_SIZE = 24.0f;
    public const float BaseBall_SIZE = 7.3f;
    public const float BeachBall_SIZE = 21.0f;
    public const float DodgeBall_SIZE = 18.0f;
    public const float SoccerBall_SIZE = 22.0f;
    public const float TennisBall_SIZE = 6.54f;
    public const float Volleyball_SIZE = 20.7f;

    public static bool CheckMyGold(int price)
    {
        int mygold = AccountManager.Instance.MyAccountInfo.Gold;
        if (mygold < price)
        {
            PopupManager.Instance.ShowGoldErrorPopup();
            return false;
        }

        return true;
    }

    public static string GetColorLabel<T>(T src, T det, bool defalut = true, bool reverse = false)
    {
        var cur = Convert.ToDouble(src);
        var old = Convert.ToDouble(det);

        string color;
        if (defalut)
        {
            //ffd700 Yellow
            color = cur <= old ? "[ffffff]" : "[DC143C]";
        }
        else
        {
            color = cur <= old ? "[00de05]" : "[ff0005]";
        }

        string result = string.Empty;
        if (reverse)
            result = string.Format("{0}{1:#,0}[-] ", color, det); //���簪
        else
            result = string.Format("{0}{1:#,0}[-]", color, src); //�ʿ䰪
        return result;
    }

    public static Vector3 GetTargetPosition(int section = 0)
    {
        float x;
        float y;

        float width = (int)GlobalValue_Table.Instance.MAP_SIZE_X - (GlobalValue_Table.Instance.MOVE_OFFSET_LEFT + GlobalValue_Table.Instance.MOVE_OFFSET_RIGHT);
        float heigh = (int)GlobalValue_Table.Instance.MAP_SIZE_Y - (GlobalValue_Table.Instance.MOVE_OFFSET_TOP + GlobalValue_Table.Instance.MOVE_OFFSET_BOTTOM);

        float offset = 100.0f;

        float half_x = width * 0.5f;
        float half_y = heigh * 0.5f;

        if (section == 0)
        {
            half_x -= offset;
            half_y -= offset;

            x = UnityEngine.Random.Range(-half_x, half_x);
            y = UnityEngine.Random.Range(-half_y, half_y);

            return new Vector3(x, y, 0.0f);
        }
        else
        {
            int divide = section * 4;
            divide /= 2;

            int random_divide_x = UnityEngine.Random.Range(0, divide) + 1;
            int random_divide_y = UnityEngine.Random.Range(0, divide) + 1;

            float randomx = (width - offset * 2) / divide;
            float randomy = (heigh - offset * 2) / divide;

            x = UnityEngine.Random.Range(randomx * (random_divide_x - 1), randomx * random_divide_x);
            y = UnityEngine.Random.Range(randomy * (random_divide_y - 1), randomy * random_divide_y);

            return new Vector3(x - half_x, y - half_y, 0.0f);
        }
    }

    public static void DeepCopy(object src, object tar)
    {
        System.Type srctype = src.GetType();
        System.Type destype = tar.GetType();
        foreach (System.Reflection.FieldInfo finfo in srctype.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            System.Reflection.FieldInfo copyinfo = destype.GetField(finfo.Name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (copyinfo != null)
            {
                copyinfo.SetValue(tar, finfo.GetValue(src));
            }
        }
    }

    public static void CreateBoxCollider(Dictionary<string, SpriteRenderer> rendererlist, Transform trans, BoxCollider2D boxcol)
    {
        Vector3 Min = Vector3.zero;
        Vector3 Max = Vector3.zero;

        foreach (SpriteRenderer render in rendererlist.Values)
        {
            CreateBoxCollider(render, ref Min, ref Max);
        }

        boxcol.size = Max - Min;
        boxcol.offset = new Vector2(0, boxcol.size.y / 2);

        boxcol.transform.parent = trans;
        boxcol.transform.localPosition = Vector3.zero;
        boxcol.transform.localRotation = Quaternion.identity;

        SetGameObjectLayer(boxcol.gameObject, trans.gameObject.layer);
    }

    public static void CreateBoxCollider(SpriteRenderer render, ref Vector3 Min, ref Vector3 Max)
    {
        Vector3 center = Quaternion.Inverse(render.transform.localRotation) * render.bounds.center;
        Vector3 extent = Quaternion.Inverse(render.transform.localRotation) * render.bounds.extents;

        //extent�� ������ �ƴ� ����� ��ȯ�� �ؼ� Min Max�� ����� ���Ѵ�. 
        extent = new Vector3(Mathf.Abs(extent.x), Mathf.Abs(extent.y), Mathf.Abs(extent.z));

        Vector3 min = center - extent;
        Vector3 max = center + extent;

        Min = Vector3.Min(Min, min);
        Max = Vector3.Max(Max, max);
    }

    public static void SetGameObjectLayer(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform trans in target.transform)
        {
            trans.gameObject.layer = layer;
            SetGameObjectLayer(trans.gameObject, layer);
        }
    }

    public static void CaptureScreenshot()
    {
        string screenshotFolder = Application.dataPath + "/../ScreenShot";
        string szfile, sztime;

        // create directory
        System.IO.Directory.CreateDirectory(screenshotFolder);
        // make a file name
        System.DateTime regularDate = System.DateTimeOffset.Now.DateTime;

        sztime = string.Format("{0}{1:D02}{2:D02}_{3:D02}{4:D02}{5:D02}",
            regularDate.Year, regularDate.Month, regularDate.Day,
            regularDate.Hour, regularDate.Minute, regularDate.Second);
        szfile = string.Format("{0}/ScreenShot_{1}.png", screenshotFolder, sztime);
        ScreenCapture.CaptureScreenshot(szfile);

        Debug.Log(szfile + " screenshot saved");
    }

    public static void InitUIBallSize(GameObject obj, ShopType type, int layer)
    {
        Ball.InitBallSize(obj, type, true);
        obj.transform.localScale *= 100.0f;
        Quaternion qut = obj.transform.localRotation;
        qut.eulerAngles = new Vector3(0, 90, 0);
        obj.transform.localRotation = qut;

        SetGameObjectLayer(obj, layer);
    }
}

public static class TableHelper
{
    public class TableCommaValue<T>
    {
        private string m_tableStr = string.Empty;
        private string TablsStr
        {
            set
            {
                m_tableStr = value;
                m_valuelist = null;
            }
            get
            {
                return m_tableStr;
            }
        }

        private List<T> m_valuelist = null;
        public List<T> ValueList
        {
            get
            {
                if (string.IsNullOrEmpty(TablsStr))
                    return new List<T>();

                if (m_valuelist == null)
                {
                    Debug.LogWarning("TablsStr : " + TablsStr);
                    m_valuelist = TableHelper.CommaStringToList<T>(TablsStr);
                }
                return m_valuelist;
            }
        }

        public static implicit operator TableCommaValue<T>(string value)
        {
            return new TableCommaValue<T> { TablsStr = value };
        }

        public static implicit operator string(TableCommaValue<T> value)
        {
            return value.TablsStr;
        }

        public static implicit operator List<T>(TableCommaValue<T> value)
        {
            return value.ValueList;
        }
    }

    public static Vector3 CommaStringToVector3(string commaString)
    {
        if (string.IsNullOrEmpty(commaString))
            return new Vector3();

        Vector3 v3 = new Vector3();
        string[] arr = commaString.Split(',');

        if (arr.Length >= 3)
        {
            v3.x = (System.Single)System.Double.Parse(arr[0]);
            v3.y = (System.Single)System.Double.Parse(arr[1]);
            v3.z = (System.Single)System.Double.Parse(arr[2]);
        }
        else if (arr.Length == 2)
        {
            v3.x = (System.Single)System.Double.Parse(arr[0]);
            v3.y = (System.Single)System.Double.Parse(arr[1]);
            v3.z = 0;
        }
        else if (arr.Length == 1)
        {
            v3.x = (System.Single)System.Double.Parse(arr[0]);
            v3.y = 0;
            v3.z = 0;
        }

        return v3;
    }

    public static string Vector3ToCommaString(Vector3 vec)
    {
        return string.Format("{0},{1},{2}", vec[0], vec[1], vec[2]);
    }

    public static List<T> CommaStringToList<T>(string commaString)
    {
        if (string.IsNullOrEmpty(commaString))
            return new List<T>();

        List<T> listt = new List<T>();
        string[] arr = commaString.Split(',');

        for (int ai = 0; ai < arr.Length; ai++)
        {
            T val = (T)System.Convert.ChangeType(arr[ai], typeof(T));
            listt.Add(val);
        }

        return listt;
    }

    public static Vector3 ListToVector3(List<float> list, Vector3 defaultvalue)
    {
        if (list == null || list.Count < 3)
            return defaultvalue;

        return new Vector3(list[0], list[1], list[2]);
    }
}