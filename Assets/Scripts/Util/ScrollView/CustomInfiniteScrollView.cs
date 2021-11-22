using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomInfiniteScrollView : MonoBehaviour
{
    Dictionary<int, InfinitePrefab> m_ChildList = new Dictionary<int, InfinitePrefab>();
    public Dictionary<int, InfinitePrefab> ChildList
    {
        get
        {
            return m_ChildList;
        }
    }

    public GameObject m_ChildPrefab;
    public UIGrid m_uiGrid;

#if USE_COVERFLOW
    private bool CoverFlowUse = false;
    private int CoverFlowOffset = 0;
    private float CoverFlowMinScale = 0.3f;
#endif

    int m_MaxData;

    public int DataNum
    {
        get
        {
            return m_MaxData;
        }
        set
        {
            m_MaxData = value;
        }
    }

    protected virtual int GetMaxData()
    {
        return m_MaxData;
    }

    UIPanel m_Panel;
    public UIPanel panel
    {
        get
        {
            return m_Panel;
        }
    }
    UIScrollView m_ScrollView;
    public UIScrollView scroll
    {
        get
        {
            return m_ScrollView;
        }
    }
    protected Vector2 m_PanelView;

    protected Vector3 m_PrefabSize;

    public Vector3 PrefabSize
    {
        get
        {
            return m_PrefabSize;
        }

        set
        {
            m_PrefabSize = value;
        }
    }

    private Vector3 m_AnimOffsetSize = Vector3.zero;
    private int m_AnimIndex = -1;

    protected Vector3 m_PanelCurrentPos = Vector3.zero;

    int m_Min = 0;
    int m_Max = 0;
    public int Min
    {
        get
        {
            return m_Min;
        }
    }
    public int Max
    {
        get
        {
            return m_Max;
        }
    }

    public delegate void SettingCall(GameObject item, int SetNum);

    SettingCall m_SetCall;

    public SettingCall SetCall
    {
        get { return m_SetCall; }
        set { m_SetCall = value; }
    }

    public Vector3 m_ChildOffset;

    bool m_IsInit = false;

    int m_ViewCount;

    private Dictionary<int, Stack<GameObject>> m_itempoolList = new Dictionary<int, Stack<GameObject>>();
    private Stack<GameObject> GetItemPoolList(int type)
    {
        if (m_itempoolList.ContainsKey(type) == false)
        {
            Stack<GameObject> stack = new Stack<GameObject>();
            m_itempoolList.Add(type, stack);
        }

        return m_itempoolList[type];
    }

    protected virtual void Awake()
    {
        m_Panel = gameObject.GetComponent<UIPanel>();
        m_ScrollView = gameObject.GetComponent<UIScrollView>();

        m_PrefabSize = NGUIMath.CalculateRelativeWidgetBounds(m_ChildPrefab.transform, true).size;
        if (m_uiGrid.cellWidth != 0)
            m_PrefabSize.x = m_uiGrid.cellWidth;
        if (m_uiGrid.cellHeight != 0)
            m_PrefabSize.y = m_uiGrid.cellHeight;

        Vector3 size = m_PrefabSize;
        size.x += m_ChildOffset.x;
        size.y += m_ChildOffset.y;
        m_PrefabSize = size;

        m_uiGrid.cellWidth = size.x;
        m_uiGrid.cellHeight = size.y;
        m_uiGrid.Reposition();

        if (m_uiGrid.keepWithinPanel && m_Panel != null)
        {
            Bounds bound = new Bounds(new Vector3(m_Panel.baseClipRegion.x, m_Panel.baseClipRegion.y), new Vector3(m_Panel.baseClipRegion.z, m_Panel.baseClipRegion.w));
            m_Panel.ConstrainTargetToBounds(m_uiGrid.transform, ref bound, true);
            m_PanelCurrentPos = m_Panel.transform.localPosition;
        }
        else
        {
            m_PanelCurrentPos = m_Panel.transform.localPosition;
        }
    }

    public void OnDestroy()
    {
        AllRecycleItemPool();
        ClearItemPool();
    }

    private bool m_fullView = false;
    public bool FullView
    {
        set
        {
            m_fullView = value;
        }
    }

    public virtual void Init()
    {
        m_IsInit = false;
        m_PanelView = m_Panel.GetViewSize();

        m_ViewCount = GetMaxViewCount();
        if (m_ViewCount > DataNum)
            m_ViewCount = DataNum;

        if (m_fullView)
            m_ViewCount = DataNum;

        CreateItemPool(m_ViewCount);
        m_IsInit = true;
    }

    public int GetMaxViewCount()
    {
        int viewcount = 0;
        if (m_ScrollView.movement == UIScrollView.Movement.Horizontal)
        {
            viewcount = Mathf.CeilToInt(m_PanelView.x / m_PrefabSize.x);
        }
        else if (m_ScrollView.movement == UIScrollView.Movement.Vertical)
        {
            viewcount = Mathf.CeilToInt(m_PanelView.y / m_PrefabSize.y);
        }

        viewcount += 1;
        if (m_uiGrid.maxPerLine != 0)
            viewcount = viewcount == 0 ? m_uiGrid.maxPerLine : viewcount * m_uiGrid.maxPerLine;

        return viewcount;
    }

    public void ResetGrid()
    {
        m_PanelView = m_Panel.GetViewSize();

        if (m_ScrollView.movement == UIScrollView.Movement.Horizontal)
            m_uiGrid.maxPerLine = (int)(m_PanelView.x / m_PrefabSize.x);
        else
            m_uiGrid.maxPerLine = (int)(m_PanelView.y / m_PrefabSize.y);

        Vector3 local = m_uiGrid.transform.localPosition;
        local.x = -(m_PanelView.x * 0.5f) + (m_PrefabSize.x * 0.5f);
        local.y = (m_PanelView.y * 0.5f) - (m_PrefabSize.y * 0.5f);
        m_uiGrid.transform.localPosition = local;

        m_uiGrid.cellWidth = m_PrefabSize.x;
        m_uiGrid.cellHeight = m_PrefabSize.y;

        AllRecycleItemPool();
    }

    protected void DefaultInit()
    {
        m_IsInit = true;
        m_PanelView = m_Panel.GetViewSize();
    }

    private GameObject GetChildObject(int kind)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(GetChildPrefab(kind));
        UIDragScrollView drag = obj.GetComponent<UIDragScrollView>();
        if (drag == null)
            drag = obj.AddComponent<UIDragScrollView>();
        drag.scrollView = m_ScrollView;

        return obj;
    }

    protected virtual GameObject GetChildPrefab(int kind)
    {
        return m_ChildPrefab;
    }

    private void CreateItemPool(int count, int type = 0)
    {
        AllRecycleItemPool();
        for (int i = GetItemPoolList(type).Count; i < count; ++i)
        {
            GameObject obj = GetChildObject(type);
            obj.transform.parent = m_uiGrid.transform;
            obj.SetActive(false);
            GetItemPoolList(type).Push(obj);
        }
    }

    protected virtual int GetItemPoolType(int index)
    {
        return 0;
    }

    private GameObject GetItemPool(int type)
    {
        GameObject obj = null;
        if (GetItemPoolList(type).Count > 0)
        {
            obj = GetItemPoolList(type).Pop();
        }
        else
        {
            obj = GetChildObject(type);
        }

        obj.SetActive(false);
        return obj;
    }

    private void RecycleItemPool(int type, GameObject obj)
    {
        obj.SetActive(false);
        GetItemPoolList(type).Push(obj);
    }

    protected void ClearItemPool()
    {
        foreach (int key in m_itempoolList.Keys)
        {
            while (m_itempoolList[key].Count > 0)
            {
                GameObject obj = m_itempoolList[key].Pop();
                if (obj == null)
                    continue;
                GameObject.Destroy(obj);
            }
        }
    }

    private void GetPositionIndex(ref int startpos, ref int endpos)
    {
        int startindex = int.MaxValue;
        int endindex = int.MinValue;
        foreach (int index in m_ChildList.Keys)
        {
            if (startindex > index)
                startindex = index;
            if (endindex < index)
                endindex = index;
        }

        startpos = startindex;
        endpos = endindex;
    }

    private bool m_repositonPrefab = false;
    public bool RepositionPrefab
    {
        set
        {
            m_repositonPrefab = value;
        }
        get
        {
            return m_repositonPrefab;
        }
    }

    public GameObject AddPrefab(int Index)
    {
        int maxdata = GetMaxData();
        if (0 > Index || maxdata <= Index)
            return null;
        int pooltype = GetItemPoolType(Index);
        if (m_ChildList.ContainsKey(Index))
        {
            if (RepositionPrefab)
                m_ChildList[Index].transform.localPosition = GetPos(Index);

            return m_ChildList[Index].gameObject;
        }

        GameObject clone = GetItemPool(pooltype);

        Transform clonetrnas = clone.transform;
        clonetrnas.parent = m_uiGrid.transform;
        clonetrnas.localPosition = GetPos(Index);
        clonetrnas.name = string.Format("{0:D2}_{1:D4}", pooltype, Index);
        clonetrnas.localScale = Vector3.one;
        clone.SetActive(true);
        clonetrnas.ChangeLayerReculsively(m_uiGrid.gameObject.layer);

        InfinitePrefab infiniteprefab = clone.GetComponent<InfinitePrefab>();
        if (infiniteprefab == null)
            infiniteprefab = clone.AddComponent<InfinitePrefab>();
        infiniteprefab.Set(this, Index, m_ChildOffset);
        infiniteprefab.PoolType = pooltype;

        //        InitCoverFlow(clone, Index);

        if (m_SetCall != null)
            SetCall(clone, Index);

        m_ChildList.Add(Index, infiniteprefab);

        AddPrefab(0);
        AddPrefab(maxdata - 1);

        return clone;
    }

#if USE_COVERFLOW
    private void InitCoverFlow(GameObject obj, int index)
    {
        if (CoverFlowUse == false)
            return;

        CorverFlowUnit unit = obj.GetComponent<CorverFlowUnit>();
        if (unit == null)
            unit = obj.AddComponent<CorverFlowUnit>();

        unit.MinScale = CoverFlowMinScale;
        unit.CellSize = CoverFlowOffset == 0.0f ? m_uiGrid.cellHeight : CoverFlowOffset;
        unit.UseOffset = CoverFlowOffset == 0.0f ? false : true;
        unit.CurrentIndex = index;
    }
#endif

    public void RemovePrefab(int Index)
    {
        if (m_Min <= Index && Index <= m_Max)
            return;

        if (Index == GetMaxData() - 1 || Index == 0)
            return;

        if (m_ChildList.ContainsKey(Index) == false)
            return;

        //clear pool
        RecycleItemPool(GetItemPoolType(Index), m_ChildList[Index].gameObject);
        m_ChildList.Remove(Index);
    }

    public void AllRemovePrefab()
    {
        if (m_ChildList.Count == 0)
            return;

        ClearItemPool();
        m_ChildList.Clear();
    }

    public void AllRecycleItemPool()
    {
        foreach (int childkey in ChildList.Keys)
        {
            RecycleItemPool(ChildList[childkey].PoolType, m_ChildList[childkey].gameObject);
        }
        m_ChildList.Clear();
    }

    protected virtual Vector3 GetPos(int Index)
    {
        return GetPos(Index, m_PrefabSize);
    }

    private Vector3 GetPos(int Index, Vector3 size)
    {
        Vector3 pos = Vector3.zero;
        int XIndex = 0;
        int YIndex = 0;

        switch (m_uiGrid.arrangement)
        {
            case UIGrid.Arrangement.Horizontal:
                if (m_uiGrid.maxPerLine != 0)
                {
                    XIndex = Index % m_uiGrid.maxPerLine;
                    YIndex = Index / m_uiGrid.maxPerLine;
                }
                else
                    XIndex = Index;

                pos = new Vector3(((m_uiGrid.cellWidth * XIndex)), -(m_uiGrid.cellHeight * YIndex), 0);
#if USE_COVERFLOW
                pos.x += CoverFlowUse && Index != 0 ? CoverFlowOffset / 2 : 0;
#endif
                break;

            case UIGrid.Arrangement.Vertical:
                if (m_uiGrid.maxPerLine != 0)
                {
                    XIndex = Index / m_uiGrid.maxPerLine;
                    YIndex = Index % m_uiGrid.maxPerLine;
                }
                else
                    YIndex = Index;

                pos = new Vector3((m_uiGrid.cellWidth * XIndex), -((m_uiGrid.cellHeight * YIndex)), 0);
                break;
        }

        if (Index > m_AnimIndex)
            pos -= m_AnimOffsetSize;

        return pos;
    }

    void Update()
    {
        if (m_IsInit == false)
            return;

        AddListItem();
    }

    void AddListItem()
    {
        m_ScrollView.panel.CalculateConstrainOffset(m_ScrollView.bounds.min, m_ScrollView.bounds.max);
        if (GetListMinMax(m_uiGrid.arrangement, ref m_Min, ref m_Max) == false)
            return;

        for (int i = m_Min; i < m_Max; ++i)
        {
            AddPrefab(i);
        }
    }

    public void RePosition()
    {
        m_ScrollView.panel.CalculateConstrainOffset(m_ScrollView.bounds.min, m_ScrollView.bounds.max);
        if (GetListMinMax(m_uiGrid.arrangement, ref m_Min, ref m_Max) == false)
            return;

        for (int index = m_Min; index < m_Max; ++index)
        {
            InfinitePrefab obj = m_ChildList[index];
            obj.gameObject.transform.localPosition = GetPos(index);
        }
    }

    protected virtual bool GetListMinMax(UIGrid.Arrangement arrangement, ref int min, ref int max)
    {
        switch (arrangement)
        {
            case UIGrid.Arrangement.Vertical:
                float width = m_PanelCurrentPos.x - m_Panel.transform.localPosition.x;
                int horizentalPos = (int)(width / m_PrefabSize.x);

                if (horizentalPos <= 0)
                    horizentalPos = 0;

                if (m_uiGrid.maxPerLine != 0)
                {
                    m_Min = horizentalPos * m_uiGrid.maxPerLine;
                    m_Max = m_Min + m_ViewCount;
                }
                else
                {
                    m_Min = horizentalPos;
                    m_Max = horizentalPos + m_ViewCount;
                }
                break;

            case UIGrid.Arrangement.Horizontal:
                float height = m_Panel.transform.localPosition.y - m_PanelCurrentPos.y;
                int verticalPos = (int)(height / m_PrefabSize.y);

                if (verticalPos < 0)
                    verticalPos = 0;

                if (m_uiGrid.maxPerLine != 0)
                {
                    m_Min = verticalPos * m_uiGrid.maxPerLine;
                    m_Max = m_Min + m_ViewCount;
                }
                else
                {
                    m_Min = verticalPos;
                    m_Max = verticalPos + m_ViewCount;
                }
                break;

            default:
                return false;
        }

        return true;
    }

    public void SetPosiitonByIndex(uint index)
    {
        float movex = 0.0f;
        float movey = 0.0f;

        SpringPanel springpanel = GetComponent<SpringPanel>();
        if (springpanel != null && springpanel.enabled)
            springpanel.enabled = false;

        switch (m_uiGrid.arrangement)
        {
            case UIGrid.Arrangement.Horizontal:
                {
                    float posx = m_PrefabSize.x * index;
                    movex = m_PanelCurrentPos.x - posx - m_Panel.transform.localPosition.x;
                    Vector3 offset = CalculateConstrainOffset(m_Panel, m_Panel.finalClipRegion + new Vector4(-movex, 0.0f), m_ScrollView.bounds.min, m_ScrollView.bounds.max);
                    if (offset.sqrMagnitude > 0f)
                        movex += offset.x;
                }
                break;

            case UIGrid.Arrangement.Vertical:
                {
                    float posy = m_PrefabSize.y * index;
                    movey = m_PanelCurrentPos.y + posy - m_Panel.transform.localPosition.y;
                    Vector3 offset = CalculateConstrainOffset(m_Panel, m_Panel.finalClipRegion + new Vector4(0.0f, -movey), m_ScrollView.bounds.min, m_ScrollView.bounds.max);
                    if (offset.sqrMagnitude > 0f)
                        movey += offset.y;

                }
                break;

            default:
                break;
        }

        scroll.MoveRelative(new Vector3(movex, movey, 0.0f));
    }

    public virtual Vector3 CalculateConstrainOffset(UIPanel panel, Vector4 finalClipRegion, Vector2 min, Vector2 max)
    {
        Vector4 cr = finalClipRegion;

        float offsetX = cr.z * 0.5f;
        float offsetY = cr.w * 0.5f;

        Vector2 minRect = new Vector2(min.x, min.y);
        Vector2 maxRect = new Vector2(max.x, max.y);
        Vector2 minArea = new Vector2(cr.x - offsetX, cr.y - offsetY);
        Vector2 maxArea = new Vector2(cr.x + offsetX, cr.y + offsetY);

        if (panel.softBorderPadding && panel.clipping == UIDrawCall.Clipping.SoftClip)
        {
            minArea.x += panel.clipSoftness.x;
            minArea.y += panel.clipSoftness.y;
            maxArea.x -= panel.clipSoftness.x;
            maxArea.y -= panel.clipSoftness.y;
        }

        return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
    }

}
