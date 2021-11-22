using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameLoadingForColor : UIGameLoading
{
    public UISprite TargetSprite;

    public void SetTargetColor(Color color)
    {
        LoadingTransition.SetTargetColor(color);
    }
}
