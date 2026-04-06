using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SelectionItem : MonoBehaviour
{
    public event Action<SelectionItem> OnSelectionItemPanelClosed;
    public event Action<SelectionItem> OnSelectionItemPanelSelected;

    public Button MainButton => mainButton;
    public List<Button> CurrentActiveGamepadButtons => currentActiveGamepadButtons;
    public bool IsPanelSelected => isPanelSelected;
    
    public Equipment item;
    [SerializeField] private CombatEntity myCharacter;
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI rarity;
    [SerializeField] private TextMeshProUGUI slot;
    [SerializeField] private TextMeshProUGUI RelicDescription;
    [SerializeField] private TextMeshProUGUI ScrollDescription;
    [SerializeField] private TextMeshProUGUI ScrollEnergyDescription;

    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private StatDisplay[] stats;

    [SerializeField] private Button equip;
    [SerializeField] private Button  inventory;
    [SerializeField] private Button  selectRelic;

    [SerializeField] private ToolTip _toolTip;
    [SerializeField] private Image icon;

    public bool isFlipping = false;
    //[SerializeField] private ToolTip[] SpellToolTips;
    [SerializeField] private SpellDisplay _spellDisplay;
    [SerializeField] private GameObject Cardback;
    [SerializeField] private Image CardFront;

    [SerializeField] private AudioClip[] randomCard;
    [SerializeField] private float cardVol;
    private Equipment _equipment;

    [SerializeField] private GameObject abilityTutorial;
    [SerializeField] private GameObject rarityTutorial;
    [SerializeField] private GameObject statsTutorial;    

    public bool isAvailable = true;

    [SerializeField] private List<Button> currentActiveGamepadButtons = new List<Button>();
    private Button mainButton = null;
    private Button skipButton = null;

    private PanelHoverEffect panelHoverEffect = null;
    private bool isPanelSelected = false;
    private bool isEquipment = false;

    #region Initialization
    public void InitializeSelectionItem(Equipment equipmentToSet)
    {
        panelHoverEffect ??= GetComponent<PanelHoverEffect>();
        SetDescriptionActive(equipmentToSet);

        SetButtonActive(equipmentToSet);

        SetCardBack();

        SetPanelMainProperties(equipmentToSet);

        SetTexts(equipmentToSet);

        //statsTutorial.SetActive(false); identify tutorial related calls later
        //at this point, statsTutorial and abilityTutorial was set to false

        PopulateStatsForDisplay(equipmentToSet);

        SetSpellDisplay(equipmentToSet); //there's a rarityTutorial.SetActive(false) here

        SetTooltipValues(equipmentToSet);

        if (equipmentToSet is Relic or Consumable)
        {
            SetExtraDescription(equipmentToSet as Relic);
            statsTutorial.SetActive(false);
        }
        else
            rarityTutorial.SetActive(false);

        SetupVerticalNavigation();
        
        isAvailable = true;

        StartCoroutine(RotateObjectForward());
    }

    public void SetSkipButton(Button button) => skipButton = button;

    public void DeinitializeSelectionItem()
    {

        Destroy(this.gameObject);
    }

    private void SetDescriptionActive(Equipment equipmentToSet)
    {
        isEquipment = equipmentToSet is not Relic or Consumable;
        RelicDescription.gameObject.SetActive(equipmentToSet is Relic);
        _spellDisplay.gameObject.SetActive(equipmentToSet is Weapon);
        ScrollDescription.gameObject.SetActive(equipmentToSet.slot == Equipment.Slot.Scroll);
        ScrollEnergyDescription.transform.parent.gameObject.SetActive(equipmentToSet.slot == Equipment.Slot.Scroll || equipmentToSet is Weapon); //parent is the EnergyDisplay content holder
    }

    private void SetButtonActive(Equipment equipmentToSet)
    {
        equip.onClick.RemoveListener(EquipedFromSelection);
        inventory.onClick.RemoveListener(AddToInventory);
        selectRelic.onClick.RemoveListener(SelectRelic);
        selectRelic.onClick.RemoveListener(AddToInventory);

        if (equipmentToSet.isRelic)
        {
            equip.gameObject.SetActive(false);
            inventory.gameObject.SetActive(false);
            equip.interactable = false;
            inventory.interactable = false;

            selectRelic.gameObject.SetActive(true);
            selectRelic.interactable = true;
            selectRelic.onClick.AddListener(SelectRelic);
        }
        else if(equipmentToSet.isPotion)
        {
            equip.gameObject.SetActive(false);
            inventory.gameObject.SetActive(false);
            equip.interactable = false;
            inventory.interactable = false;

            selectRelic.gameObject.SetActive(true);
            selectRelic.interactable = true;
            selectRelic.onClick.AddListener(AddToInventory);
        }
        else
        {
            selectRelic.gameObject.SetActive(false);
            selectRelic.interactable = false;

            equip.gameObject.SetActive(true);
            inventory.gameObject.SetActive(true);
            equip.interactable = true;
            inventory.interactable = true;
            equip.onClick.AddListener(EquipedFromSelection);
            inventory.onClick.AddListener(AddToInventory);
        }
        //selectRelic.gameObject.SetActive(equipmentToSet is Relic);
        //inventory.gameObject.SetActive(equipmentToSet is not Relic);
        //equip.gameObject.SetActive(equipmentToSet is not Relic and not Consumable);

        //selectRelic.interactable = equipmentToSet is Relic;
        //inventory.interactable = equipmentToSet is not Relic;
        //equip.interactable = equipmentToSet is not Relic and not Consumable;

        mainButton = equipmentToSet is Relic or Consumable ? selectRelic : equip;
    }

    private void SetCardBack()
    {
        Cardback.SetActive(true);
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    private void SetPanelMainProperties(Equipment equipmentToSet)
    {
        myCharacter = CombatController._instance.Player.GetComponent<CombatEntity>();
        _equipment = equipmentToSet;
        item = equipmentToSet;

        icon.sprite = equipmentToSet.icon;
        title.color = rarity.color;

        if (equipmentToSet.stats[Stats.Rarity] != 0)
            CardFront.color = title.color;
    }

    private bool HasSpell(Equipment equipmentToSet)
    {
        return equipmentToSet.slot is Equipment.Slot.OneHander or ImportantStuff.Equipment.Slot.Scroll;
    }

    private void SetTexts(Equipment equipmentToSet)
    {
        title.text = equipmentToSet.name;
        SetRarityText(equipmentToSet.stats[Stats.Rarity], equipmentToSet);

        //Slot text
        //Weapon and Scroll slot exception
        if (equipmentToSet.slot == Equipment.Slot.OneHander)
            slot.text = "Weapon"; 

        else if (equipmentToSet.slot == Equipment.Slot.Scroll)
            slot.text = "Scroll";            

        else
            slot.text = equipmentToSet.slot.ToString();

        if (equipmentToSet is Relic or Consumable)
            slot.color = title.color;

    }

    private void PopulateStatsForDisplay(Equipment equipmentToSet)
    {
        int count = 0;
        foreach (var kvp in equipmentToSet.stats)
        {
            if (kvp.Key != Stats.Rarity && kvp.Key != Stats.ItemLevel)
            {
                stats[count].UpdateValues(kvp.Key, kvp.Value);
                count++;
            }
        }
        
        for (int i = stats.Length - 1; i > count - 1; i--)
        {
            stats[i].gameObject.SetActive(false);
        }
    }

    private string GetEnergyCost(Weapon weaponToSet)
    {
        List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
        string Cost = DataTable[(int)weaponToSet.spellType1][2].ToString();
        return Cost;
    }

    private void SetSpellDisplay(Equipment equipmentToSet)
    {
        if(equipmentToSet is not Weapon)
        {
            _spellDisplay.gameObject.SetActive(false);
            return;
        }

        if (equipmentToSet is Consumable)
        {
            rarityTutorial.SetActive(false); //does this have to be called here?
            return;
        }

        Weapon weaponToSet = equipmentToSet as Weapon;                

        ScrollEnergyDescription.text = GetEnergyCost(weaponToSet);
        _spellDisplay.UpdateValues(weaponToSet.spellType1, weaponToSet, myCharacter);

        if (equipmentToSet.slot == Equipment.Slot.Scroll)       
            ScrollDescription.text = TheSpellBook._instance.GetScrollDescription(weaponToSet.spellType1);
    }

    private void SetTooltipValues(Equipment equipmentToSet)
    {
        _toolTip.is_item = equipmentToSet is not Relic or Consumable;
        _toolTip.is_spell = HasSpell(equipmentToSet);
        _toolTip.is_relic = equipmentToSet is Relic;

        if (equipmentToSet.stats.ContainsKey(Stats.ItemLevel))
            _toolTip.iLvl = equipmentToSet.stats[Stats.ItemLevel].ToString();

        _toolTip.rarity = equipmentToSet.stats[Stats.Rarity];
        _toolTip.Cost = "";
        _toolTip.Title = equipmentToSet.name;
        _toolTip.e = equipmentToSet;
    }

    private void SetExtraDescription(Equipment equipmentToSet)
    {
        string description = string.Empty;

        if (equipmentToSet is Relic)
        {
            description = (equipmentToSet as Relic).relicDescription;
        }
        else if(equipmentToSet is Consumable)
        {
            description = (equipmentToSet as Consumable).description;
        }

        _toolTip.Message = description;
        RelicDescription.text = description;        
    }

    private void ClearLastActiveGamepadButtons()
    {
        if (currentActiveGamepadButtons.Count <= 0) return;

        foreach (var button in currentActiveGamepadButtons)
        {
            var gamepadButtonListener = button.GetComponentInParent<IGamepadButtonListener>();
            if (gamepadButtonListener != null)
                gamepadButtonListener.OnGamepadButtonSelected -= CheckSelectedStatus;
        }
        currentActiveGamepadButtons.Clear();
    }

    private void SetupVerticalNavigation()
    {
        ClearLastActiveGamepadButtons();
        currentActiveGamepadButtons = GetComponentsInChildren<Button>().Where(b => b.gameObject.activeSelf).ToList();
        
        for (int i=0; i < currentActiveGamepadButtons.Count; i++)
        {
            Selectable selectable = currentActiveGamepadButtons[i];
            Navigation navi = selectable.navigation;
            navi.mode = Navigation.Mode.Explicit;

            navi.selectOnUp = i > 0 ? currentActiveGamepadButtons[i - 1] : null;                       
            navi.selectOnDown = i < currentActiveGamepadButtons.Count - 1 ? currentActiveGamepadButtons[i + 1] : skipButton;

            selectable.navigation = navi;
        }

        SetupGamepadButtonListener();
    }

    private void CheckSelectedStatus()
    {
        if (isPanelSelected) return;

        OnSelectionItemPanelSelected?.Invoke(this);
    }

    private void SetupGamepadButtonListener()
    {
        var gamepadButtonListener = mainButton.GetComponent<IGamepadButtonListener>();
        if (gamepadButtonListener == null)
        {
            Debug.LogError($"main button for {this.item.name} panel doesn't have a IGamepadButtonListener!");
        }
        gamepadButtonListener.OnGamepadButtonSelected += CheckSelectedStatus;

    }
   
    #endregion

    IEnumerator RotateObjectForward()
    {
        PlayRandomCardFlip();

        bool halfway = false;
    
        float angle = 180;
        do {
            angle -= 200 * Time.deltaTime;
            if (angle < 0)
            {
                angle = 0; // clamp

                if (_equipment.slot == Equipment.Slot.Scroll || _equipment.slot == Equipment.Slot.OneHander)
                {
                    TutorialManager.Instance.QueueTip(TutorialNames.Abilities);
                }
                if (_equipment.slot != Equipment.Slot.Relic && _equipment.stats[Stats.Rarity] > 0)
                {
                    TutorialManager.Instance.QueueTip(TutorialNames.EquipmentRarity);
                }
                if (_equipment.slot != Equipment.Slot.Relic && _equipment.stats.Count > 2)
                {
                    TutorialManager.Instance.QueueTip(TutorialNames.Stats);
                }
                
            }

            if (angle <= 90 && halfway == false)
            {
                halfway = true;
                Cardback.SetActive(false);
                
            }
            
        
            transform.rotation = Quaternion.Euler( 0,angle, 0);
            yield return null;
        } while( angle > 0);        
    }

    IEnumerator RotateObjectBack(Action onFinish = null)
    {
        isFlipping = true;
        bool halfway = false;
    
        PlayRandomCardFlip();
        float angle = 0;
        do {
            angle += 200 * Time.deltaTime;
            if (angle > 180)
            {
                angle = 180; // clamp                
            }

            if (angle >= 90 && halfway == false)
            {
                halfway = true;
                Cardback.SetActive(true);
            }
            
        
            transform.rotation = Quaternion.Euler( 0,angle, 0);
            yield return null;
        } while( angle < 180);

        isFlipping = false;

        onFinish?.Invoke();
    }

    private void FinishedRotateBackCallback()
    {
        isAvailable = false;
        
        OnSelectionItemPanelClosed?.Invoke(this);
    }

    private void SetRarityText(int r, Equipment e)
    {
        rarity.text = "";

        switch (r)
        {
            case 0:
                //rarity.text = "Common";
                rarity.color = ToolTipManager._instance.rarityColors[0];
                break;
            case 1:
                //rarity.text = "Uncommon";
                rarity.color = ToolTipManager._instance.rarityColors[1];

                break;
            case 2:
                //rarity.text = "Rare";
                rarity.color = ToolTipManager._instance.rarityColors[2];

                break;
            case 3:
                //rarity.text = "Epic";
                rarity.color = ToolTipManager._instance.rarityColors[3];

                break;
            case 4:
                //rarity.text = "relic";
                rarity.color = ToolTipManager._instance.rarityColors[4];

                break;
            case -1 :
                rarity.text = "";
                break;
            
        }
        if(e.stats.ContainsKey(Stats.ItemLevel))
            rarity.text += " Lvl: " + e.stats[Stats.ItemLevel];
    }
    
    // public void UpdateToolTipWeapon(SpellTypes s, Weapon w)
    // {
    //
    //    
    //     List<int> power = TheSpellBook._instance.GetPowerValues(s, w, myCharacter);
    //
    //     List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
    //
    //     _toolTip.Title = DataTable[(int)s][0].ToString();;
    //     _toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
    //     _toolTip.Cost = DataTable[(int)s][2].ToString();
    //     
    //     //iLVL
    //     int a;
    //     w.stats.TryGetValue(Stats.ItemLevel, out a);
    //     _toolTip.iLvl = a.ToString();
    //     //Rarity
    //     int r;
    //     w.stats.TryGetValue(Stats.Rarity, out r);
    //     _toolTip.rarity = r;
    //     
    //     
    //     
    //     
    // }
    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        message = message.Replace("$", turns.ToString());
        message = message.Replace("@", amount.ToString());
        message = message.Replace("#", (Mathf.RoundToInt(amount/4)*4).ToString());
        
        return message;

    }

    private void BindMainButtons(bool toBind)
    {
        if(item is Relic or Consumable)
            selectRelic.GetComponent<ButtonBindingHandler>().ManualBindInput(toBind);
        else
        {
            equip.GetComponent<ButtonBindingHandler>().ManualBindInput(toBind);
            inventory.GetComponent<ButtonBindingHandler>().ManualBindInput(toBind);
        }
    }




    public void SelectPanel()
    {
        if (isPanelSelected) return;

        isPanelSelected = true;
        panelHoverEffect.ScaleUp();

        BindMainButtons(true);
    }

    public void DeselectPanel()
    {
        if (!isPanelSelected || !isAvailable) return;

        isPanelSelected = false;
        panelHoverEffect.ScaleDown();

        BindMainButtons(false);
    }

    #region Editor bind function
    public void SelectRelic()
    {
        //should behave similar to AddToInventory()
        isAvailable = false;

        DisableButtons();

        UIController._instance.PlayGetRelic();
        StatsTracker.Instance.TrackRelicSelected(item);

        // add to character  
        CombatController._instance.Player._Relics.Add(item);
        //remove relic from seen relic list
        RelicManager._instance.SelectRelic(item);

        //clear selections
        StartCoroutine(RotateObjectBack(FinishedRotateBackCallback));   
    }

    public void AddToInventory()
    {
        isAvailable = false;

        DisableButtons();

        bool canAddToInventory = EquipmentManager._instance.TryPutItemToInventoryFromSelection(item, this);
        if (canAddToInventory)
        {
            UIController._instance.PlayPlaceItem();
            StartCoroutine(RotateObjectBack(FinishedRotateBackCallback));
        }
        else
        {
            Debug.Log($"Should handle item can't go to inventory here");
            //timed-interactable popup to instruct player to clean the inventory?
        }
    }

    public void EquipedFromSelection()
    {
        isAvailable = false;

        DisableButtons();

        bool canEquipItem = EquipmentManager._instance.TryEquipItemFromSelection(item, this);
        if (canEquipItem)
        {
            UIController._instance.PlayPlaceItem();
            StartCoroutine(RotateObjectBack(FinishedRotateBackCallback));
        }
        else
        {
            Debug.Log($"Should handle cannot equip item here");
        }
    }
    #endregion

    //public void RemoveSelection()
    //{
    //    // disable interation on buttons
    //    SelectionManager._instance.SelectionMade(this);

    //    StartCoroutine(RotateObjectBack());
    //    //Destroy(gameObject);
    //}

    public void DisableButtons()
    {
        equip.interactable = false;
        inventory.interactable = false;
        selectRelic.interactable = false;
    }

    public void PlayRandomCardFlip()
    {
        SoundManager.Instance.Play2DSFX(randomCard[Random.Range(0, randomCard.Length)], cardVol, 1, .05f);
    }

    // public void SpellToolTip(SpellTypes s, Weapon w, int index)
    // {
    //     
    //     SpellToolTips[index].gameObject.SetActive(true);
    //     List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
    //     List<int> power = TheSpellBook._instance.GetPowerValues(s, w, myCharacter);
    //
    //     //Debug.Log(w.name + "--------------");
    //
    //
    //     //tt.enabled = true;
    //
    //
    //     SpellToolTips[index].Title = DataTable[(int)s][0].ToString();;
    //     SpellToolTips[index].Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
    //     SpellToolTips[index].Cost = DataTable[(int)s][2].ToString();
    //     
    //     //iLVL
    //     int a;
    //     w.stats.TryGetValue(Stats.ItemLevel, out a);
    //     SpellToolTips[index].iLvl = a.ToString();
    //     //Rarity
    //     int r;
    //     w.stats.TryGetValue(Stats.Rarity, out r);
    //     SpellToolTips[index].rarity = r;
    //     
    //     //Debug.Log("we did the things?");
    //
    //     
    // }

    private string GetWeaponType(SpellTypes spell)
    {
        string name = "";
        switch (spell)
        {
            case SpellTypes.Dagger1:
                name = "Dagger";
                break;
            case SpellTypes.Dagger2:
                name = "Dagger";
                break;
            case SpellTypes.Shield1:
                name = "Shield";
                break;
            case SpellTypes.Shield2:
                name = "Shield";
                break;
            case SpellTypes.Sword1:
                name = "Sword";
                break;
            case SpellTypes.Sword2:
                name = "Sword";
                break;
            case SpellTypes.Axe1:
                name = "Axe";
                break;
            case SpellTypes.Axe2:
                name = "Axe";
                break;
            case SpellTypes.Hammer1:
                name = "Hammer";
                break;
            case SpellTypes.Hammer2:
                name = "Hammer";
                break;
            case SpellTypes.Nature1:
                name = "Nature";
                break;
            case SpellTypes.Nature2:
                name = "Nature";
                break;
            case SpellTypes.Nature3:
                name = "Nature";
                break;
            case SpellTypes.Nature4:
                name = "Nature";
                break;
            case SpellTypes.Fire1:
                name = "Fire";
                break;
            case SpellTypes.Fire2:
                name = "Fire";
                break;
            case SpellTypes.Fire3:
                name = "Fire";
                break;
            case SpellTypes.Fire4:
                name = "Fire";
                break;
            case SpellTypes.Ice1:
                name = "Ice";
                break;
            case SpellTypes.Ice2:
                name = "Ice";
                break;
            case SpellTypes.Ice3:
                name = "Ice";
                break;
            case SpellTypes.Ice4:
                name = "Ice";
                break;
            case SpellTypes.Blood1:
                name = "Blood";
                break;
            case SpellTypes.Blood2:
                name = "Blood";
                break;
            case SpellTypes.Blood3:
                name = "Blood";
                break;
            case SpellTypes.Blood4:
                name = "Blood";
                break;
            case SpellTypes.Shadow1:
                name = "Shadow";
                break;
            case SpellTypes.Shadow2:
                name = "Shadow";
                break;
            case SpellTypes.Shadow3:
                name = "Shadow";
                break;
            case SpellTypes.Shadow4:
                name = "Shadow";
                break;
            }

        return name;

    }
}
