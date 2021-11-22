using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_OptionButtonOnOff : MonoBehaviour
{
    private void OnClick()
    {
        SendMessageUpwards("OnClickOnOff");
    }
}
