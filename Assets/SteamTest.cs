using System.Collections;
using System.Collections.Generic;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using UnityEngine;

public class SteamTest : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    void Start() {
        if(SteamManager.Initialized) {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }
    }
#endif
}
