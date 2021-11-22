using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryItem : InfiniteObjectInfo
{
    public GameObject SelectSprite;
    public Transform TargetTrans;

    public GameObject InfosObject;
    public GameObject AddSlotObject;

    public UILabel BallNameLabel;
    public UILabel PointLabel;

    public UILabel SlotNumberLabel;

    private GameObject m_itemObject = null;
    private GameObject ItemObject
    {
        set
        {
            if (m_itemObject != null)
            {
                Destroy(m_itemObject);
                m_itemObject = null;
            }
            m_itemObject = value;
        }
        get
        {
            return m_itemObject;
        }
    }

    private BallInfos m_myBallInfos = null;
    public void Init(bool isselect)
    {
        OnSelectItem(isselect);

        SlotNumberLabel.text = string.Format("{0:00}", DataPos + 1);

        m_myBallInfos = AccountManager.Instance.GetMyBallInfo(DataPos);
        InfosObject.SetActive(m_myBallInfos != null ? true : false);
        AddSlotObject.SetActive(m_myBallInfos == null ? true : false);
        CreateObject(m_myBallInfos);

        if (m_myBallInfos == null)
            return;

        BallType_DataProperty property = BallType_Table.Instance.GetBallTypeProperty(m_myBallInfos.BallType);
        BallNameLabel.text = property.BallName;
        PointLabel.text = string.Format("{0} Point", m_myBallInfos.Score);
    }

    private void CreateObject(BallInfos infos)
    {
        if (infos == null)
        {
            ItemObject = null;
            return;
        }

        Transform trans = TargetTrans;
        BallType_DataProperty property = BallType_Table.Instance.GetBallTypeProperty(infos.BallType);
        ItemObject = ResourceManager.Instance.LoadResourceObject(property.Path);
        ItemObject.transform.parent = trans;
        GameUtil.InitUIBallSize(ItemObject, property.Shop_Type, this.gameObject.layer);
        ItemObject.transform.localPosition = Vector3.zero;
    }

    public void OnClick_Point()
    {
        this.MessageEventObject.SendMessage("OnClickItem_Point", DataPos);
    }

    public void OnClick_Skin()
    {
        AccountManager.Instance.CurrentSelectBallInfo = m_myBallInfos;
        GameUIManager.Instance.PushSequence(GAME_UI_MODE.UI_Shop);
    }

    public void OnSelectItem(bool isselect)
    {
        SelectSprite.SetActive(isselect);
    }

    private void OnClick()
    {
        this.MessageEventObject.SendMessage("OnClickItemSelect", DataPos);
    }

    private void OnDestroy()
    {
        ItemObject = null;
    }

    public void OnClick_AddSlot()
    {
        this.MessageEventObject.SendMessage("OnClickItem_AddSlot", DataPos);
    }
}
