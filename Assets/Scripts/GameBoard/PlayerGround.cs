using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [SerializeField] private GameObject playerHalf;
    [SerializeField] private GameObject enemyHalf;
    private Transform[] childTransform;
    private RTSPlayer player;
    // Start is called before the first frame update
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
       // sortLayer();
    }
    public void sortLayer(int playerID)
    {
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
           
            playerHalf.layer = LayerMask.NameToLayer("Floor");
            childTransform = playerHalf.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Floor");
            }
        }
        else // Multi player seneriao
        {
            if(playerID == 0)
            {
                Debug.Log($"Id - {playerID}");
                enemyHalf.layer = LayerMask.NameToLayer("Floor");
                childTransform = enemyHalf.GetComponentsInChildren<Transform>();
                foreach (Transform child in childTransform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Floor");
                }
            }
            else
            {
               Debug.Log($"Id - {playerID}");
                playerHalf.layer = LayerMask.NameToLayer("Floor");
               childTransform = playerHalf.GetComponentsInChildren<Transform>();
                foreach (Transform child in childTransform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Floor");
                }
                
            }
           
        }
    }
    public void resetLayer()
    {
        foreach (Transform child in childTransform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
