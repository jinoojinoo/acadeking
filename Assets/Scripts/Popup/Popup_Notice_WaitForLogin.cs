using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Notice_WaitForLogin : PopupBase
{
    private void Start()
    {
        if (InGameUIScene.Instance == null)
            return;

        InGameUIScene.Instance.SetUICameraEnable(false);
    }

    private void OnDestroy()
    {
        if (InGameUIScene.Instance == null)
            return;

        InGameUIScene.Instance.SetUICameraEnable(true);
    }
}
