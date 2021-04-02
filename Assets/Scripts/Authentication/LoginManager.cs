using System;
using Mirror;
using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField useridInput = null;
    [SerializeField] private GameObject loginPopUp = null;
    [SerializeField] public TMP_Text userid = null;

    public void HandleLogin()
    {
        StaticClass.UserID = useridInput.text ;
        userid.text = StaticClass.UserID;
        loginPopUp.SetActive(false);
    }
}

