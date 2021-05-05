using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CombatEffectController : NetworkBehaviour
{
    private DamageTextObjectPool damageTextObjectPool;
    private void Start()
    {
        damageTextObjectPool = FindObjectOfType<DamageTextObjectPool>();
    }
    public void damageText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {
        //HandleDamageText(targetPos, damageNew, damgeOld, opponentIdentity, flipText);
        if (isServer)
            RpcCommandText(targetPos, damageNew, damgeOld, opponentIdentity, flipText);
        else
            CmdDamageText(targetPos, damageNew, damgeOld, opponentIdentity, flipText);
    }
    [Command]
    public void CmdDamageText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {
        ServerDamageText(targetPos, damageNew, damgeOld, opponentIdentity, flipText);
    }
    [Server]
    public void ServerDamageText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {
        HandleDamageText(targetPos, damageNew, damgeOld, opponentIdentity, flipText);
    }
    [TargetRpc]
    public void TargetCommandText(NetworkConnection other, GameObject floatingText, NetworkIdentity others)
    {
        floatingText.GetComponent<DamageTextHolder>().displayRotation.y = 180;
    }
    [ClientRpc]
    public void RpcCommandText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {
        HandleDamageText(targetPos, damageNew, damgeOld, opponentIdentity, flipText);
    }
    private void HandleDamageText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {
        targetPos.x = targetPos.x + 10;
        targetPos.y = targetPos.y + 5;
        GameObject floatingText = SetupDamageText(targetPos, damageNew, damgeOld);
        NetworkServer.Spawn(floatingText, connectionToClient);

        if (opponentIdentity == null) { return; }

        if (flipText) { TargetCommandText(opponentIdentity.connectionToClient, floatingText, opponentIdentity); }
    }
    private GameObject SetupDamageText(Vector3 targetPos, float damageToDeals, float damageToDealOriginal)
    {
        GameObject floatingText = damageTextObjectPool.GetObject();
        //GameObject floatingText = Instantiate(textPrefab, targetPos, Quaternion.identity);
        floatingText.transform.position = targetPos;
        floatingText.transform.rotation = Quaternion.identity;
        Color textColor;
        string dmgText;
        if (damageToDeals > damageToDealOriginal)
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().CriticalColor;
            dmgText = damageToDeals + " Critical";
        }
        else
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().NormalColor;
            dmgText = damageToDeals + "";
        }
        floatingText.GetComponent<DamageTextHolder>().displayColor = textColor;
        floatingText.GetComponent<DamageTextHolder>().displayText = dmgText;
        return floatingText;
    }
    

}
