using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Shop_Tab : MonoBehaviour
{
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

    private UILabel m_myLabel;
    private UILabel MyLabel
    {
        get
        {
            if (m_myLabel == null)
                m_myLabel = GetComponentInChildren<UILabel>();
            return m_myLabel;
        }
    }
    private ShopType m_myTab;
    public ShopType MyTab
    {
        set
        {
            m_myTab = value;
            MyLabel.text = GetTabName(value);
        }
    }

    private string GetTabName(ShopType type)
    {
        switch (type)
        {
            case ShopType.BaseBall:
                return StringTable.Tab_BaseBall;

            case ShopType.BasketBall:
                return StringTable.Tab_BasketBall;

            case ShopType.BeachBall:
                return StringTable.Tab_BeachBall;

            case ShopType.DodgeBall:
                return StringTable.Tab_DodgeBall;

            case ShopType.SoccerBall:
                return StringTable.Tab_SoccerBall;

            case ShopType.TennisBall:
                return StringTable.Tab_TennisBall;

            case ShopType.Volleyball:
                return StringTable.Tab_Volleyball;

            case ShopType.FootBall:
                return StringTable.Tab_FootBall;

            case ShopType.GOLD:
                return StringTable.Tab_GOLD;

            case ShopType.DisableAD:
                return StringTable.DisableAD;

            default:
                return null;
        }
    }

    private void OnClick()
    {
        SendMessageUpwards("OnClick_Tab", m_myTab);
    }
}
