using UnityEngine;
using System.Collections;

public class UIGameLoading : MonoBehaviour
{
    private static UIGameLoading m_instance = null;
    public static UIGameLoading Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject obj = ResourceManager.Instance.LoadResourceObject(UIResourcesNameDef.LoadingScreen);
                DontDestroyOnLoad(obj);
                m_instance = obj.GetComponent<UIGameLoading>();
            }

            return m_instance;
        }
    }

    public static bool IsCreateInstance()
    {
        return m_instance != null;
    }

    public GameObject TargetObject;
    public UISlider LoadingGage;
    public SceneTransition LoadingTransition;
    
    float To_black_time = .2f;
    float To_clear_time = .3f;

    private float endValue;
    private float transitionValue;

    public enum LoadingState
    {
        None,
        Before_Start,
        Start,
        After_Start,
        Before_End,
        End,
        After_End
    }
    private LoadingState m_loadingState = LoadingState.None;
    public LoadingState CurrentLoadingState
    {
        get
        {
            return m_loadingState;
        }
        
        set
        {
            m_loadingState = value;
            switch(value)
            {
                case LoadingState.Start:
                    TargetObject.SetActive(true);
                    break;
                case LoadingState.End:
                    TargetObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }

    public void SetMode(TRANSITIONMODE mode)
    {
        if(LoadingTransition == null)
            return;

        LoadingTransition.SetMode(mode);

        Camera cam = LoadingTransition.gameObject.GetComponent<Camera>();

        if(cam != null)
            cam.depth = 1000;
    }

    public IEnumerator BeforeStartLoading()
    {
        if(LoadingTransition == null)
            yield break;

        if(LoadingTransition.CurrentMode != TRANSITIONMODE.TRANSITION_NONE)
            yield return ToBlack();

        CurrentLoadingState = LoadingState.Before_Start;
    }

    public void StartLoading()
    {
        LoadingGage.Set(0.0f);
        CurrentLoadingState = LoadingState.Start;
    }

    public IEnumerator AfterStartLoading()
    {
        if(LoadingTransition == null)
            yield break;

        if(LoadingTransition.CurrentMode != TRANSITIONMODE.TRANSITION_NONE)
            yield return ToClear();

        CurrentLoadingState = LoadingState.After_Start;
    }

    public IEnumerator BeforeEndLoading()
    {
        if(LoadingTransition == null)
            yield break;

        if(LoadingTransition.CurrentMode != TRANSITIONMODE.TRANSITION_NONE)
            yield return ToBlack();

        CurrentLoadingState = LoadingState.Before_End;
    }

    public void EndLoading()
    {
        CurrentLoadingState = LoadingState.End;
    }

    public IEnumerator AfterEndLoading()
    {
        if(LoadingTransition == null)
            yield break;

        if(LoadingTransition.CurrentMode != TRANSITIONMODE.TRANSITION_NONE)
            yield return ToClear();

        CurrentLoadingState = LoadingState.After_End;
    }

    public void SetLoadingGuage(float t)
    {
        LoadingGage.Set(Mathf.Clamp01(t));
    }

    public IEnumerator StartGage()
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < 0.5f)
        {
            LoadingGage.Set(Mathf.Lerp(0.0f, 1.0f, (elapsedTime / 0.5f)));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator ToBlack()
    {
        if (To_black_time <= 0f)
            To_black_time = 0.1f;

        float dt = 0.0f;
        //Tiem Adjust
        while (dt < To_black_time)
        {
            yield return new WaitForEndOfFrame();
            dt += Time.deltaTime;
            float t = Mathf.Clamp01(dt / To_black_time);
            LoadingTransition.SetTransition(t);
        }
    }

    public IEnumerator ToClear()
    {
        if (To_clear_time <= 0f)
            To_clear_time = 0.1f;

        // Time adjust
        float dt = To_clear_time;
        while(dt > 0.0f)
        {
            yield return new WaitForEndOfFrame();

            dt -= Time.deltaTime;
            float t = Mathf.Clamp01(dt / To_clear_time);
            LoadingTransition.SetTransition(t);
        }
    }

    public void StartTransition(float delta)
    {
        if(LoadingTransition == null)
            return;
        
        LoadingTransition.SetTransition(delta);
    }
}
