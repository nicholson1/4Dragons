using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] private GameObject login;
    [SerializeField] private GameObject register;

    [SerializeField] private TMP_InputField EmailReg;
    [SerializeField] private TMP_InputField PasswordReg;
    [SerializeField] private TMP_InputField PasswordConfirmReg;
    
    [SerializeField] private TMP_InputField EmailLog;
    [SerializeField] private TMP_InputField PasswordLog;

    [SerializeField] private PlayFabManager _playFabManager;

    [SerializeField] private GameObject ErrorTextObj;
    [SerializeField] private TextMeshProUGUI ErrorText;
    [SerializeField] private CanvasGroup errorGroup;

    private CanvasGroup _canvasGroup;

    public async void Start()
    {
        // attempt to login automatically if player prefs exist
        
        _canvasGroup = GetComponent<CanvasGroup>();

        if (WorkInProgress._instance.hasDisplayed)
        {
            _canvasGroup.gameObject.SetActive(false);
            
        }
        else
        {
            StartCoroutine(FadeIn(_canvasGroup , 1));
        }
        
        bool loggedInSuccessfully = false;
        
#if !DISABLESTEAMWORKS
        Debug.Log("LoginController: Editor or Standalone platform. Checking for Steam login.");
        if (SteamManager.Initialized)
        {
            Debug.Log("LoginController: SteamManager is initialized. Attempting to get Steam auth ticket...");
            string steamTicket = await SteamManager.Instance.GetSteamAuthSessionTicketAsync();

            if (!string.IsNullOrEmpty(steamTicket))
            {
                Debug.Log("LoginController: Steam ticket obtained. Attempting PlayFab login with Steam.");
                // Call the Tuple-returning LoginWithSteamAsync
                Tuple<string, bool> steamLoginResultTuple = await _playFabManager.LoginWithSteamAsync(steamTicket);
                loggedInSuccessfully = steamLoginResultTuple.Item2; // Item2 is the bool for success

                if (loggedInSuccessfully)
                {
                    Debug.Log($"LoginController: Successfully logged in via Steam. Message: {steamLoginResultTuple.Item1}");
                    LoginSuccess();
                    return;
                }
                else
                {
                    Debug.LogError($"LoginController: PlayFab login with Steam ticket failed. Message: {steamLoginResultTuple.Item1}");
                }
            }
            else
            {
                Debug.LogWarning("LoginController: Failed to get Steam ticket (or user not logged on to Steam). Will try email/password auto-login.");
            }
        }
        else
        {
            Debug.LogWarning("LoginController: SteamManager not initialized. Skipping Steam login attempt.");
        }
#endif

        if (!loggedInSuccessfully)
        {

            PlayFabManager.AutoResult loginResult = await _playFabManager.AutoLogin();
            switch (loginResult)
            {
                case PlayFabManager.AutoResult.NODATA:
                    return;
                case PlayFabManager.AutoResult.ERROR:
                    ErrorLogin();
                    return;
                case PlayFabManager.AutoResult.SUCCESS:
                    LoginSuccess();
                    return;
            }
        }
    }
    
    public IEnumerator FadeIn(CanvasGroup targetCanvasGroup, float duration)
    {
        targetCanvasGroup.gameObject.SetActive(true);
        float elapsedTime = 0f;

        // Ensure the CanvasGroup is visible and starts from 0 alpha
        targetCanvasGroup.alpha = 0;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            targetCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null; // Wait until the next frame
        }

        // Make sure alpha is set to 1 after fading
        targetCanvasGroup.alpha = 1;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;
        
    }
    public IEnumerator FadeOutAndDeactivate(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null. Cannot fade out.");
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null; // Wait until the next frame
        }

        // Ensure the alpha is set to 0 at the end
        canvasGroup.alpha = 0f;

        // Deactivate the CanvasGroup's GameObject
        canvasGroup.gameObject.SetActive(false);
    }

    public async void SkipRegister()
    {
        EmailLog.text = "Guest@Guest.Guest";
        PasswordLog.text = "GuestGuest";
        Login();
    }

    public async void Register()
    {
        //check if email in correct format
        if (!IsValidEmail(EmailReg.text))
        {
            ErrorEmailFormat();
            return;
        }
        
        //check if password is 6 char
        if (PasswordReg.text.Length < 6 || PasswordReg.text.Length > 100)
        {
            ErrorPasswordLength();
            return;
        }
        
        // check if passwords are the same
        if(PasswordReg.text != PasswordConfirmReg.text)
        {
            ErrorPassword();
            return;
        }
        
        //check if email already exists
        bool exists = await _playFabManager.CheckIfEmailExistsAsync(EmailReg.text);
        if (exists)
        {
            ErrorEmailExists();
            return;
        }

        PlayFabManager.RegistrationResult
            result = await _playFabManager.RegisterAccountAsync(EmailReg.text, PasswordReg.text);

        switch (result)
        {
            case PlayFabManager.RegistrationResult.EMAIL_EXISTS:
                ErrorEmailExists();
                return;
            case PlayFabManager.RegistrationResult.ERROR:
                ErrorRegistration();
                return;
            case PlayFabManager.RegistrationResult.SUCCESS:
                RegistrationSuccess();
                bool loginResult = await _playFabManager.LoginAccountAsync(EmailReg.text, PasswordReg.text);
                if (loginResult == false)
                {
                    ErrorLogin();
                    return;
                }
                else
                {
                    LoginSuccess();
                }
                return;
        }
    }
    

    public async void Login()
    {
        // check if email is in correct format
        if (!IsValidEmail(EmailLog.text))
        {
            ErrorEmailFormat();
            return;
        }
        //check password len
        if (PasswordLog.text.Length < 6 || PasswordLog.text.Length > 100)
        {
            ErrorPasswordLength();
            return;
        }

        bool loginResult = await _playFabManager.LoginAccountAsync(EmailLog.text, PasswordLog.text);
        if (loginResult == false)
        {
            ErrorLogin();
            return;
        }
        else
        {
            LoginSuccess();
        }
        ToolTipManager._instance.HideToolTipAll();
        
    }
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Regular expression for basic email validation
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }
    
    private void RegistrationSuccess()
    {
        CloseError();
        //login i suppose
        
        // set player prefs
        
    }
    private void LoginSuccess()
    {
        CloseError();
        //login i suppose

        if(this.gameObject.activeInHierarchy)
            StartCoroutine(FadeOutAndDeactivate(_canvasGroup, 1));
        WorkInProgress._instance.OpenWorkInProgress();
    }

    public void CloseError()
    {
        ErrorTextObj.SetActive(false);
    }
    public void ErrorPasswordLength()
    {
        ErrorTextObj.SetActive(true);
        ErrorText.text = "Passwords must be at least 6 characters!";
        StartCoroutine(FadeCanvasGroup(errorGroup, 1, 1));


    }
    private void ErrorPassword()
    {
        ErrorTextObj.SetActive(true);
        ErrorText.text = "Passwords do not match!";
        StartCoroutine(FadeCanvasGroup(errorGroup, 1, 1));

    }
    private void ErrorRegistration()
    {
        ErrorTextObj.SetActive(true);
        ErrorText.text = "Error during registration!";
        StartCoroutine(FadeCanvasGroup(errorGroup, 1, 1));

    }
    private void ErrorEmailFormat()
    {
        ErrorTextObj.SetActive(true);
        ErrorText.text = "Email not in correct format!";
        StartCoroutine(FadeCanvasGroup(errorGroup, 1, 1));

    }
    private void ErrorEmailExists()
    {
        ErrorTextObj.SetActive(true);
        ErrorText.text = "Email already exists!";
        StartCoroutine(FadeCanvasGroup(errorGroup, 1, 1));

    }
    private void ErrorLogin()
    {
        ErrorTextObj.SetActive(true);
        ErrorText.text = "Email and or password is wrong!";
        StartCoroutine(FadeCanvasGroup(errorGroup, 1, 1));
    }
    public void LoadLogin()
    {
        login.SetActive(true);
        register.SetActive(false);
    }
    public void LoadRegister()
    {
        login.SetActive(false);
        register.SetActive(true);
    }

    public void SignOut()
    {
        _playFabManager.SignOut();
        this.gameObject.SetActive(true);
        SceneManager.LoadScene(0);

    }
    
    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        // Check if the CanvasGroup is valid
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null.");
            yield break;
        }

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(true);

        // Store the initial alpha value
        float startAlpha = canvasGroup.alpha;

        // Track the time elapsed
        float elapsedTime = 0f;

        // Gradually change the alpha value
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        // Set the final alpha value to ensure it reaches the target
        canvasGroup.alpha = targetAlpha;
        if(targetAlpha == 0)
            canvasGroup.gameObject.SetActive(false);

    }
}
