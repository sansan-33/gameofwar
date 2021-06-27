using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [SerializeField] GameObject playerMesh;
    [SerializeField] GameObject enemyMesh;
    [SerializeField] List<GameObject> layers;
    [SerializeField] GameObject enenmyBack;
    [SerializeField] GameObject enenmyLeft;
    [SerializeField] GameObject enenmyCentre;
    [SerializeField] GameObject enenmyRight;
    [SerializeField] GameObject playerBack;
    [SerializeField] GameObject playerLeft;
    [SerializeField] GameObject playerCentre;
    [SerializeField] GameObject playerRight;
    [SerializeField] private GameObject enemyHalf;
    [SerializeField] private GameObject playerHalf;
    private bool breakLeftWall = false;
    private bool breakCentreWall = false;
    private bool breakRightWall = false;
    private Transform[] childTransform;
    private RTSPlayer player;
    // Start is called before the first frame update
    public void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        CardDealer.FinishDealEnemyCard += SubscribeEvent;
    }
    private void SubscribeEvent()
    {
        GreatWallController.GateOpened += GateOpend;
    }
    private void GateOpend(string playerId, string doorIndex)
    {
        if(playerId == player.GetPlayerID().ToString())
        {
            switch (doorIndex)
            {
                case "0":
                    breakLeftWall = true;
                    Debug.Log("Break Left");
                    break;
                case "1":
                    breakCentreWall = true;
                    Debug.Log("Break Centre");
                    break;
                case "2":
                    breakRightWall = true;
                    Debug.Log("Break Right");
                    break;
            }
        }
    }
    public void sortLayer(int playerID)
    {
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {

            /*enemyHalf.layer = LayerMask.NameToLayer("Floor");
            childTransform = enemyHalf.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransform)
            {
                
                child.gameObject.layer = LayerMask.NameToLayer("Floor");
            }*/
            enenmyBack.layer = LayerMask.NameToLayer("Floor");
            enenmyCentre.layer = LayerMask.NameToLayer("Floor");
            enenmyLeft.layer = LayerMask.NameToLayer("Floor");
            enenmyRight.layer = LayerMask.NameToLayer("Floor");
            //enenmyBack.SetActive(true);
            //enenmyCentre.SetActive(true);
            // enenmyLeft.SetActive(true);
            // enenmyRight.SetActive(true);
            // Debug.Log($"breakLeftWall{breakLeftWall}breakCentreWall{breakCentreWall}breakRightWall{breakRightWall}");
            if (breakLeftWall == true) { playerLeft.layer = LayerMask.NameToLayer("Floor"); }//playerLeft.SetActive(true); }
            if (breakCentreWall == true) { playerCentre.layer = LayerMask.NameToLayer("Floor"); } // playerCentre.SetActive(true); }
            if (breakRightWall == true) { playerRight.layer = LayerMask.NameToLayer("Floor"); }// playerRight.SetActive(true); }

            playerMesh.SetActive(true);
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
        /* foreach (Transform child in childTransform)
         {
             child.gameObject.layer = LayerMask.NameToLayer("Default");
         }*/
        foreach (GameObject child in layers)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
      playerMesh.SetActive(false);
        enemyMesh.SetActive(false);
    }
     
}
