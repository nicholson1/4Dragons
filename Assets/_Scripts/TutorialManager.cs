using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using PlayFab.MultiplayerModels;
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
    public static event Action<TutorialNames> CloseTutorial;


    public bool showingTip = false;

    [SerializeField] private MoveAndSpin guide;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);

        // Initialize the tip dictionary
        tipDictionary = new Dictionary<TutorialNames, TutorialTip>();
        foreach (var tip in TutorialTips)
        {
            tipDictionary[tip.ID] = tip;
        }

        TutorialsEnabled = PlayerPrefsManager.GetTutorialEnabled() == 1;
    }

    private float timer = 3f;
    private void FixedUpdate()
    {
        if (!showingTip && _tutorialNamesQueue.Count > 0)
        {
            timer -= Time.fixedDeltaTime;

            if (timer <= 0)
            {
                ShowTip();
                timer = 3f;
            }
        }
    }

    public void ToggleTutorialEnabled()
    {
        TutorialsEnabled = !TutorialsEnabled;
        if(TutorialsEnabled)
            PlayerPrefsManager.SetTutorialEnabled(1);
        else
            PlayerPrefsManager.SetTutorialEnabled(0);

    }

    public void QueueTip(TutorialNames tipID)
    {
        if (!TutorialsEnabled || !tipDictionary.ContainsKey(tipID)) return;
        
        if(tipDictionary[tipID].IsShown) return;
        
        if(_tutorialNamesQueue.Contains(tipID)) return;

        if (tipID == TutorialNames.SkipSelection)
        {
            tipDictionary[tipID].IsShown = true;
            if (TriggerTutorial != null) TriggerTutorial(tipID);
            return;
        }

        _tutorialNamesQueue.Enqueue(tipID);
        
        if(!showingTip)
            ShowTip();
    }

    public void DequeueTipOnSuccess(TutorialNames tipID)
    {
        if(_tutorialNamesQueue.Count == 0)
            return;
        if (tipID == _tutorialNamesQueue.Peek())
        {

            _tutorialNamesQueue.Dequeue();
        }
    }

    public void MoveSprite(TutorialNames tipID)
    {
        if(tipID != TutorialNames.Cleanse)
            guide.MoveToRandom();
    }

    public void ShowTip()
    {
        if(_tutorialNamesQueue.Count == 0)
        {
            showingTip = false;
            return;
        }
        TutorialNames tipID  =_tutorialNamesQueue.Peek();
        var tip = tipDictionary[tipID];
        //Debug.Log($"Tutorial Tip: {tip.Message}");
        tip.IsShown = true;
        tipDictionary[tipID] = tip;

        if (TriggerTutorial != null) TriggerTutorial(tipID);
    }
    
    
    public void CloseTip(TutorialNames tipID)
    {
        if (CloseTutorial != null) CloseTutorial(tipID);
    }

    public string GetText(TutorialNames id)
    {
        var tip = tipDictionary[id];
        return tip.Message;
    }
    
    public bool CheckIsShown(TutorialNames id)
    {
        var tip = tipDictionary[id];
        return tip.IsShown;
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
    Abilities,
    Cleanse,
    Start,
    Stats,

    
}
