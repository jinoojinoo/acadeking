using UnityEngine;
using System.Collections;

public class HUDScale : MonoBehaviour
{
    // 제작시 해상도
    public const float Width = 1080.0f;
    public const float Height = 1920.0f;

    public const float AspectRatio = Width / Height;

    public Camera mUiCamera;
    public Camera mMainCamera;
    public Camera UiCamera
    {
        get
        {
            return mUiCamera;
        }
    }

    public bool isFixScale = false;


    static public float aspectRatio
    {
        get { return mCurrentAspectRatio / AspectRatio; }
    }

    public static float mCurrentAspectRatio = 0.0f;

    private void ModifyUIScale()
    {
        float newAspectRatio = mUiCamera.aspect;

        if (mCurrentAspectRatio == newAspectRatio)
        {
            return;
        }

        mCurrentAspectRatio = newAspectRatio;
        Vector3 newScale = transform.localScale;

        float size = 2f / Height;
        Vector3 ls = transform.localScale;

        if (!Mathf.Approximately(ls.x, size) ||
            !Mathf.Approximately(ls.y, size) ||
            !Mathf.Approximately(ls.z, size))
        {
            newScale = new Vector3(size, size, size);
        }

        if (isFixScale)
        {
            newScale.x = newScale.x * (mCurrentAspectRatio / AspectRatio);

        }
    }

    void Rearrange()
    {
        BroadcastMessage("RearrageWidget", mCurrentAspectRatio / AspectRatio, SendMessageOptions.DontRequireReceiver);
    }

    void Awake()
    {
        mCurrentAspectRatio = 0.0f;
        ModifyCamera();
    }

    void ModifyCamera()
    {
        if (isFixScale == false)
            return;
        SetModifyPanel(this.transform);
        //        Debug.LogError("widht : " + Width + ", height : " + Height);
        //        Debug.LogError("screen widht : " + Screen.width + ", screen height : " + Screen.height);

    }

    static public void SetModifyPanel(Transform trans)
    {
        if (trans.childCount <= 0)
            return;

        for (int i = 0; i < trans.childCount; ++i)
        {
            Camera[] cams = trans.GetChild(i).GetComponentsInChildren<Camera>();

            if (cams == null)
                continue;

            float Ratio = 0.0f;
            foreach (Camera cam in cams)
            {
                SetHUDModifyCamera(cam);
            }
            mCurrentAspectRatio = Ratio;
        }
    }
    static public void SetModifyCamera(Transform trans)
    {
        Camera[] cams = trans.GetComponentsInChildren<Camera>();

        if (cams == null)
            return;

        foreach (Camera cam in cams)
        {
            SetHUDModifyCamera(cam);
        }
    }

    static public void SetHUDModifyCamera(Camera cam)
    {
// #if UNITY_EDITOR
//         Vector2 size = UIPanel.GetMainGameViewSize();
// #else
// 		Vector2 size = new Vector2(Screen.width, Screen.height);
// #endif
        Vector2 size = new Vector2(Screen.width, Screen.height);

        UIRoot rt = NGUITools.FindInParents<UIRoot>(cam.transform);

        if (rt == null)
        {
            if (UIRoot.list.Count <= 0)
                return;

            rt = UIRoot.list[0];
        }
        

#if UNITY_EDITOR
		if (rt != null) size *= rt.GetPixelSizeAdjustment(Mathf.RoundToInt(size.y));
#else
		if (rt != null) size *= rt.GetPixelSizeAdjustment(Screen.height);
#endif

        float perx = Width / size.x;
        float pery = Height / size.y;
        float v = (perx > pery) ? perx : pery;

        if (cam.orthographic)
            cam.orthographicSize = Mathf.Floor(v * 100.0f) / 100.0f;
        else
            cam.fieldOfView *= v;

        Debug.LogError("v : " + v);
    }
}