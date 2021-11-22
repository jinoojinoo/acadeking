using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public enum Bullet_Kind
{
    Damage_Bullet,
    Damage_Sword,
}

public class NetworkWeaponManager : NetworkBehaviour
{
    private static NetworkWeaponManager m_instance = null;
    public static NetworkWeaponManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    public static bool IsCreateInstance()
    {
        return m_instance != null;
    }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
        {
            m_instance = null;
        }
    }

    protected Vector3 m_fireDir;
    protected Vector3 m_firePos;
    public void FireVisualClient(uint playerid, Bullet_Kind kind, Vector3 pos, Vector3 normal)
    {

    }

    /*    public void DoLogicalExplosion(Damage_Base weaponbase, Collider2D collision)
        {
            if (string.Compare(collision.tag, "Body") != 0)
                return;

            Character_Base character = collision.GetComponentInParent<Character_Base>();
            if (character == null || weaponbase.PlayerID == character.MyNetworkPlayer.LobbyNetID)
                return;

            ProgressDamage(character, weaponbase.Damage);
            Destroy(weaponbase.MyObject);
        }*/
}
