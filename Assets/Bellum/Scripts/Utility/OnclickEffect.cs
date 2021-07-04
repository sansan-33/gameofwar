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
        _effect.transform.position = pos;
        _effect.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DisableEffect());
    }
    public void OnPointerClick()
    {
        Debug.Log("ON click");
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Debug.Log($"spawn pos{pos}");
        _effect = Instantiate(effect, parent);
        _effect.transform.position = pos;
        _effect.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DisableEffect());
    }
    private IEnumerator DisableEffect()
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(_effect);
    }
    // Update is called once per frame
    void Update()
    {
       
    }
}
