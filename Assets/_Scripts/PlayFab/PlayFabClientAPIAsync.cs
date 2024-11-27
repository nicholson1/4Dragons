using PlayFab;
using PlayFab.ClientModels;

// for working with asynchronous programming
using System.Threading.Tasks;

// A static class to provide asynchronous wrappers for PlayFab Client API methods
public static class PlayFabClientAPIAsync
{
    // Asynchronous wrapper for the PlayFab RegisterPlayFabUser API call
    // Takes a RegisterPlayFabUserRequest object and returns a Task containing the result
    public static Task<PlayFabResult<RegisterPlayFabUserResult>> RegisterPlayFabUserAsync(RegisterPlayFabUserRequest request)
    {
        // Create a TaskCompletionSource to handle the async operation
        var tcs = new TaskCompletionSource<PlayFabResult<RegisterPlayFabUserResult>>();

        // Call the PlayFab API method
        PlayFabClientAPI.RegisterPlayFabUser(
            request, 
            // On success, complete the task with the result
            result => tcs.SetResult(new PlayFabResult<RegisterPlayFabUserResult> { Success = true, Result = result }), 
            // On error, complete the task with the error details
            error => tcs.SetResult(new PlayFabResult<RegisterPlayFabUserResult> { Success = false, Error = error })
        );

        // Return the task to the caller
        return tcs.Task;
    }

    // Asynchronous wrapper for the PlayFab LoginWithPlayFab API call
    // Takes a LoginWithPlayFabRequest object and returns a Task containing the result
    public static Task<PlayFabResult<LoginResult>> LoginWithPlayFabAsync(LoginWithPlayFabRequest request)
    {
        // Create a TaskCompletionSource to handle the async operation
        var tcs = new TaskCompletionSource<PlayFabResult<LoginResult>>();

        // Call the PlayFab API method
        PlayFabClientAPI.LoginWithPlayFab(
            request, 
            // On success, complete the task with the result
            result => tcs.SetResult(new PlayFabResult<LoginResult> { Success = true, Result = result }), 
            // On error, complete the task with the error details
            error => tcs.SetResult(new PlayFabResult<LoginResult> { Success = false, Error = error })
        );

        // Return the task to the caller
        return tcs.Task;
    }

    // Asynchronous wrapper for the PlayFab SendAccountRecoveryEmail API call
    // Takes a SendAccountRecoveryEmailRequest object and returns a Task containing the result
    public static Task<PlayFabResult<SendAccountRecoveryEmailResult>> SendAccountRecoveryEmailAsync(SendAccountRecoveryEmailRequest request)
    {
        // Create a TaskCompletionSource to handle the async operation
        var tcs = new TaskCompletionSource<PlayFabResult<SendAccountRecoveryEmailResult>>();

        // Call the PlayFab API method
        PlayFabClientAPI.SendAccountRecoveryEmail(
            request, 
            // On success, complete the task with the result
            result => tcs.SetResult(new PlayFabResult<SendAccountRecoveryEmailResult> { Success = true, Result = result }), 
            // On error, complete the task with the error details
            error => tcs.SetResult(new PlayFabResult<SendAccountRecoveryEmailResult> { Success = false, Error = error })
        );

        // Return the task to the caller
        return tcs.Task;
    }
    
    // Asynchronous wrapper for the PlayFab LoginWithEmailAddress API call
    // Takes a LoginWithEmailAddressRequest object and returns a Task containing the result
    public static Task<PlayFabResult<LoginResult>> LoginWithEmailAddressAsync(LoginWithEmailAddressRequest request)
    {
        // Create a TaskCompletionSource to handle the async operation
        var tcs = new TaskCompletionSource<PlayFabResult<LoginResult>>();

        // Call the PlayFab API method
        PlayFabClientAPI.LoginWithEmailAddress(
            request,
            // On success, complete the task with the result
            result => tcs.SetResult(new PlayFabResult<LoginResult> { Success = true, Result = result }),
            // On error, complete the task with the error details
            error => tcs.SetResult(new PlayFabResult<LoginResult> { Success = false, Error = error })
        );

        // Return the task to the caller
        return tcs.Task;
    }
}

// A generic class to represent the result of a PlayFab API call
// Includes a success flag, the result object (if successful), and an error object (if failed)
public class PlayFabResult<T>
{
    public bool Success { get; set; } // Indicates whether the operation was successful
    public T Result { get; set; } // Contains the result of the operation if successful
    public PlayFabError Error { get; set; } // Contains error details if the operation failed
}
