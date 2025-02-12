using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    public static EventUI instance;
    [SerializeField] private TextMeshProUGUI eventText;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionTexts;
    [SerializeField] private Image eventImage;
    [SerializeField] private Sprite[] eventSprites;
    [SerializeField] private bool loadEventSprites;

    EventInfo _currentEventInfo;
    List<OptionInfo> _currentOptionInfos;
    OutcomeInfo _currentOutcomeInfo;

    bool _hasSelectedOption = false;

    private void OnValidate()
    {
        if (!loadEventSprites)
            return;

        loadEventSprites = false;
        LoadEventSprites();

    }

    private void LoadEventSprites()
    {
        eventSprites = new Sprite[Enum.GetNames(typeof(EEvent)).Length];

        for (int i = 0; i < eventSprites.Length; i++)
        {
            string spritePath = "EventSprites/" + ((EEvent)i).ToString();
            Sprite sprite = Resources.Load<Sprite>(spritePath);

            if (sprite == null)
            {
                Debug.LogError("Did not find " + spritePath);
                continue;
            }

            eventSprites[i] = sprite;
        }
    }

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

            bool hasOption = _currentOptionInfos[i].option > EOption.None;
            optionButtons[i].gameObject.SetActive(hasOption);

            if(hasOption)
                optionTexts[i].text = _currentOptionInfos[i].displayName + GetOptionDetailsText(_currentOptionInfos[i].option);
            //Debug.Log(i+": "+_currentOptionInfos[i].option);
        }

        if(eventSprites == null || eventSprites.Length < (int)_currentEventInfo.eEvent)
            LoadEventSprites();

        eventImage.sprite = eventSprites[(int)_currentEventInfo.eEvent];

        // toggle ui show
        UIController._instance.ToggleEventUI(1);

        // set state
        _hasSelectedOption = false;
    }

    private string GetOptionDetailsText(EOption option)
    {
        Database database = EventManager.Instance.Database;

        if (!database.optionsTab.GetBool((int)option, "ShowOutcomes"))
            return string.Empty;

        string optionDetails = ": ";
        EOutcome[] outcomes = new EOutcome[EventManager.OUTCOME_COUNT];
        float[] chances = new float[EventManager.OUTCOME_COUNT];
        float[] values = new float[EventManager.OUTCOME_COUNT];

        for (int i = 0; i < outcomes.Length; i++) 
        {
            outcomes[i] = (EOutcome)database.optionsTab.GetEnum(typeof(EOutcome), (int)option, "Outcome"+i);
            chances[i] = database.optionsTab.GetFloat((int)option, "Chance" + i);
            values[i] = database.optionsTab.GetFloat((int)option, "Value" + i);
        }

        for (int i = 0; i < outcomes.Length; i++)        
            if (outcomes[i] > EOutcome.None && chances[i] > 0f)
            {
                float positivity = database.outcomesTab.GetFloat((int)outcomes[i], "Positivity");

                if (positivity == 0f)
                    optionDetails += "<color=yellow>";
                else if (positivity > 0f)
                    optionDetails += "<color=green>";
                else
                    optionDetails += "<color=red>";

                optionDetails += database.outcomesTab.GetString((int)outcomes[i], "DisplayName");

                if (values[i] != 0f && values[i] != 1f)
                {
                    optionDetails += " x";

                    if (values[i] > 0f && values[i] < 1f)
                        optionDetails += values[i].ToString("0.00");
                    else
                        optionDetails += values[i].ToString("##0.##");
                }

                if (chances[i] > 0f && chances[i] < 1f)
                    optionDetails += " ("+chances[i].ToString("##0%")+")";

                optionDetails += "</color>";

                for(int j = i+1; j < outcomes.Length; j++)
                {
                    if (outcomes[j] == EOutcome.None || chances[j] <= 0f)
                        continue;

                    optionDetails += ", ";
                    break;
                }
            }

        if (optionDetails.Length <= 2)
            return string.Empty;

        return optionDetails;
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
        
        UIController._instance.PlayUIClick();
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
        UIController._instance.PlayUIClick();

    }

    private void ActivateOutcome(OutcomeInfo outcomeInfo)
    {
        float value = outcomeInfo.value;
        EOutcome outcome = outcomeInfo.outcome;

        Debug.Log(outcome);
        switch (outcome)
        {
            case EOutcome.None: /* nothing happens*/ 
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.ReviveGet: /* increase revives*/
                CombatController._instance.retryAvailable += Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.ReviveLose: /* decrease revives*/
                CombatController._instance.retryAvailable -= Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.GoldGet:
                CombatController._instance.Player.GetGold(Mathf.RoundToInt(value));
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.GoldLose:
                CombatController._instance.Player.GetGold(Mathf.RoundToInt(value));
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.GetGoldPercent:
                int gainGold = Mathf.RoundToInt(CombatController._instance.Player._gold * value/100);
                CombatController._instance.Player.GetGold(gainGold);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.LoseGoldPercent:
                int lossGold = Mathf.RoundToInt(CombatController._instance.Player._gold * value/100);
                CombatController._instance.Player.GetGold(-lossGold);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.StrengthUp:
                CombatController._instance.StrengthBlessing += Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.StrengthDown:
                CombatController._instance.StrengthBlessing -= Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.SpellPowerUp:
                CombatController._instance.SpellPowerBlessing += Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.SpellPowerDown:
                CombatController._instance.SpellPowerBlessing -= Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.ArmorDown:
                CombatController._instance.ArmorBlessing -= Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.ArmorUp:
                CombatController._instance.ArmorBlessing += Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.MagicResistUp:
                CombatController._instance.MRBlessing += Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.MagicResistDown:
                CombatController._instance.MRBlessing -= Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.StartDamaged:
                CombatController._instance.startDamaged += Mathf.RoundToInt(value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.RandomRarityUp:
                for (int i = 0; i < value; i++)
                {
                    EquipmentManager._instance.EnhanceRandom();
                }
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.RandomLevelUp:
                for (int i = 0; i < value; i++)
                {
                    EquipmentManager._instance.UpgradeRandom();
                }
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.BreakWeapon:
                for (int i = 0; i < value; i++)
                {
                    EquipmentManager._instance.BreakWeapon();
                }
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.Buff:
                CombatController._instance.PreCombatBuffs.Add((CombatEntity.BuffTypes)value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.Debuff:
                CombatController._instance.PreCombatDeBuffs.Add((CombatEntity.DeBuffTypes)value);
                UIController._instance.ToggleMapUI(1);
                break;
            case EOutcome.RelicGet:
                SelectionManager._instance.CreateChestReward(false, SelectionManager.ChestType.Relic, SelectionManager.ChestType.None);
                UIController._instance.ToggleLootUI(1);
                break;
            case EOutcome.EquipmentGet:
                UIController._instance.ToggleInventoryUI(1);
                SelectionManager._instance.CreateChestReward(false, SelectionManager.ChestType.Equipment, SelectionManager.ChestType.None);
                UIController._instance.ToggleLootUI(1);
                break;
            
            /*
             * 
                BreakWeapon = 63,
                RelicGet = 3,
                Buff
             */
            

            default:Debug.LogError("Neil, please write code for the " + outcome.ToString() + " outcome."); break;
        }
    }

    public void CloseEventUI()
    {
        UIController._instance.ToggleEventUI(0);
    }
}
