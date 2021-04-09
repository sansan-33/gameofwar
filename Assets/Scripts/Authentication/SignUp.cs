using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SignUp : MonoBehaviour
{
    [SerializeField] public TMP_Text userid = null;
    public event Action<bool> singup;
    [SerializeField] private GoogleAuth googleAuth;
    [SerializeField] private TMP_InputField emailInput = null;
    [SerializeField] private TMP_InputField passwordInput = null;
    [SerializeField] private GameObject signupPopUp = null;

    // Update is called once per frame
    void Update()
    {

    }
    public void HandleSignUp()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        googleAuth.SignUp(email, password);
    }
}
