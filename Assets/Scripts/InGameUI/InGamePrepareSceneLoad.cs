using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public abstract class InGamePrepareSceneLoad
{
    public InGamePrepareSceneLoad()
    {
        InitPrepareState();
    }

    public enum PrepareStateType
    {
        Wait,
        Fail,
        Success,
    }

    protected PrepareStateType m_prepareState = PrepareStateType.Wait;
    public abstract void InitPrepareState();
    public abstract void ActionPrepareState();

    public virtual PrepareStateType GetPrepareState()
    {
        return m_prepareState;
    }
}

public class InGamePrepareSceneLoad_Lobby : InGamePrepareSceneLoad
{
    public override void InitPrepareState()
    {
        m_prepareState = PrepareStateType.Wait;
    }

    public override void ActionPrepareState()
    {
        InGameManager.Instance.CurrentPlayMode = InGameManager.PLAY_MODE.None;

        ResourceManager.Instance.PreLoadGameObjectByTable(null, UIGAMEOBJECT_TYPE.ArcadeNew, -1, true);
        ResourceManager.Instance.PreLoadGameObjectByTable(null, UIGAMEOBJECT_TYPE.ArcadeManager, -1, true);
        ResourceManager.Instance.PreLoadGameObjectByTable(null, UIGAMEOBJECT_TYPE.Arcade_Camera, -1, true);

        m_prepareState = PrepareStateType.Success;
    }
}

public class InGamePrepareSceneLoad_InGame: InGamePrepareSceneLoad
{
    public override void InitPrepareState()
    {
        m_prepareState = PrepareStateType.Wait;
    }

    public override void ActionPrepareState()
    {
        m_prepareState = PrepareStateType.Success;
    }
}

