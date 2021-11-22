using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryScrollView : CustomInfiniteScrollViewMono<UI_InventoryItem>
{
    private const int MAX_VIEW_ITEMCOUNT = 4;
    private const int MAX_INVENTORY_COUNT = 10;
    private int m_maxCount = 0;
    protected override void InitData()
    {
        m_maxCount = MAX_INVENTORY_COUNT;
        ResetForAddSlot();
        ResetPosition();
    }

    private void ResetForAddSlot()
    {
        int count = AccountManager.Instance.MyBallList.m_ballList.Count + 1;
        if (count > MAX_INVENTORY_COUNT)
            count = MAX_INVENTORY_COUNT;

        ScrollView.DataNum = count;
        ScrollView.Init();

        m_UIScrollView.enabled = MAX_VIEW_ITEMCOUNT > ScrollView.DataNum ? false : true;
    }

    protected override UI_InventoryItem InitObject(GameObject obj, int dataPos)
    {
        UI_InventoryItem item = base.InitObject(obj, dataPos);
        if (item == null)
        {
            obj.SetActive(false);
            return null;
        }

        if (dataPos >= m_maxCount)
        {
            obj.SetActive(false);
            return null;
        }

        item.Init(m_selectIndex == dataPos);
        return item;
    }

    private BallInfos m_currentBallInfos = null;
    private BallInfos CurrentBallInfos
    {
        get
        {
            return m_currentBallInfos;
        }
    }
    private int m_selectIndex = 0;
    private int SelectIndex
    {
        set
        {
            UI_InventoryItem olditem = GetItem(m_selectIndex);
            if (olditem != null)
                olditem.OnSelectItem(false);

            m_selectIndex = value;
            UI_InventoryItem selectitem = GetItem(m_selectIndex);
            if (selectitem != null)
                selectitem.OnSelectItem(true);

            m_currentBallInfos = AccountManager.Instance.GetMyBallInfo(selectitem.DataPos);
        }
    }

    private void OnClickItemSelect(int datapos)
    {
        SelectIndex = datapos;
    }

    private int m_needCost = 0;
    private void OnClickItem_Point(int datapos)
    {
        BallInfos infos = AccountManager.Instance.GetMyBallInfo(datapos);
        if (infos == null)
            return;

        OnClickItemSelect(datapos);
        PopupBase popupbase = PopupManager.Instance.ShowPopup(POPUP_TYPE.Ball_Point, OK_Point, null);

        m_needCost = GlobalValue_Table.Instance.Cost_Point * (infos.Score - 1);

        popupbase.MsgLabel.text = string.Format(popupbase.MsgLabel.text,
            infos.Score,
            infos.Score + 1,
            GameUtil.GetColorLabel(m_needCost, AccountManager.Instance.MyAccountInfo.Gold));
    }

    private void OK_Point(params object[] parameters)
    {
        if (GameUtil.CheckMyGold(m_needCost) == false)
            return;

        AccountManager.Instance.MyAccountInfo.Gold -= m_needCost;
        AccountManager.Instance.UpdateBallInfo(CurrentBallInfos.Index, CurrentBallInfos.BallType, CurrentBallInfos.Score + 1);
        ResetData();
    }

    private int m_addslotIndex = 0;
    private void OnClickItem_AddSlot(int datapos)
    {
        BallInfos infos = AccountManager.Instance.GetMyBallInfo(datapos);
        if (infos != null)
            return;

        m_addslotIndex = datapos;
        m_needCost = GlobalValue_Table.Instance.Cost_AddSlot;
        PopupBase popupbase = PopupManager.Instance.ShowPopup(POPUP_TYPE.Ball_AddSlot, OK_AddSlot, null);
        popupbase.MsgLabel.text = string.Format(StringTable.ADDSlot, GameUtil.GetColorLabel(m_needCost, AccountManager.Instance.MyAccountInfo.Gold));
    }

    private void OK_AddSlot(params object[] parameters)
    {
        if (GameUtil.CheckMyGold(m_needCost) == false)
            return;

        AccountManager.Instance.MyAccountInfo.Gold -= m_needCost;
        AccountManager.Instance.UpdateBallInfo(m_addslotIndex);

        ResetForAddSlot();
    }
}
