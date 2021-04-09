using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    //Firebase variables
    [Header("Sign Up Form")]
    [SerializeField] private TMP_InputField usernameRegisterField = null;
    [SerializeField] private TMP_InputField emailRegisterField = null;
    [SerializeField] private TMP_InputField passwordRegisterField = null;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField = null;
    [SerializeField] private TMP_Text warningRegisterText = null;
    [SerializeField] private GameObject signupPopUp = null;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    [SerializeField] private GameObject loginPopUp = null;


    //User Profile variables
    [Header("Profile")]
    [SerializeField] private TMP_Text usernameProfileText = null;
    [SerializeField] private TMP_Text useridProfileText = null;
    [SerializeField] private GameObject userProfilePopUp = null;

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public event Action<bool> authStateChanged;

    [SerializeField] public TMP_Text userid = null;

    public void HandleSignUp()
    {
        StartCoroutine( Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    //Function for the login button
    public void HandleLogin()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the login button
    public void HandleLoadUserProfile()
    {
        usernameProfileText.text = user.DisplayName;
        useridProfileText.text = user.UserId;
    }
    //Function for the sign out button
    public void HandleSignOut()
    {
        auth.SignOut();
        userProfilePopUp.SetActive(false);
        ClearRegisterFeilds();
        ClearLoginFeilds();
    }
    public void ShowLoginProfile()
    {
        if (auth != null && auth.CurrentUser != null && user != null)
        {
            userProfilePopUp.SetActive(true);
            HandleLoadUserProfile();
        }
        else
            loginPopUp.SetActive(true);
    }
    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                StaticClass.UserID = "";
                StaticClass.Username = "";
                userid.text = "userid";
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("AuthStateChanged Signed in " + user.UserId + " username: " + user.DisplayName);
                userid.text = user.DisplayName ?? "";
                StaticClass.Username = user.DisplayName ?? "";
                StaticClass.UserID = user.UserId;
                authStateChanged?.Invoke(false);
            }
        }
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            warningLoginText.text = "";

            loginPopUp.SetActive(false);
            authStateChanged?.Invoke(false);
            ClearLoginFeilds();
            ClearRegisterFeilds();

        }
    }
    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }
    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result;

                if (user != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = user.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        signupPopUp.SetActive(false);
                        warningRegisterText.text = "";
                        ClearRegisterFeilds();
                        
                    }
                }
            }
        }
    }

}
