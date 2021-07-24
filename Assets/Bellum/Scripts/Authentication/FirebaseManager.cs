using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public event Action authStateChanged;
    public static event Action userNameChanged;
    public event Action userCreated;
    public FirebaseApp app;

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
    [SerializeField] private TMP_InputField usernameProfileText = null;
    [SerializeField] private TMP_Text useridProfileText = null;
    [SerializeField] private GameObject userProfilePopUp = null;
    [SerializeField] private Image userExperienceSlider = null;
    [SerializeField] private TMP_Text userLevelText = null;
    [SerializeField] private TMP_Text userExperienceText = null;
    [SerializeField] private TMP_Text userHighestPointText = null;
    [SerializeField] private TMP_Text userGoldText = null;
    [SerializeField] private TMP_Text userRubyText = null;
    [SerializeField] private TMP_Text userOpalText = null;
    [SerializeField] private TMP_Text userEmeraldText = null;
    [SerializeField] private TMP_Text userSapphireText = null;
    [SerializeField] private TMP_Text userTopazText = null;

    //ruby, opal,emerald,sapphire,topaz
    APIManager apiManager;
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private LanguageSelectionManager languageSelectionManager;

    //Auto Login
    [Header("Auto Login")]
    public static Dictionary<string, string[]> IPEmail = new Dictionary<string, string[]>() {
        { "192.168.2.181", new string []{"jack.33.wong@gmail.com","123456" } },
        { "192.168.2.16", new string[] { "leonard.11.wong@gmail.com", "sw63qd" } },
        { "192.168.2.136", new string[] { "louis.10.wong@gmail.com", "no name" } },
        { "192.168.2.11", new string[] { "antheayip@gmail.com", "123456" } },
        { "192.168.2.147", new string[] { "levi.03.wong@gmail.com", "sweetcorn" } }
    };

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
        userHighestPointText.text = StaticClass.highestPoint;
        userLevelText.text = StaticClass.level;

        userGoldText.text = StaticClass.gold;
        userRubyText.text = StaticClass.ruby;
        userOpalText.text = StaticClass.opal;
        userEmeraldText.text = StaticClass.emerald;
        userSapphireText.text = StaticClass.sapphire;
        userTopazText.text = StaticClass.topaz;
        userExperienceText.text = StaticClass.experience + " / " + (Int32.Parse(StaticClass.level) + 1) * 100 ;
        userExperienceSlider.fillAmount = (float)(Int32.Parse(StaticClass.experience) / (100f * (Int32.Parse(StaticClass.level) + 1)));


    }
    public void UpdateUserProfile()
    {
        StartCoroutine(HandleUserProfile());
    }
    IEnumerator HandleUserProfile()
    {
        Debug.Log($"UpdateUserProfile id {user.UserId} name{usernameProfileText.text}");
        UserProfile userProfile = new UserProfile();
        userProfile.DisplayName = usernameProfileText.text;
        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = user.UpdateUserProfileAsync(userProfile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
        StartCoroutine(apiManager.UpdateUserNameProfile(user.UserId, usernameProfileText.text));
        StaticClass.Username = usernameProfileText.text;
        userNameChanged?.Invoke();
        Debug.Log($"In Firebase manager apiManager is null ? {apiManager == null}");
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
        {
            loginPopUp.SetActive(true);
        }
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
        //Set the authentication instance object
        app = Firebase.FirebaseApp.DefaultInstance;
        auth = FirebaseAuth.GetAuth(app);
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    private void Start()
    {
        apiManager = new APIManager();
        StartCoroutine(AutoLogin());
    }
    IEnumerator AutoLogin()
    {
        yield return new WaitForSeconds(1f);
        string ip = GetLocalIPv4();
        saveSystem.LoadSaveDataFromDisk();
        Debug.Log($"saveSystem.saveData.ToJson : {saveSystem.saveData.ToJson()}");
        languageSelectionManager.loadLocaleFromSaveSystem(saveSystem);

        //if (StaticClass.UserID == null || StaticClass.UserID.Length == 0 )
        //if (IPEmail.ContainsKey(ip))
        //yield return Login(IPEmail[ip][0], IPEmail[ip][1]);
        yield return Login(saveSystem.saveData._email, saveSystem.saveData._password);
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
                authStateChanged?.Invoke();
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                StaticClass.Username = user.DisplayName ?? "";
                StaticClass.UserID = user.UserId;
                authStateChanged?.Invoke();
                //Debug.Log("AuthStateChanged Signed in " + user.UserId + " username: " + user.DisplayName + " StaticClass.Username " + StaticClass.Username);
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
            saveSystem.saveData._email = _email;
            saveSystem.saveData._password = _password;
            saveSystem.SaveToFile();
            loginPopUp.SetActive(false);
            authStateChanged?.Invoke();
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
                        apiManager.UpdateUserNameProfile(user.UserId, user.DisplayName);
                    }
                }
            }
        }
    }
    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

}
