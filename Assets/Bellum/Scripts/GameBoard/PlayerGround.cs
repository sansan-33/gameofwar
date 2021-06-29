using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [SerializeField] GameObject meshParent;
    [SerializeField] GameObject playerMesh;
    [SerializeField] GameObject enemyMesh;
    [SerializeField] GameObject playerMeshAll;
    [SerializeField] GameObject enemyMeshAll;
    [SerializeField] GameObject playerMeshLeft;
    [SerializeField] GameObject enemyMeshLeft;
    [SerializeField] GameObject playerMeshCentre;
    [SerializeField] GameObject enemyMeshCentre;
    [SerializeField] GameObject playerMeshRight;
    [SerializeField] GameObject enemyMeshRight;
    [SerializeField] GameObject playerMeshLeftRight;
    [SerializeField] GameObject enemyMeshLeftRght;
    [SerializeField] GameObject playerMeshLeftCentre;
    [SerializeField] GameObject enemyMeshLeftCentre;
    [SerializeField] GameObject playerMeshRightCentre;
    [SerializeField] GameObject enemyMeshRightCentre;
    [SerializeField] List<GameObject> layers;
    [SerializeField] GameObject enemyBack;
    [SerializeField] GameObject enemyLeft;
    [SerializeField] GameObject enemyCentre;
    [SerializeField] GameObject enemyRight;
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
        TacticalBehavior.UnitTagUpdated += SubscribeEvent;
    }
    private void SubscribeEvent()
    {
        GreatWallController.GateOpened += GateOpend;
    }
    private void OnDestroy()
    {
        TacticalBehavior.UnitTagUpdated -= SubscribeEvent;
        GreatWallController.GateOpened -= GateOpend;
    }
    private void GateOpend(string playerId, string doorIndex)
    {
        Debug.Log($"playerId{playerId} player.GetPlayerID().ToString() {player.GetPlayerID().ToString()} playerId == player.GetPlayerID().ToString() {playerId == player.GetPlayerID().ToString()}");
        if (playerId == player.GetPlayerID().ToString())
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
            enemyBack.layer = LayerMask.NameToLayer("Floor");
            enemyCentre.layer = LayerMask.NameToLayer("Floor");
            enemyLeft.layer = LayerMask.NameToLayer("Floor");
            enemyRight.layer = LayerMask.NameToLayer("Floor");
            //enenmyBack.SetActive(true);
            //enenmyCentre.SetActive(true);
            // enenmyLeft.SetActive(true);
            // enenmyRight.SetActive(true);
            // Debug.Log($"breakLeftWall{breakLeftWall}breakCentreWall{breakCentreWall}breakRightWall{breakRightWall}");\
           // Debug.Log(playerMeshAll);
            if (playerMeshAll != null)
            {
                if (breakLeftWall == true)
                {
                    if (breakCentreWall == true)
                    {
                        if (breakRightWall == true)
                        {
                            playerMeshAll.SetActive(true);
                        }
                        else
                        {
                            playerMeshLeftCentre.SetActive(true);
                        }
                    }
                    else
                    {
                        playerMeshLeft.SetActive(true);
                    }
                }
                else if (breakCentreWall == true)
                {
                    if (breakRightWall == true)
                    {
                        playerMeshRightCentre.SetActive(true);
                    }
                    else
                    {
                        playerMeshCentre.SetActive(true);
                    }
                }
                else if (breakRightWall == true)
                {
                    playerMeshRight.SetActive(true);
                }
                else
                {
                    //Debug.Log("playerMesh");
                    enemyMesh.SetActive(true);
                }
            }
            
            if (breakLeftWall == true)
            {
                playerLeft.layer = LayerMask.NameToLayer("Floor");
                //playerLeft.SetActive(true);    
            }
            if (breakCentreWall == true)
            {
                playerCentre.layer = LayerMask.NameToLayer("Floor");
                // playerCentre.SetActive(true);
            }
            if (breakRightWall == true)
            {
                playerRight.layer = LayerMask.NameToLayer("Floor");
                // playerRight.SetActive(true);
            }

            
        }
        else // Multi player seneriao
        {
            Debug.Log($"PLAYER ID {playerID}");
            if (playerID == 1)
            {

                playerBack.layer = LayerMask.NameToLayer("Floor");
                playerCentre.layer = LayerMask.NameToLayer("Floor");
                playerLeft.layer = LayerMask.NameToLayer("Floor");
                playerRight.layer = LayerMask.NameToLayer("Floor");
                if (breakLeftWall == true)
                {
                    enemyLeft.layer = LayerMask.NameToLayer("Floor");
                    //playerLeft.SetActive(true);    
                }
                if (breakCentreWall == true)
                {
                    enemyCentre.layer = LayerMask.NameToLayer("Floor");
                    // playerCentre.SetActive(true);
                }
                if (breakRightWall == true)
                {
                    enemyRight.layer = LayerMask.NameToLayer("Floor");
                    // playerRight.SetActive(true);
                }
                if (playerMeshAll != null)
                {
                    if (breakLeftWall == true)
                    {
                        if (breakCentreWall == true)
                        {
                            if (breakRightWall == true)
                            {
                                enemyMeshAll.SetActive(true);
                            }
                            else
                            {
                                enemyMeshLeftCentre.SetActive(true);
                            }
                        }
                        else
                        {
                            enemyMeshLeft.SetActive(true);
                        }
                    }
                    else if (breakCentreWall == true)
                    {
                        if (breakRightWall == true)
                        {
                            enemyMeshRightCentre.SetActive(true);
                        }
                        else
                        {
                            enemyMeshCentre.SetActive(true);
                        }
                    }
                    else if (breakRightWall == true)
                    {
                        enemyMeshRight.SetActive(true);
                    }
                    else
                    {
                        //Debug.Log("playerMesh");
                        playerMesh.SetActive(true);
                    }
                }
            }
            else
            {
                enemyBack.layer = LayerMask.NameToLayer("Floor");
                enemyCentre.layer = LayerMask.NameToLayer("Floor");
                enemyLeft.layer = LayerMask.NameToLayer("Floor");
                enemyRight.layer = LayerMask.NameToLayer("Floor");
                 Debug.Log($"breakLeftWall{breakLeftWall}breakCentreWall{breakCentreWall}breakRightWall{breakRightWall}");
                if (breakLeftWall == true)
                {
                    playerLeft.layer = LayerMask.NameToLayer("Floor");
                    //playerLeft.SetActive(true);    
                }
                if (breakCentreWall == true)
                {
                    playerCentre.layer = LayerMask.NameToLayer("Floor");
                    // playerCentre.SetActive(true);
                }
                if (breakRightWall == true)
                {
                    playerRight.layer = LayerMask.NameToLayer("Floor");
                    // playerRight.SetActive(true);
                }
                if (playerMeshAll != null)
                {
                    if (breakLeftWall == true)
                    {
                        if (breakCentreWall == true)
                        {
                            if (breakRightWall == true)
                            {
                                playerMeshAll.SetActive(true);
                            }
                            else
                            {
                                playerMeshLeftCentre.SetActive(true);
                            }
                        }
                        else
                        {
                            playerMeshLeft.SetActive(true);
                        }
                    }
                    else if (breakCentreWall == true)
                    {
                        if (breakRightWall == true)
                        {
                            playerMeshRightCentre.SetActive(true);
                        }
                        else
                        {
                            playerMeshCentre.SetActive(true);
                        }
                    }
                    else if (breakRightWall == true)
                    {
                        playerMeshRight.SetActive(true);
                    }
                    else
                    {
                        //Debug.Log("playerMesh");
                        enemyMesh.SetActive(true);
                    }

                }
            }
        }
    }
   
    public void resetLayer()
    {
        if(meshParent!= null)
        {
            childTransform = meshParent.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransform)
            {
                if (child.gameObject != meshParent)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        foreach (GameObject child in layers)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
      playerMesh.SetActive(false);
        enemyMesh.SetActive(false);
    }
     
}
