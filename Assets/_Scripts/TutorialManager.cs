using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;  // Singleton instance
    public List<TutorialTip> TutorialTips;   // List of all tutorial tips
    public bool TutorialsEnabled = true;    // Toggle for tutorials

    private Dictionary<TutorialNames, TutorialTip> tipDictionary;

    private Queue<TutorialNames> _tutorialNamesQueue = new Queue<TutorialNames>();
    public static event Action<TutorialNames> TriggerTutorial;

    private bool showingTip = false;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // Initialize the tip dictionary
        tipDictionary = new Dictionary<TutorialNames, TutorialTip>();
        foreach (var tip in TutorialTips)
        {
            tipDictionary[tip.ID] = tip;
        }
    }
    
    

    public void QueueTip(TutorialNames tipID)
    {
        if (!TutorialsEnabled || !tipDictionary.ContainsKey(tipID)) return;
        
        if(tipDictionary[tipID].IsShown) return;
        
        if(_tutorialNamesQueue.Contains(tipID)) return;
        
        _tutorialNamesQueue.Enqueue(tipID);
        
        if(!showingTip)
            ShowTip();
    }

    public void ShowTip()
    {
        if(_tutorialNamesQueue.Count == 0)
        {
            showingTip = false;
            return;
        }
        showingTip = true;
        TutorialNames tipID  =_tutorialNamesQueue.Dequeue();
        var tip = tipDictionary[tipID];
        Debug.Log($"Tutorial Tip: {tip.Message}");
        tip.IsShown = true;
        tipDictionary[tipID] = tip;
        
        TriggerTutorial(tipID);
        
    }

    public string GetText(TutorialNames id)
    {
        var tip = tipDictionary[id];
        return tip.Message;
    }


    public void ResetTips()
    {
        foreach (var tip in tipDictionary.Values)
        {
            tip.IsShown = false;
        }
    }
}
[System.Serializable]
public class TutorialTip
{
    public TutorialNames ID;  // Unique identifier for the tip
    public string Message;  // The content of the tip
    public bool IsShown;  // Whether the tip has been displayed
    //public bool IsMandatory;  // Should this tip always be shown
}

public enum TutorialNames
{
    Energy,
    SkipSelection,
    EquipmentLevel,
    EquipmentRarity,
    Relics,
    HealthBar,
    Intents,
    Potions,
    
}
