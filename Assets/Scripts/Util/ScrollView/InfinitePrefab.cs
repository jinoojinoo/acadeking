using UnityEngine;
using System.Collections;

public class InfinitePrefab : MonoBehaviour
{
    UIPanel m_Panel;
    UIGrid m_uiGrid;
    CustomInfiniteScrollView m_custom;
    bool m_InitDone = false;
    int m_Index = 0;
    private Vector3 Offset;

    private GameObject m_myObject = null;
    public GameObject MyObject
    {
        get
        {
            if (m_myObject == null)
                m_myObject = this.gameObject;
            return m_myObject;
        }
    }

    public int PoolType
    {
        get;
        set;
    }

    public void Set(CustomInfiniteScrollView custom, int Index , Vector3 offset)
    {
        m_Panel = custom.panel;
        m_uiGrid = custom.m_uiGrid;
        m_custom = custom;
        m_Index = Index;
        m_InitDone = true;
        Offset = offset;
    }

    public Vector3 GetCurrentBounds()
    {
        Bounds currentBound = NGUIMath.CalculateRelativeWidgetBounds(m_Panel.transform, transform);
        return currentBound.size;
    }

	// Update is called once per frame
	void Update ()
    {
        if (m_InitDone == false || IsRemove() == false)
            return;

        Bounds currentBound = NGUIMath.CalculateRelativeWidgetBounds(m_Panel.transform, transform);
        Vector3 size = GetCurrentBounds();
        size.x += Offset.x;
        size.y += Offset.y;
        currentBound.size = size;

        Vector3 offset = m_Panel.CalculateConstrainOffset(currentBound.min, currentBound.max);

        switch (m_uiGrid.arrangement)
        {
            case UIGrid.Arrangement.Vertical:
                if (Mathf.Abs(offset.x) <= currentBound.size.x)
                {
                }
                else
                {
                    m_custom.RemovePrefab(m_Index);
                }
                break;

            case UIGrid.Arrangement.Horizontal:
                if (Mathf.Abs(offset.y) <= currentBound.size.y)
                {
                }
                else
                {
                    m_custom.RemovePrefab(m_Index);
                }
                break;

            default:
                break;
        }
	}
    bool IsRemove()
    {
        if (/*m_Index == 0 || m_Index == m_custom.DataNum - 1 || */
            (m_Index >= m_custom.Min && m_Index <= m_custom.Max))
            return false;

        return true;
    }
}
