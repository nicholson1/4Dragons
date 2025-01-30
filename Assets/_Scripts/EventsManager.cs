using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventsManager : MonoBehaviour
{
    public static EventsManager _instance;
    [SerializeField] private TextMeshProUGUI EventText;
    [SerializeField] private TextMeshProUGUI EventTitle;
    [SerializeField] private TextMeshProUGUI Option1text;
    [SerializeField] private TextMeshProUGUI Option2text;
    [SerializeField] private TextMeshProUGUI Option3text;

    [SerializeField] private Button Option1;
    [SerializeField] private Button Option2;
    [SerializeField] private Button Option3;
    
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
