using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
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


    public async void Start()
    {
        // attempt to login automatically if player prefs exist
        
        
        
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
        
        this.gameObject.SetActive(false);
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
