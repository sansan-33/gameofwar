using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EffectStatus : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleAttackEffectUpdated))]
    public float attack = 0f;
    [SyncVar(hook = nameof(HandleDefenseEffectUpdated))]
    public float defense = 0f;
    [SyncVar(hook = nameof(HandleHealthEffectUpdated))]
    public float health = 0f;
    [SyncVar(hook = nameof(HandleSpeedEffectUpdated))]
    public float speed = 0f;
    [SyncVar(hook = nameof(HandleFreezeEffectUpdated))]
    public bool isFreeze = false;
    [SyncVar(hook = nameof(HandleStunEffectUpdated))]
    public bool isStun = false;

    [SerializeField] private GameObject slowPrefab;
    [SerializeField] private GameObject speedPrefab;

    public int freezeKey;
    int playerID = 0;
    public static event Action<int, UnitMeta.EffectType, float> ClientOnEffectUpdated;
    private Dictionary<UnitMeta.EffectType, bool> effectStateMachine = new Dictionary<UnitMeta.EffectType, bool>();
    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        playerID = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID();
    }

    //==================================== Set Tag For DIE
    public void SetEffect(string tag, float value)
    {
        if (isServer)
            RpcEffect(tag, value);
        else
            CmdEffect(tag, value);
    }
    [Command]
    public void CmdEffect(string tag, float value)
    {
        ServerEffect(tag, value);
    }
    [ClientRpc]
    public void RpcEffect(string tag, float value)
    {
        HandleEffect(tag, value);
    }
    [Server]
    public void ServerEffect(string tag, float value)
    {
        HandleEffect(tag, value);
    }
    private void HandleEffect(string tag, float value)
    {
        Enum.TryParse(tag, out UnitMeta.EffectType myEffect);
        switch (myEffect)
        {
            case UnitMeta.EffectType.ATTACK:
                attack = attack + value;
                gameObject.GetComponent<IAttack>().ScaleDamageDeal(0, 0, value); // 0 means current value
                break;
            case UnitMeta.EffectType.DEFENSE:
                defense = defense + value;
                break;
            case UnitMeta.EffectType.HEALTH:
                health = health + value;
                gameObject.GetComponent<Health>().ScaleMaxHealth( (int) gameObject.GetComponent<Health>().getCurrentHealth() , value);
                break;
            case UnitMeta.EffectType.SPEED:
                speed = speed + value;
                gameObject.GetComponent<UnitPowerUp>().SetSpeed(speed, true);
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
    private void HandleAttackEffectUpdated(float oldValue, float newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.ATTACK, newValue);
    }
    private void HandleDefenseEffectUpdated(float oldValue, float newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.DEFENSE, newValue);
    }
    private void HandleHealthEffectUpdated(float oldValue, float newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.HEALTH, newValue);
    }
    private void HandleSpeedEffectUpdated(float oldValue, float newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.SPEED, newValue);
        if(newValue > oldValue)
        {
            Instantiate(speedPrefab, transform);
        }
        else
        {
            Instantiate(slowPrefab, transform);
        }

        
    }
    private void HandleFreezeEffectUpdated(bool oldValue, bool newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.FREEZE, newValue ? 1f :0f);
    }
    private void HandleStunEffectUpdated(bool oldValue, bool newValue)
    {
        if (tag.ToLower().Contains("king"))
            ClientOnEffectUpdated?.Invoke(playerID, UnitMeta.EffectType.STUN, newValue ? 1f : 0f);
    }
    public void UnFrezze()
    {
        //Debug.Log("Unfrezze");
        isFreeze = false;
        ImpactSmash impactSmash = null;
        ImpactSmash[] impactSmashs = FindObjectsOfType<ImpactSmash>();
        foreach (ImpactSmash _impactSmash in impactSmashs)
        {
            if (_impactSmash.GetSpecialAttackType() == SpecialAttackDict.SpecialAttackType.FREEZE)
            {
                impactSmash = _impactSmash;
            }
        }
        impactSmash.UnitRepeatAttackDelaykeys.TryGetValue(freezeKey, out float RAD);
        impactSmash.UnitSpeedkeys.TryGetValue(freezeKey, out float speed);
        impactSmash.UnitMaterial.TryGetValue(freezeKey, out Material material);
        //Debug.Log($"RAD = {RAD}, speed = {speed}, material = {material}, key = {freezeKey}");
        GetComponentInChildren<SkinnedMeshRenderer>().material = material;
        GetComponent<UnitPowerUp>().SpecialEffect(RAD, speed);

    }
}
