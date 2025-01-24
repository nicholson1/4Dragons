using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    EventsData[] _events;
    OptionsData[] _options;
    Chances<EEvent> _eventChances = new();
    Chances<(EOutcome, float)> _outcomeChances = new();

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

        _events = Database.eventsData;
        _options = Database.optionsData;

        isInitialized = true;
    }

    /// <param name="trialCount"></param>
    /// <returns>A valid EEvent with weighted likelyhood. If no trialCount is provided, it checks CombatController._instance.TrialCounter.</returns>
    public EEvent GetRandomEvent(int trialCount = -1)
    {
        if (trialCount < 0)
            trialCount = CombatController._instance.TrialCounter;

        if (!isInitialized)
            Initialize();

        _eventChances.Clear();

        foreach (EventsData eventsData in _events)
            if (trialCount >= eventsData.TrialMin)
                _eventChances.AddOutcome(eventsData.EEvent, eventsData.Chance);

        EEvent selectedEvent = EEvent.Blacksmith;

        if (_eventChances.Count <= 0)
            Debug.LogError("No valid events for player with TrialCount of " + trialCount + ".\n Selecting Default Event: " + selectedEvent);
        else
            selectedEvent = _eventChances.GetRandomOutcome();

        return selectedEvent;
    }

    /// <param name="eEvent"></param>
    /// <returns>1 to 4 options based on the given EEvent</returns>
    public List<EOption> GetOptions(EEvent eEvent)
    {
        List<EOption> options = new(4);
        EOption option = EOption.None;

        option = _events[(int)eEvent].Option0;

        if (option != EOption.None)
            options.Add(option);

        option = _events[(int)eEvent].Option1;

        if (option != EOption.None)
            options.Add(option);

        option = _events[(int)eEvent].Option2;

        if (option != EOption.None)
            options.Add(option);

        option = _events[(int)eEvent].Option3;

        if (option != EOption.None)
            options.Add(option);

        if (options.Count <= 0)
            options.Add(EOption.Leave);

        return options;
    }

    /// <param name="option"></param>
    /// <returns>A random outcome that comes from the given option with weighted likelyhood.</returns>
    public (EOutcome, float) GetOutcome(EOption option)
    {
        if (!isInitialized)
            Initialize();

        _outcomeChances.Clear();

        foreach (OptionsData optionsData in _options)
        {
            if (optionsData.Chance0 > 0f)
                _outcomeChances.AddOutcome((optionsData.Outcome0, optionsData.Value0), optionsData.Chance0);
            if (optionsData.Chance1 > 0f)
                _outcomeChances.AddOutcome((optionsData.Outcome1, optionsData.Value1), optionsData.Chance1);
            if (optionsData.Chance2 > 0f)
                _outcomeChances.AddOutcome((optionsData.Outcome2, optionsData.Value2), optionsData.Chance2);
        }

        (EOutcome, float) outcome = (EOutcome.None, 0f);

        if (_eventChances.Count <= 0)
            Debug.LogError("No valid outcomes for option: " + option + ".\n Selecting Default Outcome: " + outcome);
        else
            outcome = _outcomeChances.GetRandomOutcome();

        return outcome;
    }
}
