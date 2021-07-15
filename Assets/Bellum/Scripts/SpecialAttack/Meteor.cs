using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Meteor : MonoBehaviour,ISpecialAttack, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] GameObject meteorPrefab;
    [SerializeField] GameObject dragcCirclePrefab;
    private PlayerGround playerGround;
    private GameObject dragCircle;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("FightGround");
        foreach (GameObject ground in grounds)
        {
            if (ground.TryGetComponent(out PlayerGround pg))
            {
                playerGround = pg;
                break;
            }
        }
    }
    public void OnPointerDown()
    {

    }
    public int GetSpCost()
    {
        return 0;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragCircle = Instantiate(dragcCirclePrefab);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }
        dragCircle.transform.position = hit.point;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }
        Instantiate(meteorPrefab).transform.position = hit.point;
        Destroy(dragCircle);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
