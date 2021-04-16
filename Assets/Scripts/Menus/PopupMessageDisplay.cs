using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupMessageDisplay : MonoBehaviour
{
    [SerializeField] public GameObject PopupMessageDefaultPanel = null;
    [SerializeField] public GameObject PopupMessageErrorPanel = null;
    [SerializeField] public GameObject PopupParentPanel = null;

    public void displayText(float timer, string message, bool isOK)
    {
        PopupParentPanel.SetActive(true);
        StartCoroutine(HandleDisplayText(timer, message, isOK));
    }

    IEnumerator HandleDisplayText(float timer, string message, bool isOK)
    {
        if (isOK) {
            PopupMessageDefaultPanel.SetActive(true);
            PopupMessageDefaultPanel.GetComponentInChildren<TMP_Text>().text = message;
        }
        else {
            PopupMessageErrorPanel.SetActive(true);
            PopupMessageErrorPanel.GetComponentInChildren<TMP_Text>().text = message;
        }
        yield return new WaitForSeconds(timer);
        PopupMessageErrorPanel.SetActive(false);
        PopupMessageDefaultPanel.SetActive(false);
        PopupParentPanel.SetActive(false);

    }

}
