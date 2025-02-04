using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    public static EventUI instance;
    [SerializeField] private TextMeshProUGUI eventText;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionTexts;

    EventInfo _currentEventInfo;
    List<OptionInfo> _currentOptionInfos;
    OutcomeInfo _currentOutcomeInfo;

    bool _hasSelectedOption = false;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

        EventManager.Instance.Initialize();
    }

    public void RandomEvent()
    {
        // ensure any randomness is based on our current seed
        Random.InitState(CombatController._instance.LastNodeClicked.nodeSeed);

        // select which event to show
        _currentEventInfo = EventManager.Instance.GetRandomEvent();

        //fill out the info
        eventTitle.text = _currentEventInfo.displayName;
        eventText.text = _currentEventInfo.text;

        _currentOptionInfos = EventManager.Instance.GetOptions(_currentEventInfo.eEvent);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if(i >= _currentOptionInfos.Count)
            {
                //Debug.Log(i + " >= "+_currentOptionInfos.Count+ " so set inactive");
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            optionButtons[i].gameObject.SetActive(_currentOptionInfos[i].option > EOption.None);
            optionTexts[i].text = _currentOptionInfos[i].displayName;
            //Debug.Log(i+": "+_currentOptionInfos[i].option);
        }

        // toggle ui show
        UIController._instance.ToggleEventUI(1);

        // set state
        _hasSelectedOption = false;
    }

    public void ClickOption(int buttonIndex)
    {
        if(_hasSelectedOption)
        {
            AcceptConsequences();
            return;
        }

        // check option

        if (buttonIndex > _currentOptionInfos.Count)
        {
            Debug.LogError("Clicked impossible button: " + buttonIndex);
            buttonIndex %= _currentOptionInfos.Count;
        }

        EOption clickedOption = _currentOptionInfos[buttonIndex].option;

        // generate outcome
        _currentOutcomeInfo = EventManager.Instance.GetOutcome(clickedOption);

        // display outcome
        eventText.text = _currentOutcomeInfo.text;
        optionTexts[buttonIndex].text = _currentOutcomeInfo.displayName;

        for (int i = 0; i < optionButtons.Length; i++)
            optionButtons[i].gameObject.SetActive(i == buttonIndex);

        // set state
        _hasSelectedOption = true;
    }

    private void AcceptConsequences()
    {
        // Make Outcome happen
        ActivateOutcome(_currentOutcomeInfo);

        // Close UI
        CloseEventUI();

        // Set state
        _hasSelectedOption = false;

        // Continue the game flow
        //Debug.LogError("Neil, make the game keep moving here");
        CombatController._instance.SetMapCanBeClicked(true);
        UIController._instance.ToggleMapUI(1);
    }

    private void ActivateOutcome(OutcomeInfo outcomeInfo)
    {
        float value = outcomeInfo.value;
        EOutcome outcome = outcomeInfo.outcome;

        switch (outcome)
        {
            case EOutcome.None: /* nothing happens*/ 
                break;
            case EOutcome.ReviveGet: /* increase revives*/
                CombatController._instance.retryAvailable += Mathf.RoundToInt(value);
                break;
            case EOutcome.ReviveLose: /* decrease revives*/
                CombatController._instance.retryAvailable -= Mathf.RoundToInt(value);
                break;
            case EOutcome.GoldGet:
                CombatController._instance.Player.GetGold(Mathf.RoundToInt(value));
                break;
            case EOutcome.GoldLose:
                CombatController._instance.Player.GetGold(Mathf.RoundToInt(value));
                break;
            case EOutcome.GetGoldPercent:
                int gainGold = Mathf.RoundToInt(CombatController._instance.Player._gold * value/100);
                CombatController._instance.Player.GetGold(gainGold);
                break;
            case EOutcome.LoseGoldPercent:
                int lossGold = Mathf.RoundToInt(CombatController._instance.Player._gold * value/100);
                CombatController._instance.Player.GetGold(-lossGold);
                break;
            case EOutcome.StrengthUp:
                CombatController._instance.StrengthBlessing += Mathf.RoundToInt(value);
                break;
            case EOutcome.StrengthDown:
                CombatController._instance.StrengthBlessing -= Mathf.RoundToInt(value);
                break;
            case EOutcome.SpellPowerUp:
                CombatController._instance.SpellPowerBlessing += Mathf.RoundToInt(value);
                break;
            case EOutcome.SpellPowerDown:
                CombatController._instance.SpellPowerBlessing -= Mathf.RoundToInt(value);
                break;
            case EOutcome.ArmorDown:
                CombatController._instance.ArmorBlessing -= Mathf.RoundToInt(value);
                break;
            case EOutcome.ArmorUp:
                CombatController._instance.ArmorBlessing += Mathf.RoundToInt(value);
                break;
            case EOutcome.MagicResistUp:
                CombatController._instance.MRBlessing += Mathf.RoundToInt(value);
                break;
            case EOutcome.MagicResistDown:
                CombatController._instance.MRBlessing -= Mathf.RoundToInt(value);
                break;
            

            default:Debug.LogError("Neil, please write code for the " + outcome.ToString() + " outcome."); break;
        }
    }

    public void CloseEventUI()
    {
        UIController._instance.ToggleEventUI(0);
    }
}
