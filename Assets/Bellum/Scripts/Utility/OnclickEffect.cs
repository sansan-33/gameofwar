using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OnclickEffect : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update
    [SerializeField] GameObject effect;
    [SerializeField] Transform parent;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    GameObject _effect;
    Camera mainCamera;
    bool a = false;
    void Start()
    {
        mainCamera = Camera.main;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("ON click");
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Debug.Log($"spawn pos{pos}");
        _effect = Instantiate(effect, parent);
        _effect.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleCenter);
        Ray ray = mainCamera.ScreenPointToRay(pos);
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
        _effect.transform.position = hit.point;
        _effect.GetComponent<ParticleSystem>().Play();
        Debug.Log($"Hit {hit.point}");
        StartCoroutine(DisableEffect(_effect));
        a = true;
    }
    public void OnPointerClick()
    {
        Debug.Log("ON click");
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Debug.Log($"spawn pos{pos}");
        _effect = Instantiate(effect, parent);
        //_effect.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleCenter);
        //Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(pos);
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask);
        _effect.transform.position = hit.point;
        Debug.Log($"Hit {hit.point}");
        _effect.GetComponent<ParticleSystem>().Play();
        
        StartCoroutine(DisableEffect(_effect));
    }
    private IEnumerator DisableEffect(GameObject effect)
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(effect);
    }
    // Update is called once per frame
    void Update()
    {
        /*if(a == true)
        {
            Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
            Debug.Log($"spawn pos{pos}");
            _effect = Instantiate(effect, parent);
            _effect.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleCenter);
            Ray ray = mainCamera.ScreenPointToRay(pos);
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
            {
                Debug.Log(hit.transform);
                Debug.Log(hit.transform.gameObject.layer);
                return;
            }
            _effect.transform.position = hit.point;
            _effect.GetComponent<ParticleSystem>().Play();
        }*/
        

    }
}
