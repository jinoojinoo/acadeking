using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiniteObjectInfo : MonoBehaviour
{
    protected GameObject m_messageeventObject;
    public GameObject MessageEventObject
    {
        protected get
        {
            return m_messageeventObject;
        }
        set
        {
            m_messageeventObject = value;
        }
    }

    public UIPanel ParentPanel
    {
        get;
        set;
    }

    public UIScrollView ScrollView
    {
        get;
        set;
    }

    public CustomInfiniteScrollView CustomScrollView
    {
        get;
        set;
    }

    public int DataPos
    {
        get;
        set;
    }
}

public abstract class CustomInfiniteScrollViewMono<T> : MonoBehaviour where T : InfiniteObjectInfo
{
    public GameObject m_MessageTargetObject
    {
        get;
        set;
    }

    public GameObject m_ScrollViewObject;
    protected CustomInfiniteScrollView ScrollView
    {
        get;
        set;
    }
    protected UIScrollView m_UIScrollView
    {
        get;
        set;
    }

    private UIPanel m_myPanel = null;
    protected UIPanel MyPanel
    {
        get
        {
            if (m_myPanel == null)
                m_myPanel = this.GetComponent<UIPanel>();

            return m_myPanel;
        }
    }
    private Vector3 m_oriPosition;

    public void Awake()
    {
        AwakeInfinite();
    }

    public void Initialize()
    {
        InitInfinite();
        InitDataInfinite();
    }

    public void InitDataInfinite()
    {
        InitData();
    }

    public void AwakeInfinite()
    {
        m_oriPosition = this.transform.localPosition;
        ScrollView = m_ScrollViewObject.GetComponent<CustomInfiniteScrollView>();
        ScrollView.SetCall = InitObjectFunc;
        m_UIScrollView = m_ScrollViewObject.GetComponent<UIScrollView>();
        m_UIScrollView.onDragFinished = onDragFinished;
    }

    private void onDragFinished()
    {
        Bounds b = m_UIScrollView.bounds;
        Vector3 constraint = MyPanel.CalculateConstrainOffset(b.min, b.max);

        if (m_UIScrollView.movement == UIScrollView.Movement.Horizontal)
        {
            constraint.y = 0f;
            if (constraint.sqrMagnitude > 0.1f)
            {
                if (constraint.x < 0)
                    OnListReached_Top();
                else
                    OnListReached_Bottom();
            }
        }

        else if (m_UIScrollView.movement == UIScrollView.Movement.Vertical)
        {
            constraint.x = 0f;
            if (constraint.sqrMagnitude > 0.1f)
            {
                if (constraint.y > 0)
                    OnListReached_Top();
                else
                    OnListReached_Bottom();
            }
        }
    }

    protected virtual void OnListReached_Top()
    {

    }

    protected virtual void OnListReached_Bottom()
    {

    }

    protected void ResetPosition()
    {
        this.transform.localPosition = m_oriPosition;
        MyPanel.clipOffset = Vector2.zero;
        ScrollView.scroll.DisableSpring();
    }

    private void InitInfinite()
    {
        if (ScrollView == null)
            return;
		
        ScrollView.AllRemovePrefab();
    }

    public virtual void ResetData() // 데이터만 다시 셋팅
    {
        foreach (KeyValuePair<int, InfinitePrefab> child in ScrollView.ChildList)
        {
            InitObject(child.Value.gameObject, child.Key);
        }
    }

    public virtual void ResetData(int index) // 데이터만 다시 셋팅
    {
        foreach (KeyValuePair<int, InfinitePrefab> child in ScrollView.ChildList)
        {
            if (child.Key == index)
            {
                InitObject(child.Value.gameObject, child.Key);
                return;
            }
        }
    }

    public virtual void SetPosiitonByIndex(uint index)
    {
        ScrollView.SetPosiitonByIndex(index);
    }

    public virtual void ResetInitialize() // 처음부터 다시생성
    {
        ScrollView.scroll.ResetPosition();
        InitInfinite();
    }

    protected virtual void InitObjectFunc(GameObject obj, int datapos)
    {
        T child = obj.GetComponent<T>();
        if(child != null)
            child.MessageEventObject = m_MessageTargetObject == null ? this.gameObject : m_MessageTargetObject;

        child.ParentPanel = MyPanel;
        child.ScrollView = m_UIScrollView;
        child.CustomScrollView = ScrollView;

        InitObject(obj, datapos);
    }

    protected virtual T InitObject(GameObject obj, int datapos)
    {
        T info = obj.GetComponent<T>();
        info.DataPos = datapos;
        return info;
    }

    protected abstract void InitData();

    public virtual T GetItem(int datapos) 
    {
        if (ScrollView.ChildList.ContainsKey(datapos) == false)
            return null;

        return ScrollView.ChildList[datapos].GetComponent<T>();
    }

    public T GetMinItem()
    {
        return GetItem(ScrollView.Min);
    }
}