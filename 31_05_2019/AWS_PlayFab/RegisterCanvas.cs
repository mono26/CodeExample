using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System;

#if GOOGLEGAMES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class RegisterCanvas : MonoBehaviour
{
    [Header("Register")]
    //Meta references to panels we need to show / hide
    public InputField registerEmail;
    public InputField registerUsername;
    public InputField registerPassword;
    public InputField confirmPassword;
    public GameObject registerPanel;
    public GameObject loginPanel;
    public Button finishRegisterButton;
    public Button cancelRegisterButton;
    [SerializeField] LoadingPanel loading = null;

    PlayFabServices authService = PlayFabServices.Instance;
    Authtypes registrationType = Authtypes.None;

    // Start is called before the first frame update
    void Start()
    {
        finishRegisterButton.onClick.AddListener(OnFinishRegisterButtonClicked);
        cancelRegisterButton.onClick.AddListener(OnCancelRegisterButtonClicked);
    }

    /// <summary>
    /// No account was found, and they have selected to register a username (email) / password combo.
    /// </summary>
    private void OnFinishRegisterButtonClicked()
    {
        if (registrationType == Authtypes.None)
        {
            return;
        }

        if (!InputIsValid())
        {
            Debug.LogError("Todos los campos deben ser validos");
            return;
        }

        if (loading)
        {
            loading.UpdateLabel("Registering user");
            loading.StartLoading();
        }
        else
        {
            Debug.LogError("Register canvas is missing the loading reference");
        }

        if (registrationType == Authtypes.EmailAndPassword)
        {
            Debug.LogError("Standard registration");
            FinishStandardRegistration();
        }
        else if (registrationType == Authtypes.Google)
        {
            Debug.LogError("Social registration");
            FinishSocialRegistration();
        }
    }

    bool InputIsValid()
    {
        // Para validar todos los inputs
        if (authService.AuthType == Authtypes.EmailAndPassword)
        {
            if (!EmailIsValid())
            {
                return false;
            }
            if (!PasswordIsValid())
            {
                return false;
            }
        }
        if (!UserNameIsValid())
        {
            return false;
        }

        return true;
    }
    
    bool EmailIsValid()
    {
        bool isValid = true;
        string email = registerEmail.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
        {
            Debug.LogError("Debes introducir un email valido");
            isValid = false;
        }
        return isValid;
    }

    bool UserNameIsValid()
    {
        bool isValid = true;
        string userName = registerUsername.text;
        if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(userName))
        {
            Debug.LogError("Debes introducir un username valido");
            isValid = false;
        }
        return isValid;
    }

    bool PasswordIsValid()
    {
        bool isValid = true;
        string password = registerPassword.text;
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
        {
            Debug.LogError("Debes introducir una clave valida");
            isValid = false;
        }
        if (registerPassword.text != confirmPassword.text)
        {
            Debug.LogErrorFormat(" {0} no es igual a: {1}", registerPassword.text, confirmPassword.text);
            isValid = false;
        }
        return isValid;
    }

    void FinishStandardRegistration()
    {
        authService.email = registerEmail.text;
        authService.password = registerPassword.text;
        authService.username = registerUsername.text;
        authService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }

    void FinishSocialRegistration()
    {
        StartCoroutine(authService.RegisterGoogleAccount(registerUsername.text));
    }

    /// <summary>
    /// They have opted to cancel the Registration process.
    /// Possibly they typed the email address incorrectly.
    /// </summary>
    private void OnCancelRegisterButtonClicked()
    {
        //Reset all forms
        registerEmail.text = string.Empty;
        registerUsername.text = string.Empty;
        registerPassword.text = string.Empty;
        confirmPassword.text = string.Empty;

        //Show panels
        ToggleRegistration(false);
        loginPanel.SetActive(true);
    }

    public void ToggleRegistration(bool _toggle)
    {
        registerPanel.SetActive(_toggle);
    }

    void DisplayProperFields()
    {
        bool emailPasswordAuth = registrationType == Authtypes.EmailAndPassword ? true : false;
        registerEmail.gameObject.SetActive(emailPasswordAuth);
        registerPassword.gameObject.SetActive(emailPasswordAuth);
        confirmPassword.gameObject.SetActive(emailPasswordAuth);
    }

    public void SetUserNameField(string _userName)
    {
        registerUsername.text = _userName;
    }

    public void SetEmailField(string _userName)
    {
        registerEmail.text = _userName;
    }

    public void SetPasswordField(string _userName)
    {
        registerPassword.text = _userName;
    }

    public void SetRegistrationType(Authtypes _type)
    {
        registrationType = _type;
    }
}
