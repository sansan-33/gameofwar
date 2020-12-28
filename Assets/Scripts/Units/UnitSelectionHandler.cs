using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;

    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Vector2 startPosition;

    private RTSPlayer player;
    private Camera mainCamera;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        Input.simulateMouseWithTouches = true;
        mainCamera = Camera.main;

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;      
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {


        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
                StartSelectionArea();
            else if (Input.GetTouch(0).phase == UnityEngine.TouchPhase.Ended)
                ClearSelectionArea();
        }
        try{
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartSelectionArea();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ClearSelectionArea();
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                UpdateSelectionArea();
            }
        }catch (Exception)
        {

        }
       
    }

    private void StartSelectionArea()
    {
        //Debug.Log($"1 Start Selection Area is card  tap {isCardTap()}");
        
        if (!Keyboard.current.leftShiftKey.isPressed && !isSelectedDoubleTap() &&  !isCardTap() )
        {
            //Debug.Log($"SelectedUnits {SelectedUnits.Count} , is selected double ? {isSelectedDoubleTap()} , is Card Tap {isCardTap()} ");
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();
            //Debug.Log($"All Clear Selected Units {SelectedUnits.Count}");
        }

        unitSelectionArea.gameObject.SetActive(true);
        
        startPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();  

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue(); ;

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition +
            new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {

        //Debug.Log($"ClearSelectionArea Unit Touch Phase ended");
        unitSelectionArea.gameObject.SetActive(false);
        
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Vector2 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue(); ;

            Ray ray = mainCamera.ScreenPointToRay(pos);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            if (!unit.hasAuthority) { return; }

            //Debug.Log($"Add Selected Area Unit {unit}");
            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }
        
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) { continue; }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x &&
                screenPosition.x < max.x &&
                screenPosition.y > min.y &&
                screenPosition.y < max.y)
            {
                //Debug.Log($"Add Selected My Unit {unit}");
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }

        //Debug.Log($"SelectedUnits {SelectedUnits.Count}");


    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

    private bool isSelectedDoubleTap()
    {
        //Debug.Log($"isSelectedDoubleTap {SelectedUnits.Count}");
        bool selectedDoubleTap = false;
        for (var i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began)
            {
                if (Input.GetTouch(i).tapCount == 2 || SelectedUnits.Count > 0)
                {
                    //Debug.Log("Double Tap");
                    selectedDoubleTap = true;
                    break;
                }
            }
        }
        return selectedDoubleTap;
    }
    private bool isDoubleTap()
    {
        bool doubleTap = false;
        for (var i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began)
            {
                if (Input.GetTouch(i).tapCount == 2)
                {
                    //Debug.Log("Double Tap");
                    doubleTap = true;
                    break;
                }
            }
        }
        return doubleTap;
    }
    private bool isCardTap()
    {
        bool cardTap = false;
        Vector2 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue(); ;

        Ray ray = mainCamera.ScreenPointToRay(pos);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { cardTap = true; }
        //Debug.Log($"Is card ? {cardTap}");
        //Debug.Log($"Is card hit {hit} / button {hit.collider.GetComponent<Button>()}" );
        //if (hit.collider.TryGetComponent<Button>(out Button btn)) { cardTap = true; }

        return cardTap;
    }
}
