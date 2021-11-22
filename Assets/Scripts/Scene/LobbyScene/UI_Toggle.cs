using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Toggle : MonoBehaviour
{
    public int DefalutIndex = 0;
    private int m_currentIndex = 0;
    public int CurrentIndex
    {
        get
        {
            return m_currentIndex;
        }
    }
    private List<UIButton> m_groupButtons = new List<UIButton>();
    private List<UILabel> m_groupLabelss = new List<UILabel>();

    private void Awake()
    {
        m_groupLabelss = new List<UILabel>(this.GetComponentsInChildren<UILabel>());
        m_groupButtons = new List<UIButton>(this.GetComponentsInChildren<UIButton>());

        for (int i = 0; i < m_groupButtons.Count;++i)
        {
            m_groupButtons[i].hover = Color.white;
            m_groupButtons[i].pressed = Color.white;
            m_groupButtons[i].disabledColor = Color.white;

            m_groupButtons[i].onClick.Clear();
            EventDelegate onclick = new EventDelegate(this, "OnClick_Button");
            onclick.parameters[0].value = i;
            m_groupButtons[i].onClick.Add(onclick);
        }

        OnClick_Button(DefalutIndex);
    }

    private void OnClick_Button(int index)
    {
        m_currentIndex = index;
        for (int i = 0; i < m_groupButtons.Count; ++i)
        {
            m_groupButtons[i].defaultColor = (i == index ? Color.white : Color.gray);
            m_groupLabelss[i].color = (i == index) ? Color.yellow : Color.white;
        }
    }
}
