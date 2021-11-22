using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

[System.Serializable]
public class BallInfos
{
    public BallInfos(int index)
    {
        InitSecrue();

        Index = index;
        Score = 2;
        BallType = 0;
    }

    public BallInfos(int index, int type, int score)
    {
        InitSecrue();

        Index = index;
        Score = score;
        BallType = type;
    }

    DelegateSecrueProperty<int> m_encryption_Index = null;
    DelegateSecrueProperty<int> m_encryption_Type = null;
    DelegateSecrueProperty<int> m_encryption_score = null;

    public void InitSecrue()
    {
        m_encryption_Index = new DelegateSecrueProperty<int>();
        m_encryption_Index.InitSecrue();
        m_encryption_Index.Value = m_index;

        m_encryption_Type = new DelegateSecrueProperty<int>();
        m_encryption_Type.InitSecrue();
        m_encryption_Type.Value = m_balltype;

        m_encryption_score = new DelegateSecrueProperty<int>();
        m_encryption_score.InitSecrue();
        m_encryption_score.Value = m_score;
    }

    public void AdjustSecure()
    {
        m_index = m_encryption_Index.Value;
        m_balltype = m_encryption_Type.Value;
        m_score = m_encryption_score.Value;
    }

    public void UpdateBallType(int type)
    {
        BallType = type;
    }

    public void UpdateBallInfo(int score, int balltype)
    {
        Score = score;
        BallType = balltype;
    }

    public int m_index = 0;
    public int m_balltype = 0;
    public int m_score = 2;

    public int Index
    {
        get
        {
            return m_encryption_Index.Value;
        }
        set
        {
            m_encryption_Index.Value = value;
        }
    }

    
    public int BallType
    {
        get
        {
            return m_encryption_Type.Value;
        }
        private set
        {
            m_encryption_Type.Value = value;
        }
    }


    public int Score
    {
        get
        {
            return m_encryption_score.Value;
        }
        private set
        {
            m_encryption_score.Value = value;
        }
    }
    public ulong m_skintype = 0;
    public ulong SkinType
    {
        get
        {
            return m_skintype;
        }
        private set
        {
            m_skintype = value;
        }
    }

    public void SetSkinType(int skin)
    {
        m_skintype |= ((ulong)1 << skin);
    }
}

public class AccountManager : Singleton<AccountManager>
{
    private const int DEFAULT_SCORE = 2;
    private const int DEFAULT_TYPE = 0;

    public class Account_Infos
    {
        private ENG_Encryption m_Encryption = new ENG_Encryption("62306084");
        private string m_encryption_Gold = string.Empty;
        private string m_encryption_Score = string.Empty;

//        private int m_myGold = -1;
        public int Gold
        {
            get
            {
                if (GoogleGamesManager.Instance.IsLoadedGameData())
                    return GetEncryptionValue(ref m_encryption_Gold, GoogleGamesManager.Instance.MyGameSaveData.Gold.ToString(), PlayerPrefabsID.GetGold());
                else
                    return GetEncryptionValue(ref m_encryption_Gold, string.Empty, PlayerPrefabsID.GetGold());
            }
            set
            {
                m_encryption_Gold = m_Encryption.Encrypt(value.ToString());

                if (m_myGoldFunc != null)
                    m_myGoldFunc(value);

                if (GoogleGamesManager.Instance.IsLoadedGameData())
                {
                    GoogleGamesManager.Instance.MyGameSaveData.Gold = value;
                    GoogleGamesManager.Instance.SaveToCloud();
                }
                else
                    PlayerPrefabsID.SaveGold(m_encryption_Gold);
            }
        }

        private int GetEncryptionValue(ref string str_value, string google_value, string prefabs_value)
        {
            if (string.IsNullOrEmpty(str_value))
            {
                if (GoogleGamesManager.Instance.IsLoadedGameData())
                    str_value = m_Encryption.Encrypt(google_value);
                else
                    str_value = prefabs_value;
            }

            if (string.IsNullOrEmpty(str_value))
                str_value = m_Encryption.Encrypt("0");

            return int.Parse(m_Encryption.Decrypt(str_value));
        }

//        private int m_myScore = -1;
        public int MyScore
        {
            get
            {
                if (GoogleGamesManager.Instance.IsLoadedGameData())
                    return GetEncryptionValue(ref m_encryption_Score, GoogleGamesManager.Instance.MyGameSaveData.Score.ToString(), PlayerPrefabsID.GetScore());
                else
                    return GetEncryptionValue(ref m_encryption_Score, string.Empty, PlayerPrefabsID.GetScore());
            }
            set
            {
                m_encryption_Score = m_Encryption.Encrypt(value.ToString());

                if (GoogleGamesManager.Instance.IsLoadedGameData())
                {
                    GoogleGamesManager.Instance.MyGameSaveData.Score = value;
                    GoogleGamesManager.Instance.SaveToCloud();
                }
                else
                    PlayerPrefabsID.SaveScore(m_encryption_Score);
            }
        }

        private System.Action<int> m_myGoldFunc = null;
        public System.Action<int> MyGoldFunc
        {
            set
            {
                m_myGoldFunc = value;
                if (m_myGoldFunc != null)
                    m_myGoldFunc(Gold);
            }
            get
            {
                return m_myGoldFunc;
            }
        }

        List<Shop_DataProperty> m_myitemList = new List<Shop_DataProperty>();
        public void AddItem(Shop_DataProperty property)
        {
            m_myitemList.Add(property);
        }
    }

    private Account_Infos m_myAccountInfo = null;
    public Account_Infos MyAccountInfo
    {
        get
        {
            if (m_myAccountInfo == null)
            {
                m_myAccountInfo = new Account_Infos();
            }
            return m_myAccountInfo;
        }
    }

    public void AdReward()
    {
        Debug.LogError("AdReward 1");
        MyAccountInfo.Gold += GlobalValue_Table.Instance.AD_REWARD_GOLD;
    }

    public string GetUserName()
    {
        if (GoogleGamesManager.Instance.IsSignIn())
            return GoogleGamesManager.Instance.UserName;

        return PlayerPrefs.GetString(PlayerPrefabsID.UserName, string.Empty);
    }

    public string GetPassword()
    {
        return PlayerPrefs.GetString(PlayerPrefabsID.PassWord, string.Empty);
    }

    [System.Serializable]
    public class BallInfosList
    {
        public List<BallInfos> m_ballList;

        public string SaveToString()
        {
            AdjustSecure();
            return SaveToString(this);
        }

        private static string SaveToString(object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public static BallInfosList CreateFromJSON(string jsonString)
        {
            BallInfosList list = JsonUtility.FromJson<BallInfosList>(jsonString);
            list.InitSecrue();
            return list;
        }

        public BallInfos GetMyBallInfo(int index)
        {
            return m_ballList.Find(x => x.Index == index);
        }

        public void UpdateBallInfo(int index, int type, int score)
        {
            BallInfos info = GetMyBallInfo(index);
            if (info == null)
            {
                info = new BallInfos(index, type, score);
                m_ballList.Add(info);
            }
            else
            {
                info.UpdateBallInfo(score, type);
            }
        }

        private void AdjustSecure()
        {
            foreach (BallInfos info in m_ballList)
            {
                info.AdjustSecure();
            }
        }

        private void InitSecrue()
        {
            foreach(BallInfos info in m_ballList)
            {
                info.InitSecrue();
            }
        }
    }

    private BallInfosList m_ballList = null;
    public BallInfosList MyBallList
    {
        get
        {
            if (m_ballList == null)
            {
                string json = null;
                if (GoogleGamesManager.Instance.IsLoadedGameData())
                {
                    json = GoogleGamesManager.Instance.MyGameSaveData.BallListStr;
                }
                else
                {
                    json = PlayerPrefabsID.GetBallList();
                }

                if (string.IsNullOrEmpty(json))
                {
                    m_ballList = new BallInfosList();
                    m_ballList.m_ballList = new List<BallInfos>();
                    m_ballList.m_ballList.Add(new BallInfos(0, 0, 2));
                    m_ballList.m_ballList.Add(new BallInfos(1, 0, 2));
                    m_ballList.m_ballList.Add(new BallInfos(2, 0, 2));
                    SaveBallInfos();
                }
                else
                    m_ballList = BallInfosList.CreateFromJSON(json);
            }

            return m_ballList;
        }
    }

    public int GetEquipSkinCount(int type)
    {
        int equipcount = 0;
        foreach (BallInfos info in MyBallList.m_ballList)
        {
            if (info.BallType == type)
            {
                equipcount++;
            }
        }
        return equipcount;
    }

    public int GetAvailableSkinCount(int type)
    {
        int havecount = 0;
        foreach (BallInfos info in MyBallList.m_ballList)
        {
            ulong checktype = (info.SkinType >> (int)type) & 0x1;
            if (checktype == 1)
                havecount++;
        }

        if (havecount == 0)
        {
            return int.MinValue;
        }

        return havecount - GetEquipSkinCount(type);
    }

    public void SetSkin(int type)
    {
        foreach (BallInfos info in MyBallList.m_ballList)
        {
            ulong checktype = (info.SkinType >> (int)type) & 0x1;
            if (checktype == 0)
            {
                info.SetSkinType(type);
                return;
            }
        }
    }

    public BallInfos GetMyBallInfo(int index)
    {
        return MyBallList.GetMyBallInfo(index);
    }

    public void UpdateBallInfo()
    {
        UpdateBallInfo(CurrentSelectBallInfo.Index,
            CurrentSelectBallInfo.BallType,
            CurrentSelectBallInfo.Score);
    }

    public void UpdateBallInfo(int index)
    {
        UpdateBallInfo(index, DEFAULT_TYPE, DEFAULT_SCORE);
    }

    public void UpdateBallInfo(int index, int type, int score)
    {
        MyBallList.UpdateBallInfo(index, type, score);
        SaveBallInfos();
    }

    private void SaveBallInfos()
    { 
        if (GoogleGamesManager.Instance.IsLoadedGameData())
        {
            GoogleGamesManager.Instance.MyGameSaveData.BallListStr = MyBallList.SaveToString();
            GoogleGamesManager.Instance.SaveToCloud();
        }
        else
            PlayerPrefabsID.SetBallList(MyBallList.SaveToString());
    }

    private BallInfos m_currentselectBallInfo = null;
    public BallInfos CurrentSelectBallInfo
    {
        get
        {
            return m_currentselectBallInfo;
        }
        set
        {
            m_currentselectBallInfo = value;
        }
    }

    private int m_mouseStrength = int.MinValue;
    public int MouseStrength
    {
        get
        {
            LoadGameOption();
            return m_mouseStrength;
        }
        set
        {
            m_mouseStrength = value;
        }
    }

    private bool m_ismouseMode = false;
    public bool IsMouseMode
    {
        get
        {
            LoadGameOption();
            return m_ismouseMode;
        }
        set
        {
            m_ismouseMode = value;
        }
    }

    private void LoadGameOption()
    {
        if (m_mouseStrength != int.MinValue)
            return;

        PlayerPrefabsID.LoadGameOption(ref m_ismouseMode, ref m_mouseStrength);
    }

    public void SaveGameOption()
    {
        PlayerPrefabsID.SaveGameOption(m_ismouseMode, m_mouseStrength);
    }
}
