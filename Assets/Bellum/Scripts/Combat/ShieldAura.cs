using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldAura : MonoBehaviour
{
    [SerializeField] private GameObject shieldParent;
    public void aura()
    {
        int shieldHealths = 300;
        shieldParent.SetActive(true);
        Shield shield = GetComponentInParent<Shield>();
        Debug.Log($"Shield Aura === > shield is null {shield} ");
        shield.CmdSetShieldHealth(shieldHealths);
    }

}
