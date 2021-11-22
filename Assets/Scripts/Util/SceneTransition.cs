using UnityEngine;
using System.Collections;

public enum TRANSITIONMODE
{
    TRANSITION_NONE,
    TRANSITION_CUTOFF,
    TRANSITION_FADE,
}

[ExecuteInEditMode]
public class SceneTransition : MonoBehaviour
{
    public Material TransitionMaterial;
    public TRANSITIONMODE CurrentMode;
    public Texture[] TransitionTex;

    public void SetTargetColor(Color color)
    {
        TransitionMaterial.SetColor("_Color", color);
    }

    private float m_oldcutoff = -1.0f;
    private float m_oldfade = -1.0f;

    private void SetValue(float cutoff, float fade)
    {
        m_oldcutoff = cutoff;
        m_oldfade = fade;
        TransitionMaterial.SetFloat("_Cutoff", m_oldcutoff);
        TransitionMaterial.SetFloat("_Fade", m_oldfade);
    }


    public void SetMode(TRANSITIONMODE mode, int transition_num = -1)
    {
        CurrentMode = mode;

        if (CurrentMode == TRANSITIONMODE.TRANSITION_CUTOFF)
        {
            SetValue(0.0f, 1.0f);

            if (TransitionTex.Length > 0)
            {
                int random;
                if (transition_num == -1 ||
                    transition_num >= TransitionTex.Length)
                {
                    random = Random.Range(0, TransitionTex.Length);
                }
                else
                {
                    random = transition_num;
                }
                TransitionMaterial.SetTexture("_TransitionTex", TransitionTex[random]);
            }
        }
        else if (CurrentMode == TRANSITIONMODE.TRANSITION_FADE || CurrentMode == TRANSITIONMODE.TRANSITION_NONE)
        {
            SetValue(1.0f, 0.0f);
        }
    }

    public void SetTransition(float delta)
    {
        if (CurrentMode == TRANSITIONMODE.TRANSITION_CUTOFF)
            TransitionMaterial.SetFloat("_Cutoff", delta);
        else if (CurrentMode == TRANSITIONMODE.TRANSITION_FADE)
            TransitionMaterial.SetFloat("_Fade", delta);

//         if (CurrentMode == TRANSITIONMODE.TRANSITION_CUTOFF && delta == 0.0f)
//             CurrentMode = TRANSITIONMODE.TRANSITION_NONE;

//        Debug.LogError("mode : " + CurrentMode + " , delata : " + delta);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (m_oldfade <= 0.0f)
            return;
        if (TransitionMaterial == null)
            return;
        Graphics.Blit(src, dst, TransitionMaterial);
    }
}
