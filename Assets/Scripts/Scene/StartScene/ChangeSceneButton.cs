using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class ChangeSceneButton : MonoBehaviour
{
    public SceneState m_targetScene = SceneState.Start;

    void OnClick()
    {
        InGameUIScene.Instance.ChangeScene(m_targetScene);
    }
}
