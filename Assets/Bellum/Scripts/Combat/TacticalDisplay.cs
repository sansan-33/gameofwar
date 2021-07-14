using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TacticalDisplay : MonoBehaviour
{
    [SerializeField] private GameObject tacticalBarParent = null;
    private Quaternion startRotation;
    [SerializeField] public bool defaultDisbale=false;

    public void Start()
    {
        if (tacticalBarParent == null) { return; }
        tacticalBarParent.SetActive(false);
        startRotation = tacticalBarParent.transform.rotation;
        if (NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID() == 1 )
        {
            startRotation *= Quaternion.Euler(0, 180f, 0);
        }
    }
    void Update()
    {
        //tacticalBarParent.transform.rotation = startRotation;
        //StartCoroutine(LateCall());
    }
    private void OnDestroy()
    {
    }
    IEnumerator LateCall()
    {
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
        Debug.Log($"Set active false LateCall");
    }
    void OnEnable()
    {
        if(defaultDisbale) { gameObject.SetActive(false); return; }
        GameObject[] floatButtons = GameObject.FindGameObjectsWithTag("FloatButton");
        foreach (GameObject btn in floatButtons) {
            if(btn != this.gameObject)
            btn.SetActive(false);
        }
    }
}
