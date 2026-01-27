using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using Steamworks;
using UnityEngine;

public class SteamScript : MonoBehaviour
{
    protected Callback<GetTicketForWebApiResponse_t> m_OnGetSteamAuthTicket;

    // Alternatively, you can use this callback if you choose to call SteamUser.GetAuthSessionTicket(...) instead
    // TicketIsServiceSpecific in the PlayFabLoginRequest should be false in this case
    // protected Callback<GetAuthSessionTicketResponse_t> m_OnGetSteamAuthTicketAlternate;

    private HAuthTicket m_hTicket;

    public void Awake()
    {
        m_OnGetSteamAuthTicket = Callback<GetTicketForWebApiResponse_t>.Create(OnGetSteamAuthTicket);
    }
    
    public void OnGUI()
    {
        if (GUILayout.Button("Log In") && SteamManager.Initialized)
        {
            GetSteamAuthTicket();
        }
    }

    private void GetSteamAuthTicket()
    {
        m_hTicket = SteamUser.GetAuthTicketForWebApi("AzurePlayFab");

        if (m_hTicket == HAuthTicket.Invalid)
        {
            Debug.Log("Failed to request steam auth ticket");
        }
        else
        {
            Debug.Log("Steam auth ticket requested");
        }
    }

    private void OnGetSteamAuthTicket(GetTicketForWebApiResponse_t pCallback)
    {
        Debug.Log("Steam auth ticket callback invoked");

        if (pCallback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Failed to get steam auth ticket: " + pCallback.m_eResult);
        }

        StringBuilder sb = new();
        for (int i = 0; i < pCallback.m_cubTicket; ++i)
        {
            sb.AppendFormat("{0:x2}", pCallback.m_rgubTicket[i]);
        }

        PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest
        {
            CreateAccount = true,
            SteamTicket = sb.ToString(),
            TicketIsServiceSpecific = true
        }, OnComplete, OnFailed);
    }

    private void OnComplete(LoginResult obj)
    {
        SteamUser.CancelAuthTicket(m_hTicket);
        Debug.Log("Success!");
    }

    private void OnFailed(PlayFabError error)
    {
        SteamUser.CancelAuthTicket(m_hTicket);
        Debug.Log("Failed PlayFab login: " + error.GenerateErrorReport());
    }
}