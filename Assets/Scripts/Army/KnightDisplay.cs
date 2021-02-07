using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text NumberOfKnight = null;
    private RTSPlayer player;

    private void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }
    private void Update()
    {
          ClientHandlePlayerUpdated();
        
    }
    
    private void ClientHandlePlayerUpdated()
    {
     
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int Totalplayers = players.Length;
        int Totalenemies = enemies.Length;
        NumberOfKnight.text = $"{Totalenemies} VS {Totalplayers}";
        Debug.Log(Totalenemies);
        Debug.Log(Totalplayers);
    }
}
