using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject DestroyParent = null;
    [SerializeField] private GameObject unitProgressImageParent = null;
    [SerializeField] private Building building = null;
    [SerializeField] private Unit unit = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 1;
    [SerializeField] private int ResourcesNeeded = 500;

    [SerializeField] private float unitSpawnDuration = 5f;
    private Camera mainCamera;
    private BoxCollider unitCollider;
    private RTSPlayer player;
    private GameObject unitPreviewInstance;
    private Renderer unitRendererInstance;
    [SyncVar(hook = nameof(interactableTrue))]
    private int queuedUnits;
    [SyncVar(hook = nameof(Resources))]
    private int a;
    [SyncVar]
    private float unitTimer;
    [SyncVar]
    int resources;
    private float progressImageVelocity;
    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if (priceText == null) { return; }
        unitCollider = unit.GetComponent<BoxCollider>();
    }

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
        if (unitPreviewInstance == null) { return; }

        UpdateBuildingPreview();

    }
    [Server]
    private void unitTimers()
    {
        Button btn = this.gameObject.GetComponent<Button>();

        if (queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;
      
        if (unitTimer > 5)
        {
            btn.interactable = true;
        }

       if (unitTimer < unitSpawnDuration) { return; }

        // Debug.Log($"2 Produce Unit unitSpawnDuration {unitSpawnDuration} , unitTimer {unitTimer}");
        
        queuedUnits--;
        unitTimer = 0f;
        Screen.orientation = ScreenOrientation.Portrait;
      
    }


    private void UpdateTimerDisplay()
    {
        
        float newProgress = unitTimer / unitSpawnDuration;
        
        if (newProgress < unitProgressImage.fillAmount)
        {
            
            unitProgressImage.fillAmount = newProgress;

        }
        else
        {
            RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            unitProgressImageParent.SetActive(true);
            //the yellow effect
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f
            );
            if (unitProgressImage.fillAmount > 0.99)
            {
                unitProgressImageParent.SetActive(false);
                Button btn = this.gameObject.GetComponent<Button>();
                btn.interactable = true;
            }
           

        }

    }


    private void Resources(int olda, int newa)
    {

        
        if (queuedUnits == maxUnitQueue) { return; }

        //RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        Button btn = this.gameObject.GetComponent<Button>();

        //  if (player.GetResources() > ResourcesNeeded) { btn.interactable = true; ; }
        //Debug.Log(false);
        queuedUnits++;
        //hook interactableTrue
        if (player.GetResources() < ResourcesNeeded) { return; }
        player.SetResources(player.GetResources() - ResourcesNeeded);

    }
    public void onBeginDrag(PointerEventData eventData)
    {

        //unitPreviewInstance = Instantiate(unit.GetBuildingPreview());
        //unitRendererInstance = unitPreviewInstance.GetComponentInChildren<Renderer>();
        //testing
        //unitPreviewInstance.SetActive(false);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"End Drag {eventData.position}");

        if (unitPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            a++;
            player.CmdTryPlaceunit(unit.GetId(), hit.point);
        }

        Destroy(unitPreviewInstance);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        //unitPreviewInstance = Instantiate(unit.GetBuildingPreview());
        unitRendererInstance = unitPreviewInstance.GetComponentInChildren<Renderer>();

        unitPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Button btn = this.gameObject.GetComponent<Button>();
        btn.interactable = false;
        a++;
        try { 
            if (unitPreviewInstance == null) { return; }

            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                player.CmdTryPlaceunit(unit.GetId(), hit.point);
            }

            Destroy(unitPreviewInstance);
            Destroy(DestroyParent);
        }
        catch(Exception){}
    }

    private void UpdateBuildingPreview()
    {
        try { 
            Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();

            Ray ray = mainCamera.ScreenPointToRay(pos);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }

            unitPreviewInstance.transform.position = hit.point;

            if (!unitPreviewInstance.activeSelf)
            {
                unitPreviewInstance.SetActive(true);
            }

            Color color = player.CanPlaceBuilding(unitCollider, hit.point) ? Color.green : Color.red;

            unitRendererInstance.material.SetColor("_BaseColor", color);
        }catch (Exception ){}

    }
    private void HandleTouchStart()
    {
        //unitPreviewInstance = Instantiate(unit.GetBuildingPreview());
        unitRendererInstance = unitPreviewInstance.GetComponentInChildren<Renderer>();

        unitPreviewInstance.SetActive(false);

    }
    private void HandleTouchEnd(Vector2 touchPosition)
    {
        if (unitPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceunit(unit.GetId(), hit.point);
        }

        Destroy(unitPreviewInstance);

    }
    private void UpdateTouchBuildingPreview(Vector2 touchPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }

        unitPreviewInstance.transform.position = hit.point;

        if (!unitPreviewInstance.activeSelf)
        {
            unitPreviewInstance.SetActive(true);
        }

        Color color = player.CanPlaceBuilding(unitCollider, hit.point) ? Color.green : Color.red;
        unitRendererInstance.material.SetColor("_BaseColor", color);
    }
    private void interactableTrue(int oldUnits, int newUnits)
    {
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        //Debug.Log(9);
        remainingUnitsText.text = newUnits.ToString();
        //print the number
        Button btn = this.gameObject.GetComponent<Button>();


       // if (player.GetResources() < ResourcesNeeded) { btn.interactable = false; ; }
        //botton can touch now
        //  Debug.Log(queuedUnits);
    }
}
