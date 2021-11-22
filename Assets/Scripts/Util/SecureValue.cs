using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

[System.Serializable]
public delegate void ValueCallback<T>(T value);
public delegate void Callback();

public class ENG_Encryption
{
    byte[] Skey = new byte[8];

    public ENG_Encryption(string strKey)
    {
        Skey = ASCIIEncoding.ASCII.GetBytes(strKey);
    }

    public string Encrypt(string p_data)
    {
        if (Skey.Length != 8)
        {
            throw (new Exception("Invalid key. Key length must be 8 byte."));
        }

        DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();
        rc2.Key = Skey;
        rc2.IV = Skey;

        MemoryStream ms = new MemoryStream();
        CryptoStream cryStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] data = Encoding.UTF8.GetBytes(p_data.ToCharArray());

        cryStream.Write(data, 0, data.Length);
        cryStream.FlushFinalBlock();
        cryStream.Close();
        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string p_data)
    {
        DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();
        rc2.Key = Skey;
        rc2.IV = Skey;

        MemoryStream ms = new MemoryStream();
        CryptoStream cryStream = new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write);
        byte[] data = Convert.FromBase64String(p_data);

        cryStream.Write(data, 0, data.Length);
        cryStream.FlushFinalBlock();
        cryStream.Close();

        int buffercnt = 1;
        for (; buffercnt < ms.GetBuffer().Length; ++buffercnt)
        {
            if (ms.GetBuffer()[buffercnt] == 0)
                break;
        }

        String resultstr = Encoding.UTF8.GetString(ms.GetBuffer(), 0, buffercnt);
        return resultstr;
    }
}

public class DelegateSecrueProperty<T>
{
    static int m_secrueCount = 0;

    public T propertyValue;
    public ValueCallback<T> callbacks = null;
    private ENG_Encryption secVal = null;
    private string ENG_KEY;
    private string m_engValue;

    public void InitSecrue()
    {
        if (secVal != null)
            return;

        ENG_KEY = string.Format("{0:D8}", ++m_secrueCount);
        secVal = new ENG_Encryption(ENG_KEY);
        Value = propertyValue;
    }

    public T Value
    {
        get
        {
            if (secVal != null)
            {
                return (T)Convert.ChangeType(secVal.Decrypt(m_engValue), typeof(T));
            }
            else
            {
                return propertyValue;
            }
        }
        set
        {
            //var val = Convert.ToInt32(value);
            if (secVal != null)
            {
                m_engValue = secVal.Encrypt(value.ToString());
            }
            else
            {
                propertyValue = value;
            }
            if (null != callbacks)
            {
                callbacks(value);
            }
        }
    }

    public void AddCallback(ValueCallback<T> callback)
    {
        if (null == callbacks)
            callbacks = callback;
        else
            callbacks += callback;

        callbacks(Value);
    }

    public void RemoveCallback(ValueCallback<T> callback)
    {
        if (null == callbacks)
        {
            return;
        }

        callbacks -= callback;
    }
}
