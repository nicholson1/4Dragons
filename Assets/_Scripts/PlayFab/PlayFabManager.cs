using System;
using System.Collections.Generic;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using _Scripts.PlayFab;
using UnityEngine;
using LoginResult = PlayFab.ClientModels.LoginResult;
using PlayFabError = PlayFab.PlayFabError;
using PlayFabErrorCode = PlayFab.PlayFabErrorCode;

public class PlayFabManager : MonoBehaviour
{
    private string _sessionTicket; //
    private DateTime _loginTime;
    private string _playFabId;
    private bool _waitingOnProfile; //used to determine when playFab has sent or error out on profile
    private bool _errorGettingProfile; //indicates if there was an error getting the profile
    private bool _emailVerifiedForCurrentProfile; //indicated whether the user has confirmed their email address
    private const double SessionTimeoutHours = 20.0;

    private const string EmailKey = "PlayFabEmail"; //  Key for storing email in PlayerPrefs
    private const string PasswordKey = "PlayFabPassword"; // Key for storing Password in PlayerPrefs
    private string current_sw_version = "NO_VERSION";
    private string current_platform = "NO_PLATFORM";

    public bool IsInitialized => !string.IsNullOrEmpty(_sessionTicket);
    public bool IsLoggedIn => IsInitialized && !IsSessionTicketExpired();
    public string LoggedInEmail => (IsLoggedIn) ? LoadCredential(EmailKey) : "";

    public static PlayFabManager _instance;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        // Ensure PlayFab Title ID is set
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            Debug.LogError("Title ID is not set. Please set the Title ID in the PlayFab settings.");
        }

        current_sw_version = Application.version;
        current_platform = Application.platform.ToString();
    }

    public async Task<bool> CheckIfEmailExistsAsync(string email)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = "dummy P a s s w ord" // A dummy password for the login attempt
        };

        var tcs = new TaskCompletionSource<bool>();

        PlayFabClientAPI.LoginWithEmailAddress(request,
            result =>
            {
                // Login succeeded - email exists
                tcs.SetResult(true);
            },
            error =>
            {
                if (error.Error == PlayFabErrorCode.AccountNotFound)
                {
                    // Account not found - email does not exist
                    tcs.SetResult(false);
                }
                else if (error.Error == PlayFabErrorCode.InvalidEmailAddress ||
                         error.Error == PlayFabErrorCode.InvalidPassword)
                {
                    // Invalid email or password - email exists
                    tcs.SetResult(true);
                }
                else
                {
                    // Other errors
                    Debug.LogError("Error checking email: " + error.GenerateErrorReport());
                    tcs.SetResult(false);
                }
            }
        );

        return await tcs.Task;
    }

    public enum RegistrationResult
    {
        SUCCESS,
        EMAIL_EXISTS,
        ERROR
    }
    public enum AutoResult
    {
        SUCCESS,
        NODATA,
        ERROR
    }

    public async Task<RegistrationResult> RegisterAccountAsync(string email, string password)
    {
        RegistrationResult registrationResult = RegistrationResult.ERROR;
        var registerRequest = new RegisterPlayFabUserRequest
        {
            RequireBothUsernameAndEmail = false,
            Email = email,
            Password = password
        };

        var result = await PlayFabClientAPIAsync.RegisterPlayFabUserAsync(registerRequest);
        if (result.Success)
        {
            SaveCredentials(email, password);
            OnRegisterSuccess(result.Result);
            registrationResult = RegistrationResult.SUCCESS;
        }
        else
        {
            if (result.Error.Error == PlayFabErrorCode.EmailAddressNotAvailable)
            {
                registrationResult = RegistrationResult.EMAIL_EXISTS;
            }
            else
            {
                OnError(result.Error);
                registrationResult = RegistrationResult.ERROR;
            }

        }

        return registrationResult;
    }


    //======================================== Login Functionality ==================================================
    public async Task<bool> LoginAccountAsync(string email, string password)
    {
        DateTime startTime = DateTime.UtcNow;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Cannot login: Email or password is blank.");
            return false;
        }

        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        SaveCredentials(email, password);

        var result = await PlayFabClientAPIAsync.LoginWithEmailAddressAsync(loginRequest);
        if (result.Success)
        {
            double totalSeconds = DateTime.UtcNow.Subtract(startTime).TotalSeconds;
            OnLoginSuccess(result.Result);
            return true;
        }
        else
        {
            OnError(result.Error);
            return false;
        }
    }

    public async Task<string> GetSessionTicketAsync()
    {
        DateTime startTime = DateTime.UtcNow;
        if (IsSessionTicketExpired())
        {
            await RefreshSessionTicketAsync();
        }

        if (string.IsNullOrEmpty(_sessionTicket))
        {
            Debug.LogWarning("User is not logged in. Session ticket is null or empty.");
            return null;
        }

        return _sessionTicket;
    }

    private bool IsSessionTicketExpired()
    {
        return (DateTime.UtcNow - _loginTime).TotalHours >= SessionTimeoutHours;
    }

    private async Task RefreshSessionTicketAsync()
    {
        string email = LoadCredential(EmailKey);
        string password = LoadCredential(PasswordKey);

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            password = EncryptionUtility.Decrypt(password);
            await LoginAccountAsync(email, password);
        }
        else
        {
            Debug.LogWarning("Cannot refresh session ticket: Email or password is missing.");
        }
    }

    public async Task<AutoResult> AutoLogin()
    {
        
        string email = LoadCredential(EmailKey);
        string password = LoadCredential(PasswordKey);

        if (password == null || email == null)
            return AutoResult.NODATA;
        
        password = EncryptionUtility.Decrypt(password);
        
        DateTime startTime = DateTime.UtcNow;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Cannot login: Email or password is blank.");
            return AutoResult.ERROR;
        }

        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        SaveCredentials(email, password);

        var result = await PlayFabClientAPIAsync.LoginWithEmailAddressAsync(loginRequest);
        if (result.Success)
        {
            double totalSeconds = DateTime.UtcNow.Subtract(startTime).TotalSeconds;
            OnLoginSuccess(result.Result);
            return AutoResult.SUCCESS;
        }
        else
        {
            OnError(result.Error);
            return AutoResult.ERROR;
        }
        
    }


    public async Task RecoverPasswordAsync(string email)
    {
        var recoveryRequest = new SendAccountRecoveryEmailRequest
        {
            Email = email,
            TitleId = PlayFabSettings.staticSettings.TitleId
        };

        var result = await PlayFabClientAPIAsync.SendAccountRecoveryEmailAsync(recoveryRequest);
        if (result.Success)
        {
            Debug.Log("Account recovery email sent successfully.");
        }
        else
        {
            OnError(result.Error);
        }
    }


    public async Task<string> RequestPasswordChange(string email)
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = email,
            TitleId = PlayFabSettings.TitleId // replace with your title id
        };

        var tcs = new TaskCompletionSource<string>();
        PlayFabClientAPI.SendAccountRecoveryEmail(request,
            result => { tcs.TrySetResult("Password change email sent successfully"); },
            error => { tcs.TrySetResult($"Password change email send failed: {error.GenerateErrorReport()}"); });

        return await tcs.Task;
    }


    //===================================== email verification functionality =======================================

    public async Task<bool> IsPlayerEmailVerified()
    {
        if (_emailVerifiedForCurrentProfile)
            return true;

        _errorGettingProfile = false;
        _waitingOnProfile = true;
        ObtainEmailVerificationStatus();
        var timeOut = DateTime.Now.AddSeconds(10);

        while (_waitingOnProfile && !_errorGettingProfile && DateTime.Now < timeOut)
            await Task.Delay(250);

        if (_errorGettingProfile || _waitingOnProfile)
        {
            Debug.LogError(
                $"Unable to verify email.  _errorGettingProfile: {_errorGettingProfile}, _waitingOnProfile: {_waitingOnProfile}");
            return false;
        }


        return _emailVerifiedForCurrentProfile;
    }

    private void ObtainEmailVerificationStatus()
    {
        var request = new GetPlayerProfileRequest
        {
            PlayFabId = _playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowContactEmailAddresses = true, // This enables the retrieval of email status.
            }
        };

        PlayFabClientAPI.GetPlayerProfile(request, (result =>
        {

            Debug.Log("Obtained email verification status");
            if (result.PlayerProfile.ContactEmailAddresses.Count > 0 &&
                !string.IsNullOrEmpty(result.PlayerProfile.ContactEmailAddresses[0].EmailAddress))
            {
                _emailVerifiedForCurrentProfile = result.PlayerProfile.ContactEmailAddresses[0].VerificationStatus
                                                  == EmailVerificationStatus.Confirmed;
                Debug.Log($"Email verified: {_emailVerifiedForCurrentProfile}");
            }
            else
            {
                Debug.LogWarning("User didn't have contact email set.  Setting now.");
                AddOrUpdateContactEmail(_playFabId, PlayerPrefs.GetString(EmailKey));
            }

            _waitingOnProfile = false;

        }), (error) =>
        {
            _errorGettingProfile = true;
            Debug.LogError("Error obtaining email verification status: " + error.GenerateErrorReport());
            ;
        });
    }


    //============================== Credential Operations ==================================



    private void SaveCredentials(string email, string password)
    {
        PlayerPrefs.SetString(EmailKey, email);
        PlayerPrefs.SetString(PasswordKey, EncryptionUtility.Encrypt(password));
        PlayerPrefs.Save();
    }

    private string LoadCredential(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetString(key);
        }

        return null;
    }

    // =================================== Handler Methods ========================================

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("User registered successfully.");
        _sessionTicket = result.SessionTicket;
        _playFabId = result.PlayFabId;
        _loginTime = DateTime.UtcNow;
        AddOrUpdateContactEmail(_playFabId, LoadCredential(EmailKey));
    }

    private async void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("User logged in successfully.");
        _sessionTicket = result.SessionTicket;
        _loginTime = DateTime.UtcNow;
        _playFabId = result.PlayFabId;
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }

    public void SignOut()
    {
        _sessionTicket = null;
        _playFabId = null;
        PlayerPrefs.DeleteKey(EmailKey);
        PlayerPrefs.DeleteKey(PasswordKey);
        PlayerPrefs.Save();
    }

    //======================== Updating contact e-mail =================================


    void AddOrUpdateContactEmail(string playFabId, string emailAddress)
    {
        var request = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = emailAddress
        };
        PlayFabClientAPI.AddOrUpdateContactEmail(request,
            result => { Debug.Log("The player's account has been updated with a contact email"); }, FailureCallback);
    }

    void FailureCallback(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your API call. Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
    
    // ========================== Stat INFO and Calls ========================================
    public void SubmitRunData(bool victory)
    {
        // this method runs on death and on Victory
        
        List<Dictionary<string, object>> abilities = new List<Dictionary<string, object>>();
        List<Dictionary<string, object>> seenRelics = new List<Dictionary<string, object>>();
        List<Dictionary<string, object>> notSelectedRelics = new List<Dictionary<string, object>>();


        //=============== Turn Abilities into the correct format ==========================
        foreach (var ability in StatsTracker.Instance.AbilityUsageTracker.Values)
        {
            abilities.Add(new Dictionary<string, object>
            {
                { "AbilityType", ability.AbilityType },
                { "Level", ability.Level },
                { "Rarity", ability.Rarity },
                { "UsageCount", ability.UsageCount }
            });
        }
        //=============== Turn seen Relics into the correct format ==========================
        foreach (var relic in StatsTracker.Instance.RelicsPicked)
        {
            seenRelics.Add(new Dictionary<string, object>
            {
                { relic.Key, relic.Value },
            });
        }
        //=============== Turn UN seen Relics into the correct format ==========================
        foreach (var relic in StatsTracker.Instance.RelicsNotPicked)
        {
            notSelectedRelics.Add(new Dictionary<string, object>
            {
                { relic.Key, relic.Value },
            });
        }
        
        //todo likley change this to a Named event or something
        //todo make sure the abilities seen relics and not selected relics are appropirately being added to the body.
        
        var request = new WriteClientPlayerEventRequest
        {
            EventName = "RunData",
            Body = new Dictionary<string, object>() {
                { "game_difficulty", CombatController._instance.Difficulty },
                { "final_player_level", CombatController._instance.Player._level},
                { "won_game", victory},
                { "platform", current_platform},
                { "app_version", current_sw_version },
                { "Abilities", abilities },
                { "Selected_Relics", seenRelics },
                { "Not_Selected_Relics", notSelectedRelics }

            },
        };
        
        //========== Debug Print the dictionary ===========================

        // string s = "";
        // foreach (var item in request.Body)
        // {
        //     s += $"{item.Key}: {item.Value} \n";
        // }
        // Debug.Log(s);
        // ======================================================

        PlayFabClientAPI.WritePlayerEvent(request, 
            result => Debug.Log("Successfully submitted Run Data to PlayFab."),
            error => Debug.LogError("Failed to submit data: " + error.GenerateErrorReport()));
    }
}

