using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CardCounter : NetworkBehaviour, IPointerClickHandler
{
    
    [SerializeField] private Transform unitSpawnPoint = null;
    //[SerializeField] private Unit unitPrefab = null;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 1;
    [SerializeField] private int ResourcesNeeded = 500;

    [SerializeField] private float unitSpawnDuration = 5f;
   
    [SyncVar(hook = nameof(interactableTrue))]
    private int queuedUnits;
    [SyncVar(hook = nameof(Resources))]
    private int a;
    [SyncVar]
    private float unitTimer;
    [SyncVar]
    int resources;
    private float progressImageVelocity;
  
    private void Update()
    {

        if (isServer)
        {
            unitTimers();
           
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
       
    }
   
    [Server]
    private void unitTimers()
    {
        Button btn = this.gameObject.GetComponent<Button>();
        

        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();


        

       
        if (queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;
        if (player.GetResources() < ResourcesNeeded)
        {

            GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

            foreach (GameObject card in cards)
            {
                card.GetComponent<Button>().interactable = false; Debug.Log(9);
               
            }
        }
        if (player.GetResources() < ResourcesNeeded)
        {

            Debug.Log(8);
        }
        else
        {


            if (unitTimer > 5)
            {


                btn.interactable = true;
               

            }
        }
    
        if (btn.interactable == false)
        {


            return;
        }

       

        if (player.GetResources() > ResourcesNeeded )
        {
           
            GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

            foreach (GameObject card in cards)
            {
               card.GetComponent<Button>().interactable = true; ;
                
            }
        }
        
        if (unitTimer > 5)
        {
           
           
            btn.interactable = true;
        }

        if (unitTimer < unitSpawnDuration) { return; }

        queuedUnits--;
        unitTimer = 0f;
        Screen.orientation = ScreenOrientation.Portrait;
        float newProgress = unitTimer / unitSpawnDuration;

      //  Debug.Log($"queuedUnits{queuedUnits}");


    }


    private void UpdateTimerDisplay()
    {
        //unitProgressImage.fillAmount = 0.5f;
       
        //unitTimer = 0f;
        float newProgress = unitTimer / unitSpawnDuration;
       
        if (newProgress < unitProgressImage.fillAmount)
        {
                unitProgressImage.fillAmount = newProgress;
           
        }
        else
        {
            RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
           
            //the yellow effect
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f

            ); 

        }

    }

    
    private void Resources(int olda, int newa)
    {

        Debug.Log($"connectionToClient {connectionToClient}");
         if (queuedUnits == maxUnitQueue) { return; }

        //RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        Button btn = this.gameObject.GetComponent<Button>();
       
      //  if (player.GetResources() > ResourcesNeeded) { btn.interactable = true; ; }
        //Debug.Log(false);
        queuedUnits++;
        //hook interactableTrue
        if(player.GetResources() < ResourcesNeeded) { return; }
        player.SetResources(player.GetResources() - ResourcesNeeded);
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
       
        // Debug.Log($"Card Counter OnPointerClick {eventData}");

        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        Button btn = this.gameObject.GetComponent<Button>();
        btn.interactable = false;
       
                //botton can not touch
        a++;
        //hook Resources
    }

    private void interactableTrue(int oldUnits, int newUnits)
    {RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
      
        //Debug.Log(9);
        remainingUnitsText.text = newUnits.ToString();
        //print the number
        Button btn = this.gameObject.GetComponent<Button>();
        
        
        if (player.GetResources() < ResourcesNeeded) { btn.interactable = false; ; }
        //botton can touch now
        //  Debug.Log(queuedUnits);
    }

    
}
/*1 Pointerclick
  2 interactable=true
  3 unitqueue++
  4 click queueunit Update
  5 Check if Unit Timer >1 btn = true
  6 Update produce queueUnit -- */
