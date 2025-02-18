using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [field: SerializeField] public Database Database { get; private set;}

    bool isInitialized;

    Chances<EventInfo> _eventChances = new();
    Chances<OutcomeInfo> _outcomeChances = new();
    List<EEvent> _pastEvents = new();

    public static readonly int OUTCOME_COUNT = 3;
    public static EventManager Instance => GetInstance();
    static EventManager _instance;

    private bool onLoop = false;

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

    public EventInfo GetRandomEvent(int trialCount = -1)
    {
        if (trialCount < 0)
            trialCount = CombatController._instance.TrialCounter;

        if (!isInitialized)
            Initialize();

        _eventChances.Clear();

        for(int i = 0; i < Database.eventsTab.rowEntries.Length; i++)
        {
            int trialMin = Database.eventsTab.GetInt(i, "TrialMin");

            if (trialCount < trialMin)
            {
                //Debug.Log("Player does not qualify for " + (EEvent)i + " because " + trialCount + " < " + trialMin);
                continue;
            }

            EEvent eEvent = (EEvent)Database.eventsTab.GetEnum(typeof(EEvent), i, "EEvent");

            if (_pastEvents.Contains(eEvent))
            {
                //Debug.Log("Player did the " + eEvent + " event. (" + (EEvent)i + " == " + eEvent + ")");
                continue;
            }

            float chance = Database.eventsTab.GetFloat(i, "Chance");

            if (chance <= 0f)
            {
                //Debug.Log("The chance of " + eEvent + " is " + chance + " so we don't add it to chances.");
                continue;
            }

            EventInfo eventInfo = new()
            {
                eEvent = eEvent,
                displayName = Database.eventsTab.GetString(i, "DisplayName"),
                text = Database.eventsTab.GetString(i, "Text")
            };

            _eventChances.AddOutcome(eventInfo, chance);
            //Debug.Log("Added " + eEvent + " event @ " + chance);
        }

        if (_eventChances.Count <= 0)
        {
            if(!onLoop)
            {
                _pastEvents.Clear();
                onLoop = true;
                return GetRandomEvent(trialCount);
            }
            
            EventInfo defaultEvent = new()
            {
                eEvent = EEvent.Blacksmith,
                displayName = "Wandering Blacksmith",
                text = "You Encounter a strange traveling blacksmith. He seems willing to do business with you."
            };

            Debug.LogError("No valid events for player with TrialCount of " + trialCount + ".\n Selecting Default Event: " + defaultEvent.eEvent);
            return defaultEvent;
        }

        EventInfo selectedEvent = _eventChances.GetRandomOutcome();
        //Debug.Log("Selected Event: " + selectedEvent.eEvent);
        _pastEvents.Add(selectedEvent.eEvent);
        onLoop = false;
        return selectedEvent;
    }

    public List<OptionInfo> GetOptions(EEvent eEvent)
    {
        if (!isInitialized)
            Initialize();

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

    public OutcomeInfo GetOutcome(EOption option)
    {
        if (!isInitialized)
            Initialize();

        _outcomeChances.Clear();
        int optionRow = (int)option;

        for (int i = 0; i < OUTCOME_COUNT; i++)
        {
            string chanceColumn = "Chance" + i;
            float chance = Database.optionsTab.GetFloat(optionRow, chanceColumn);

            if (chance > 0f)
            {
                EOutcome outcome = (EOutcome)Database.optionsTab.GetEnum(typeof(EOutcome), optionRow, "Outcome" + i);

                OutcomeInfo outcomeInfo = new()
                {
                    outcome = outcome,
                    displayName = Database.outcomesTab.GetString((int)outcome, "DisplayName"),
                    value = Database.optionsTab.GetFloat(optionRow, "Value" + i),
                    sfx = Database.optionsTab.GetString(optionRow, "SFX" + i),
                    text = Database.optionsTab.GetString(optionRow, "Text" + i)
                };

                _outcomeChances.AddOutcome(outcomeInfo, chance);
                //Debug.Log("Add " + outcome + " @ " + chance.ToString("##0%"));
            }
        }

        if (_outcomeChances.Count <= 0)
        {
            Debug.LogError("No valid outcomes for option: " + option + ".\n Selecting Default Outcome: None");

            OutcomeInfo outcomeInfo = new()
            {
                outcome = EOutcome.None,
                displayName = "Non-Event",
                value = 0,
                sfx = "None",
                text = "Nothing Happens."
            };

            return outcomeInfo;
        }

        OutcomeInfo randomOutcomeInfo = _outcomeChances.GetRandomOutcome();
        //Debug.Log("Selected outcome: "+ randomOutcomeInfo.outcome);
        return randomOutcomeInfo;
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
    public string sfx;
    public string text;
    public bool success;
}