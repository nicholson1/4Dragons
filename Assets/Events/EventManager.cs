using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// Use EventManager.Instance to access the following methods for Event data:
/// EventManager.Instance.GetRandomEvent(int trialCount);
/// EventManager.Instance.GetOptions(EEvent eEvent);
/// EventManager.Instance.GetOutcome(EOption option);
/// </summary>
public class EventManager : MonoBehaviour
{
    [field: SerializeField] public Database Database { get; private set;}

    bool isInitialized;

    Chances<EventInfo> _eventChances = new();
    Chances<OutcomeInfo> _outcomeChances = new();

    public static EventManager Instance => GetInstance();
    static EventManager _instance;

    private static EventManager GetInstance()
    {
        if(_instance == null)
        {
            GameObject eventManagerObject = new GameObject("EventManager");
            _instance = eventManagerObject.AddComponent<EventManager>();
        }

        return _instance;
    }

    public void Initialize()
    {
        if (Database == null)
            Database = Resources.Load<Database>("Database");

        isInitialized = true;
    }

    /// <param name="trialCount"></param>
    /// <returns>A valid EEvent with weighted likelyhood. If no trialCount is provided, it checks CombatController._instance.TrialCounter.</returns>
    public EventInfo GetRandomEvent(int trialCount = -1)
    {
        if (trialCount < 0)
            trialCount = CombatController._instance.TrialCounter;

        if (!isInitialized)
            Initialize();

        _eventChances.Clear();

        for(int i = 0; i < Database.eventsTab.rowEntries.Length; i++)
            if(trialCount >= Database.eventsTab.GetInt(i, "TrialMin"))
            {
                EventInfo eventInfo = new()
                {
                    eEvent = (EEvent)Database.eventsTab.GetEnum(typeof(EEvent), i, "EEvent"),
                    displayName = Database.eventsTab.GetString(i, "DisplayName"),
                    text = Database.eventsTab.GetString(i, "Text")
                };

                _eventChances.AddOutcome(eventInfo);
            }

        EventInfo selectedEventInfo = new()
        {
            eEvent = EEvent.Blacksmith,
            displayName = "Travelling Blacksmith",
            text = "You Encounter a strange traveling blacksmith. He seems willing to do business with you."
        };

        if (_eventChances.Count <= 0)
            Debug.LogError("No valid events for player with TrialCount of " + trialCount + ".\n Selecting Default Event: " + selectedEventInfo.eEvent);
        else
            selectedEventInfo = _eventChances.GetRandomOutcome();

        return selectedEventInfo;
    }

    /// <param name="eEvent"></param>
    /// <returns>1 to 4 options based on the given EEvent</returns>
    public List<OptionInfo> GetOptions(EEvent eEvent)
    {
        List<OptionInfo> options = new(4);
        EOption option = EOption.None;
        int row = (int)eEvent;

        for (int i = 0; i < 4; i++)
        {
            string column = "Option" + i.ToString("0");
            option = (EOption)Database.eventsTab.GetEnum(typeof(EOption), row, column);

            if (option != EOption.None)
            {
                OptionInfo addOption = new()
                {
                    option = option,
                    displayName = Database.optionsTab.GetString((int)option, "DisplayName")
                };

                options.Add(addOption);
            }
        }

        if (options.Count <= 0)
        {
            OptionInfo addOption = new()
            {
                option = EOption.Leave,
                displayName = "Leave"
            };

            options.Add(addOption);
        }

        return options;
    }

    /// <param name="option"></param>
    /// <returns>A random outcome that comes from the given option with weighted likelyhood.</returns>
    public OutcomeInfo GetOutcome(EOption option)
    {
        if (!isInitialized)
            Initialize();

        _outcomeChances.Clear();

        for (int i = 0; i < Database.eventsTab.rowEntries.Length; i++)
        {
            for(int j = 0; j < 3; j++) // up to 3 outcomes available
            {
                string chanceNumber = "Chance" + j;
                float chance = Database.eventsTab.GetFloat(i, chanceNumber);

                if (chance > 0f)
                {
                    EOutcome outcome = (EOutcome)Database.optionsTab.GetEnum(typeof(EOutcome), i, "Outcome" + j);

                    OutcomeInfo outcomeInfo = new()
                    {
                        outcome = outcome,
                        displayName = Database.outcomesTab.GetString((int)outcome, "DisplayName"),
                        value = Database.optionsTab.GetFloat(i, "Value"+j),
                        text = Database.optionsTab.GetString(i, "Text"+j)
                    };

                    _outcomeChances.AddOutcome(outcomeInfo);
                }
            }            
        }

        if (_eventChances.Count <= 0)
        {
            Debug.LogError("No valid outcomes for option: " + option + ".\n Selecting Default Outcome: None");

            OutcomeInfo outcomeInfo = new()
            {
                outcome = EOutcome.None,
                displayName = "Non-Event",
                value = 0,
                text = "Nothing Happens."
            };

            return outcomeInfo;
        }

        return _outcomeChances.GetRandomOutcome();
    }
}


public struct EventInfo
{
    public EEvent eEvent;
    public string displayName;
    public string text;
}

public struct OptionInfo
{
    public EOption option;
    public string displayName;
}

public struct OutcomeInfo
{
    public EOutcome outcome;
    public string displayName;
    public float value;
    public string text;
}