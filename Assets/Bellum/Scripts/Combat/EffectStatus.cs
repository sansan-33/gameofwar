using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EffectStatus : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleAttackEffectUpdated))]
    public int attack;
    [SyncVar(hook = nameof(HandleDefenseEffectUpdated))]
    public int defense;
    [SyncVar(hook = nameof(HandleHealthEffectUpdated))]
    public int health;
    [SyncVar(hook = nameof(HandleSpeedEffectUpdated))]
    public int speed;
    [SyncVar(hook = nameof(HandleFreezeEffectUpdated))]
    public bool isFreeze;
    [SyncVar(hook = nameof(HandleStunEffectUpdated))]
    public bool isStun;

    int playerID = 0;
    public static event Action<int, UnitMeta.EffectType,int> ClientOnEffectUpdated;
    private Dictionary<UnitMeta.EffectType, bool> effectStateMachine = new Dictionary<UnitMeta.EffectType, bool>();
    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        playerID = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID();
    }

    //==================================== Set Tag For DIE
    public void SetEffect(string tag, int value)
    {
        if (isServer)
            RpcEffect(tag, value);
        else
            CmdEffect(tag, value);
    }
    [Command]
    public void CmdEffect(string tag, int value)
    {
        ServerEffect(tag, value);
    }
    [ClientRpc]
    public void RpcEffect(string tag, int value)
    {
        HandleEffect(tag, value);
    }
    [Server]
    public void ServerEffect(string tag, int value)
    {
        HandleEffect(tag, value);
    }
    private void HandleEffect(string tag, int value)
    {
        Enum.TryParse(tag, out UnitMeta.EffectType myEffect);
        switch (myEffect)
        {
            case UnitMeta.EffectType.ATTACK:
                attack = attack + value;
                break;
            case UnitMeta.EffectType.DEFENSE:
                defense = defense + value;
                break;
            case UnitMeta.EffectType.HEALTH:
                health = health + value;
                break;
            case UnitMeta.EffectType.SPEED:
                speed = speed + value;
                break;
            case UnitMeta.EffectType.FREEZE:
                isFreeze = value > 0; 
                break;
            case UnitMeta.EffectType.STUN:
                isStun = value > 0;
                break;
            default:
                break;
        }
    }
    private void HandleAttackEffectUpdated(int oldValue, int newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.ATTACK, newValue);
    }
    private void HandleDefenseEffectUpdated(int oldValue, int newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.DEFENSE, newValue);
    }
    private void HandleHealthEffectUpdated(int oldValue, int newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.HEALTH, newValue);
    }
    private void HandleSpeedEffectUpdated(int oldValue, int newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.FREEZE, newValue);
    }
    private void HandleFreezeEffectUpdated(bool oldValue, bool newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.FREEZE, newValue ? 1 :0);
    }
    private void HandleStunEffectUpdated(bool oldValue, bool newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.FREEZE, newValue ? 1 : 0);
    }
}
