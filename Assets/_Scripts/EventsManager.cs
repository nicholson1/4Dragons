using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    public static EventsManager _instance;
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
    }

    public void RandomEvent()
    {
        // ensure any randomness is based on our current seed
        Random.InitState(CombatController._instance.LastNodeClicked.nodeSeed);
        
        
        
        // select which event to show
        
        //fill out the info
        
        // toggle ui show
        UIController._instance.ToggleEventUI(1);
    }

    public void CloseEventUI()
    {
        UIController._instance.ToggleEventUI(0);
    }
}
