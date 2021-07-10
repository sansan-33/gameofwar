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
    }
    private IEnumerator DisableEffect(GameObject effect)
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(effect);
    }
    // Update is called once per frame
   

}
