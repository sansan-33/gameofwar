using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [SerializeField] GameObject playerMesh;
    [SerializeField] GameObject enemyMesh;
    [SerializeField] private GameObject enemyHalf;
    [SerializeField] private GameObject playerHalf;
    private Transform[] childTransform;
    private GameObject cosedMesh;
    //private RTSPlayer player;
    // Start is called before the first frame update
    void Start()
    {
        //Test
        //player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
       // sortLayer();
    }
    public void sortLayer(int playerID)
    {
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {

            enemyHalf.layer = LayerMask.NameToLayer("Floor");
            childTransform = enemyHalf.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransform)
            {
                
                child.gameObject.layer = LayerMask.NameToLayer("Floor");
            }
            playerMesh.SetActive(true);
            //Debug.Log($"{playerMesh.activeSelf}");
        }
        else // Multi player seneriao
        {
            if (playerID == 0)
            {

                enemyHalf.layer = LayerMask.NameToLayer("Floor");
                childTransform = enemyHalf.GetComponentsInChildren<Transform>();
                foreach (Transform child in childTransform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Floor");
                   
                }
                playerMesh.SetActive(true);
            }
            else
            {
                playerHalf.layer = LayerMask.NameToLayer("Floor");
                childTransform = playerHalf.GetComponentsInChildren<Transform>();
                foreach (Transform child in childTransform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Floor");
                    
                }
                enemyMesh.SetActive(true);

            }
           
        }
    }
   
    public void resetLayer()
    {
        foreach (Transform child in childTransform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        //cosedMesh = enemyMesh == true ? playerMesh : enemyMesh;
        playerMesh.SetActive(false);
        enemyMesh.SetActive(false);
        //cosedMesh.SetActive(false);
        //Debug.Log($"{cosedMesh} is active -->{cosedMesh.activeSelf}");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
