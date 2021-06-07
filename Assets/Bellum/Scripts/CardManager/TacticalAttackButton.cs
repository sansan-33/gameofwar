using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class TacticalAttackButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] public TacticalBehavior.TaticalAttack type;
    [SerializeField] public TacticalBehavior tacticalBehavior = null;
    RTSPlayer player;
    public void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        tacticalBehavior.taticalAttack(type, player.GetPlayerID());
    }
}
