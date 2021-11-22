using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundles;

public class SoundManager : MonoBehaviour
{
    private static bool applicationIsQuitting = false;

    private static SoundManager m_instance = null;
    public static SoundManager Instance
    {
        get
        {
            if (applicationIsQuitting)
                return null;

            if (m_instance == null)
            {
                GameObject obj = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.SoundManager);
                DontDestroyOnLoad(obj);

                m_instance = obj.GetComponent<SoundManager>();
                m_instance.LoadOptionValue();
            }

            return m_instance;
        }
    }

    void OnApplicationQuit()
    {
        m_instance = null;
        applicationIsQuitting = true;
    }

    public bool IsCreateInstance() { return m_instance != null; }

    #region private value

    private const float maxVolumeBGM = 1.0f;
    private const float maxVolumeSFX = 1.0f;
    private const int sfxChannel = 16;

    private float currentVolumeBGM = 1.0f;
    private float currentVolumeSFX = 1.0f;

    private bool mutedBGM = false;
    public bool IsMutedBGM
    {
        get
        {
            return mutedBGM;
        }
    }
    private bool mutedFX = false;
    public bool IsMutedFX
    {
        get
        {
            return mutedFX;
        }
    }

    private bool m_mutedWhole = false;
    public bool MutedWhole
    {
        get
        {
            return m_mutedWhole;
        }
        set
        {
            m_mutedWhole = value;
            SaveOptionValue();
        }
    }

    private List<AudioSource> sfxSources;

    private AudioSource m_bgmSource;
    private AudioSource bgmSource
    {
        get
        {
            if (m_bgmSource == null)
            {
                m_bgmSource = gameObject.AddComponent<AudioSource>();
                m_bgmSource.loop = true;
                m_bgmSource.playOnAwake = false;
                m_bgmSource.volume = GetBGMVolume();
            }
            return m_bgmSource;
        }
    }


    #endregion

    #region init

    public void Initilize()
    {
        string[] bgms = AssetBundleManager.Instance.GetAllAssetName("assetdata_sound_bgm");

        for (int index = 0; index < bgms.Length; index++)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(bgms[index]);
            ResourceManager.Instance.LoadResourceFromCache<AudioClip>(name);
        }

        string[] fxs = AssetBundleManager.Instance.GetAllAssetName("assetdata_sound_fx");

        for (int index = 0; index < fxs.Length; index++)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(fxs[index]);
            ResourceManager.Instance.LoadResourceFromCache<AudioClip>(name);
        }
    }

    #endregion


    #region fade

    private void BGMFadeOut(float duration)
    {
        if (bgmSource.clip == null)
        {
            Debug.Log("Could not fade BGM out as BGM AudioSource has no currently playing clip.");
            return;
        }

        StartCoroutine(BGMFade(GetBGMVolume(), 0.0f, 0.0f, duration));
    }

    private void BGMFadeIn(AudioClip clip, float delay = 0.0f, float duration = 0.0f)
    {
        bgmSource.clip = clip;
        bgmSource.volume = 0.0f;
        bgmSource.Play();

        StartCoroutine(BGMFade(0.0f, GetBGMVolume(), delay, duration));
    }

    private IEnumerator BGMFadeOutIn(AudioClip clip, float delay = 0.0f, float duration = 0.0f)
    {
        yield return StartCoroutine(BGMFade(GetBGMVolume(), 0.0f, 0.0f, duration));

        bgmSource.clip = clip;
        bgmSource.volume = 0.0f;
        bgmSource.Play();

        yield return StartCoroutine(BGMFade(0.0f, GetBGMVolume(), delay, duration));
    }

    private IEnumerator BGMFade(float fromVol, float toVol, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float t = (elapsed / duration);
            float volume = Mathf.Lerp(fromVol, toVol, t);
            bgmSource.volume = volume;

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (Mathf.Approximately(bgmSource.volume, 0.0f))
            bgmSource.Stop();
    }

    #endregion


    #region bgm

    public void PlayBGM(string name, bool fade = false, float fadeDuration = 0.0f)
    {
        AudioClip ac = ResourceManager.Instance.LoadResourceFromCache<AudioClip>(name);

        if (ac == null)
            return;

        PlayBGM(ac, fade, fadeDuration);
    }

    public void PlayBGM(AudioClip clip, bool fade = false, float fadeDuration = 0.0f)
    {
        if (fade)
        {
            if (bgmSource.isPlaying)
                StartCoroutine(BGMFadeOutIn(clip, fadeDuration / 2, fadeDuration / 2));
            else
                BGMFadeIn(clip, 0.0f, fadeDuration);
        }
        else
        {
            bgmSource.volume = GetBGMVolume();
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void StopBGM(bool fade = false, float fadeDuration = 0.0f)
    {
        if (!bgmSource.isPlaying)
            return;

        if (fade)
            BGMFadeOut(fadeDuration);
        else
            bgmSource.Stop();
    }

    #endregion


    #region sfx

    private AudioSource GetSFXSource()
    {
        AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = GetSFXVolume();

        if (sfxSources == null)
            sfxSources = new List<AudioSource>();

        sfxSources.Add(sfxSource);

        return sfxSource;
    }

    private IEnumerator RemoveSFXSource(AudioSource sfxSource)
    {
        yield return new WaitForSeconds(sfxSource.clip.length);
        sfxSources.Remove(sfxSource);
        Destroy(sfxSource);
    }

    private IEnumerator RemoveSFXSourceFixedDuration(AudioSource sfxSource, float length)
    {
        yield return new WaitForSeconds(length);
        sfxSources.Remove(sfxSource);
        Destroy(sfxSource);
    }

    public void PlaySFX(string name, bool useDistance = false, Vector3 position = default(Vector3))
    {
        AudioClip ac = ResourceManager.Instance.LoadResourceFromCache<AudioClip>(name);

        if (ac == null)
            return;

        PlaySFX(ac, useDistance, position);
    }

    public void PlaySFXFixedDuration(string name, float duration, bool useDistance = false, Vector3 position = default(Vector3))
    {
        AudioClip ac = ResourceManager.Instance.LoadResourceFromCache<AudioClip>(name);

        if (ac == null)
            return;

        PlaySFXFixedDuration(ac, duration, useDistance, position);
    }

    public void PlaySFX(AudioClip clip, bool useDistance = false, Vector3 position = default(Vector3))
    {
        AudioSource source = GetSFXSource();
        source.clip = clip;
        source.volume = GetSFXVolume();

        if (useDistance)
        {
            source.spatialBlend = 1.0f;
            source.spread = 50.0f;
            source.minDistance = 5.0f;
            source.maxDistance = 5000.0f;
            source.transform.position = position;
        }

        source.Play();

        StartCoroutine(RemoveSFXSource(source));
    }

    public void PlaySFXFixedDuration(AudioClip clip, float duration, bool useDistance = false, Vector3 position = default(Vector3))
    {
        AudioSource source = GetSFXSource();
        source.clip = clip;
        source.volume = GetSFXVolume();
        source.loop = true;

        if (useDistance)
        {
            source.spatialBlend = 1.0f;
            source.spread = 50.0f;
            source.minDistance = 5.0f;
            source.maxDistance = 5000.0f;
            source.transform.position = position;
        }

        source.Play();

        StartCoroutine(RemoveSFXSourceFixedDuration(source, duration));
    }

    #endregion

    #region volume

    public float GetBGMVolume()
    {
        return mutedBGM || m_mutedWhole ? 0.0f : maxVolumeBGM * currentVolumeBGM;
    }

    public float GetSFXVolume()
    {
        return mutedFX || m_mutedWhole ? 0.0f : maxVolumeSFX * currentVolumeSFX;
    }

    public void DisableSound()
    {
        StopAllCoroutines();

        mutedBGM = true;
        mutedFX = true;

        if (sfxSources != null)
        {
            foreach (AudioSource source in sfxSources)
                source.volume = 0;
        }

        bgmSource.volume = 0;
    }

    public void EnableSound()
    {
        mutedBGM = false;
        mutedFX = false;

        if (sfxSources != null)
        {
            foreach (AudioSource source in sfxSources)
                source.volume = GetSFXVolume();
        }

        bgmSource.volume = GetBGMVolume();
    }

    public void MuteWhole(bool mute)
    {
        m_mutedWhole = mute;
        mutedBGM = mute;
        mutedFX = mute;
        MuteBGM(mutedBGM);
        MuteFX(mutedFX);

        SaveOptionValue();
    }

    public void MuteBGM(bool mute)
    {
        mutedBGM = mute;

        if (mutedBGM)
            bgmSource.volume = 0;
        else
            bgmSource.volume = GetBGMVolume();
    }

    public void MuteFX(bool mute)
    {
        mutedFX = mute;

        if (mutedFX)
        {
            if (sfxSources != null)
            {
                foreach (AudioSource source in sfxSources)
                    source.volume = 0;
            }
        }
        else
        {
            if (sfxSources != null)
            {
                foreach (AudioSource source in sfxSources)
                    source.volume = GetSFXVolume();
            }
        }
    }

    public void AdjustSound()
    {
        if (sfxSources != null)
        {
            foreach (AudioSource source in sfxSources)
                source.volume = GetSFXVolume();
        }

        bgmSource.volume = GetBGMVolume();
    }

    public void SetGlobalVolume(float volume)
    {
        currentVolumeBGM = volume;
        currentVolumeSFX = volume;

        AdjustSound();
    }

    public void SetBGMVolume(float volume)
    {
        currentVolumeBGM = volume;

        AdjustSound();
    }

    public void SetSFXVolume(float volume)
    {
        currentVolumeSFX = volume;

        AdjustSound();
    }

    #endregion

    private Dictionary<UISOUND_ID, AudioSource> m_audiosourceList = new Dictionary<UISOUND_ID, AudioSource>();
    public void PlaySound(UISOUND_ID id, bool loop = false, Vector3 position = default(Vector3))
    {
        AudioSource source = null;
        if (m_audiosourceList.ContainsKey(id))
        {
            source = m_audiosourceList[id];
        }
        else
        {

            UISound_DataProperty property = UISound_Table.Instance.GetUISoundDataProperty(id);
            if (property == null)
                return;

            switch (property.SoundType)
            {
                case UISOUND_TYPE.SFX:
                    PlaySFX(property.Path);
                    break;

                case UISOUND_TYPE.BGM:
                    PlayBGM(property.Path);
                    break;

                case UISOUND_TYPE.Ambient:
                    PlaySFX(property.Path, true, position);
                    break;

                default:
                    break;
            }
        }
    }

    public void StopSound(UISOUND_ID id)
    {
        if (m_audiosourceList.ContainsKey(id) == false)
            return;

        m_audiosourceList[id].Stop();
    }

    public void ClearSound()
    {
        foreach (AudioSource source in m_audiosourceList.Values)
        {
            if (source == null)
                continue;

            Destroy(source.gameObject);
        }

        m_audiosourceList.Clear();
    }

    private void OnDestroy()
    {
        ClearSound();
    }

    public void LoadOptionValue()
    {
        bool whole = false;
        int vol1 = 0, vol2 = 0;
        bool onoff1 = false, onoff2 = false;
        if (GoogleGamesManager.Instance.IsLoadedGameData())
        {
            GoogleGamesManager.Instance.MyGameSaveData.LoadOption(ref whole, ref vol1, ref onoff1, ref vol2, ref onoff2);
        }
        else
        {
            PlayerPrefabsID.LoadOptionValue(ref whole, ref vol1, ref onoff1, ref vol2, ref onoff2);
        }

        SetBGMVolume(vol1 / 100.0f);
        MuteBGM(onoff1);
        SetSFXVolume(vol2 / 100.0f);
        MuteFX(onoff2);
        MuteWhole(whole);
    }

    public void SaveOptionValue()
    {
        if (GoogleGamesManager.Instance.IsLoadedGameData())
        {
            GoogleGamesManager.Instance.MyGameSaveData.SaveOption(MutedWhole, (int)(GetCurrentBGMVolume() * 100), mutedBGM, (int)(GetCurrentSFXVolume() * 100), mutedFX);
            GoogleGamesManager.Instance.SaveToCloud();
        }
        else
            PlayerPrefabsID.SaveOption(MutedWhole, (int)(GetCurrentBGMVolume() * 100), mutedBGM, (int)(GetCurrentSFXVolume() * 100), mutedFX);
    }

    public float GetCurrentBGMVolume()
    {
        return currentVolumeBGM;
    }

    public float GetCurrentSFXVolume()
    {
        return currentVolumeSFX;
    }
}