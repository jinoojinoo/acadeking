using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData
{
    [System.NonSerialized]
    private bool m_isLoaded = false;
    public bool IsLoaded
    {
        get
        {
            return m_isLoaded;
        }
        set
        {
            m_isLoaded = value;

            if (value)
                LoadToCloud();
        }
    }

    public int OptionValue;

    private ENG_Encryption m_Encryption = new ENG_Encryption("62306084");

    private string m_encryption_Gold = string.Empty;
    public string EncryptionGold
    {
        get
        {
            return m_encryption_Gold;
        }
    }
    private string m_encryption_Score = string.Empty;
    public string EncryptionScore
    {
        get
        {
            return m_encryption_Score;
        }
    }

    public int m_saveGold = 0;
    public int m_saveScore = 0;

    public void SaveToEncryption()
    {
        m_saveGold = Gold;
        m_saveScore = Score;
    }

    public void LoadToCloud()
    {
        Gold = m_saveGold;
        Score = m_saveScore;
    }

    public int Gold
    {
        get
        {
            if (string.IsNullOrEmpty(m_encryption_Gold))
            {
                m_encryption_Gold = m_Encryption.Encrypt("0");
                return 0;
            }
            return int.Parse(m_Encryption.Decrypt(m_encryption_Gold));
        }
        set
        {
            m_encryption_Gold = m_Encryption.Encrypt(value.ToString());
        }
    }

    public int Score
    {
        get
        {
            if (string.IsNullOrEmpty(m_encryption_Score))
            {
                m_encryption_Score = m_Encryption.Encrypt("0");
                return 0;
            }
            return int.Parse(m_Encryption.Decrypt(m_encryption_Score));
        }
        set
        {
            m_encryption_Score = m_Encryption.Encrypt(value.ToString());
        }
    }

    public string BallListStr;

    public static string SaveToString(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static GameSaveData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<GameSaveData>(jsonString);
    }

    public void LoadFromPlayerPrefab()
    {
        OptionValue = PlayerPrefabsID.LoadOptionValue();
        m_encryption_Gold = PlayerPrefabsID.GetGold();
        m_encryption_Score = PlayerPrefabsID.GetScore();
        BallListStr = PlayerPrefabsID.GetBallList();
    }

    public void SaveOption(bool whole, int vol1, bool onoff1, int vol2, bool onoff2)
    {
        int value = PlayerPrefabsID.GetOptionValue(whole, vol1, onoff1, vol2, onoff2);
        PlayerPrefabsID.SaveOption(value);
        OptionValue = value;        
    }

    public void LoadOption(ref bool whole, ref int vol1, ref bool onoff1, ref int vol2, ref bool onoff2)
    {
        PlayerPrefabsID.LoadOptionValue(OptionValue, ref whole, ref vol1, ref onoff1, ref vol2, ref onoff2);
    }
}

public static class TAG_ID
{
    public static string TAG_BackBoard = "backboard";
    public static string TAG_Rim = "rim";
    public static string TAG_Chain = "chain";
    public static string TAG_Bounce = "bounce";
    public static string TAG_Ball = "Ball";

    public enum TAG_TYPE
    {
        Backboard,
        Rim,
        Chain,
        Bounce,
        Ball,
    }

    public static TAG_TYPE GetTagType(GameObject obj)
    {
        string tag = obj.tag;
        if (string.Compare(tag, TAG_BackBoard) == 0)
            return TAG_TYPE.Backboard;
        else if (string.Compare(tag, TAG_Rim) == 0)
            return TAG_TYPE.Rim;
        else if (string.Compare(tag, TAG_Chain) == 0)
            return TAG_TYPE.Chain;
        else if (string.Compare(tag, TAG_Bounce) == 0)
            return TAG_TYPE.Bounce;
        else if (string.Compare(tag, TAG_Ball) == 0)
            return TAG_TYPE.Ball;

        return TAG_TYPE.Backboard;
    }
}

public static class PlayerPrefabsID
{
    public static string LoginType = "LoginType";
    public static string UserName = "UserName";
    public static string PassWord = "PassWord";
    public static string LOGIN_Remember = "LOGIN_Remember";
    public static string GUEST_LOGIN_OK = "GuestLogin";
    public static string OptionValue = "OptionValue";
    private static string GOLD = "GOLD";
    private static string SCORE = "SCORE";
    private static string BALL_LIST = "BallList";
    private static string GameOption = "GameOption";
    private static string SELECT_MODE = "SELECT_MODE";

    public static int GetLoginType()
    {
        return (PlayerPrefs.GetInt(PlayerPrefabsID.LoginType, (int)LOGIN_TYPE.GUEST));
    }
    public static void SetLoginType(LOGIN_TYPE type, bool login)
    {
        int logintype = GetLoginType();
        bool islogin = (logintype & (1 << (int)type)) != 0;
        if (islogin == login)
            return;

        if (login)
            logintype = logintype | (1 << (int)type);
        else
            logintype = logintype & (~(1 << (int)type));

        PlayerPrefs.SetInt(PlayerPrefabsID.LoginType, logintype);
        PlayerPrefs.Save();
    }

    public static void SaveGold(string gold)
    {
        PlayerPrefs.SetString(GOLD, gold);
        PlayerPrefs.Save();
    }

    public static void SaveScore(string score)
    {
        PlayerPrefs.SetString(SCORE, score);
        PlayerPrefs.Save();
    }

    public static string GetGold()
    {
        return PlayerPrefs.GetString(GOLD, string.Empty);
    }

    public static string GetScore()
    {
        return PlayerPrefs.GetString(SCORE, string.Empty);
    }

    public const int DEFAULT_OPTION = (0 << 30) | (0 << 24) | (100 << 16) | (0 << 8) | 100;

    public static void SaveOption(int option)
    {
        PlayerPrefs.SetInt(OptionValue, option);
        PlayerPrefs.Save();
    }
    public static int LoadOptionValue()
    {
        return PlayerPrefs.GetInt(OptionValue, DEFAULT_OPTION);
    }

    public static void LoadOptionValue(int option, ref bool whole, ref int vol1, ref bool onoff1, ref int vol2, ref bool onoff2)
    {
        whole = ((option >> 30) & 0xFF) == 1 ? true : false;

        onoff1 = ((option >> 24) & 0xFF) == 1 ? true : false;
        vol1 = (option >> 16) & 0xFF;

        onoff2 = ((option >> 8) & 0xFF) == 1 ? true : false;
        vol2 = option & 0xFF;
    }

    public static void LoadOptionValue(ref bool whole, ref int vol1, ref bool onoff1, ref int vol2, ref bool onoff2)
    {
        int option = LoadOptionValue();
        LoadOptionValue(option, ref whole, ref vol1, ref onoff1, ref vol2, ref onoff2);
    }

    public static void SaveOption(bool whole, int vol1, bool onoff1, int vol2, bool onoff2)
    {
        int value = GetOptionValue(whole, vol1, onoff1, vol2, onoff2);
        PlayerPrefs.SetInt(OptionValue, value);
        PlayerPrefs.Save();
    }

    public static int GetOptionValue(bool whole, int vol1, bool onoff1, int vol2, bool onoff2)
    {
        return (whole ? 1 << 30 : 0) | (onoff1 ? 1 << 24 : 0) | (vol1 << 16) | (onoff2 ? 1 << 8 : 0) | vol2;
    }

    public static string GetBallList()
    {
        return PlayerPrefs.GetString(BALL_LIST, string.Empty);
    }

    public static void SetBallList(string str)
    {
        PlayerPrefs.SetString(BALL_LIST, str);
        PlayerPrefs.Save();
    }

    private const int DEFAULT_STRENGTH = 30;
    public static void LoadGameOption(ref bool ismouse, ref int strength)
    {
        int gameoption = PlayerPrefs.GetInt(GameOption, -1);
        if (gameoption == -1)
        {
            ismouse = false;
            strength = DEFAULT_STRENGTH;

            SaveGameOption(ismouse, strength);
            return;
        }

        ismouse = ((gameoption >> 8) & 0xFF) == 1 ? true : false;
        strength = gameoption & 0xFF;
    }

    public static void SaveGameOption(bool ismouse, int strength)
    {
        int saveoption = (ismouse ? 1 : 0) << 8 | strength;
        PlayerPrefs.SetInt(GameOption, saveoption);
        PlayerPrefs.Save();
    }

    public static bool IsGameModeSelecting()
    {
        int select = PlayerPrefs.GetInt(SELECT_MODE, 0);
        return (select != 0);
    }

    public static void SelectGameMode()
    {
        PlayerPrefs.SetInt(SELECT_MODE, 1);
    }
}
