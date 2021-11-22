using UnityEngine;
using System;
using System.Collections;
//gpg
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
//for encoding
using System.Text;
//for extra save ui
using UnityEngine.SocialPlatforms;
//for text, remove
using UnityEngine.UI;
public partial class GoogleGamesManager : SingletonMono<GoogleGamesManager>
{
    public bool isProcessing
    {
        get;
        private set;
    }
    public string loadedData
    {
        get;
        private set;
    }
    private const string m_saveFileName = "game_save_data";

    private GameSaveData m_mygamesaveData = null;
    public GameSaveData MyGameSaveData
    {
        get
        {
            return m_mygamesaveData;
        }
    }
    public bool IsLoadedGameData()
    {
        return m_mygamesaveData != null && m_mygamesaveData.IsLoaded;
    }

    public bool isAuthenticated
    {
        get
        {
            return Social.localUser.authenticated;
        }
    }

    private void ProcessCloudData(byte[] cloudData)
    {
        if (cloudData == null)
        {
            Debug.Log("No Data saved to the cloud");
            return;
        }

        Debug.Log("load to the cloud : " + cloudData);

        string progress = BytesToString(cloudData);
        loadedData = progress;
    }


    private System.Action<bool> m_loadAction = null;
    public void LoadFromCloud(System.Action<bool> action)
    {
        m_loadAction = action;
        if (m_loadAction != null)
            m_loadAction(false);
        LoadFromCloud(action, InitLoadData);
    }

    private void InitLoadData(string data)
    {
        if (data == null || string.IsNullOrEmpty(data))
        {
            LoadFromPlayerPrefab();
            SaveToCloud();

            return;
        }

        m_mygamesaveData = GameSaveData.CreateFromJSON(data);
        m_mygamesaveData.IsLoaded = true;
    }

    private void LoadFromPlayerPrefab()
    {
        m_mygamesaveData = new GameSaveData();
        m_mygamesaveData.IsLoaded = true;
        m_mygamesaveData.LoadFromPlayerPrefab();
    }

    private void LoadFromCloud(System.Action<bool> action, Action<string> afterLoadAction)
    {
        if (isAuthenticated && !isProcessing)
        {
            StartCoroutine(LoadFromCloudRoutin((string str) =>
            {
                if (afterLoadAction != null)
                    afterLoadAction(str);

                if (m_loadAction != null)
                    m_loadAction(true);
            }));
        }
    }

    private IEnumerator LoadFromCloudRoutin(Action<string> loadAction)
    {
        isProcessing = true;
        Debug.Log("Loading game progress from the cloud.");

#if UNITY_ANDROID
        ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(
            m_saveFileName, //name of file.
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            OnFileOpenToLoad);

        while (isProcessing)
        {
            yield return null;
        }

        loadAction.Invoke(loadedData);
#else
        yield break;
#endif

    }

    public void SaveToCloud()
    {
        Debug.LogError("save");

        MyGameSaveData.SaveToEncryption();
        SaveToCloud(GameSaveData.SaveToString(MyGameSaveData));

#if UNITY_EDITOR
        PlayerPrefabsID.SaveGold(MyGameSaveData.EncryptionGold);
        PlayerPrefabsID.SaveScore(MyGameSaveData.EncryptionScore);
        PlayerPrefabsID.SetBallList(MyGameSaveData.BallListStr);
#endif
    }

    private void SaveToCloud(string dataToSave)
    {
        Debug.LogError("SaveToCloud");
#if UNITY_ANDROID
        if (isAuthenticated)
        {
            loadedData = dataToSave;
            isProcessing = true;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(m_saveFileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, OnFileOpenToSave);
        }
        else
        {
            SignIn();
        }
#endif

    }

    private void OnFileOpenToSave(SavedGameRequestStatus status, ISavedGameMetadata metaData)
    {
#if UNITY_ANDROID
        if (status == SavedGameRequestStatus.Success)
        {
            byte[] data = StringToBytes(loadedData);
            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
            SavedGameMetadataUpdate updatedMetadata = builder.Build();
            ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(metaData, updatedMetadata, data, OnGameSave);
        }
        else
        {
            Debug.LogWarning("Error opening Saved Game" + status);
        }
#endif
    }


    private void OnFileOpenToLoad(SavedGameRequestStatus status, ISavedGameMetadata metaData)
    {
#if UNITY_ANDROID
        if (status == SavedGameRequestStatus.Success)
        {
            ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(metaData, OnGameLoad);
        }
        else
        {
            Debug.LogWarning("Error opening Saved Game" + status);
            isProcessing = false;
        }
#endif
    }

    private void OnGameLoad(SavedGameRequestStatus status, byte[] bytes)
    {
        if (status != SavedGameRequestStatus.Success)
        {
            Debug.LogWarning("Error Saving" + status);
        }
        else
        {
            ProcessCloudData(bytes);
        }

        isProcessing = false;
    }

    private void OnGameSave(SavedGameRequestStatus status, ISavedGameMetadata metaData)
    {
        if (status != SavedGameRequestStatus.Success)
        {
            Debug.LogError("Error Saving" + status);
        }

        Debug.LogError("OnGameSave");
        isProcessing = false;
    }

    private byte[] StringToBytes(string stringToConvert)
    {
        return Encoding.UTF8.GetBytes(stringToConvert);
    }

    private string BytesToString(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
}