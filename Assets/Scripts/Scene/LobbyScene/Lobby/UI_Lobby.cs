using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Lobby : MonoBehaviour
{
    public void OnClick_Solo()
    {
        SendMessageUpwards("OnClick_Solo");
    }

    public void OnClick_Leader()
    {
        SendMessageUpwards("OnClick_Leader1");
    }

    public void OnClick_Exit()
    {
        SendMessageUpwards("Close");
    }
}
