using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamTest : MonoBehaviour
{
    
    void Start() {
        if(SteamManager.Initialized) {
            //string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }
    }
    
}
