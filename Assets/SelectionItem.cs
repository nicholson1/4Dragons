using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SelectionItem : MonoBehaviour
{
    public event Action<SelectionItem> OnSelectionItemSelected;

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

    public bool available = true;

    private void SetDescriptionActive(Equipment equipmentToSet)
    {
        RelicDescription.gameObject.SetActive(equipmentToSet is Relic);
        ScrollDescription.gameObject.SetActive(equipmentToSet.slot == Equipment.Slot.Scroll);
        ScrollEnergyDescription.transform.parent.gameObject.SetActive(equipmentToSet.slot == Equipment.Slot.Scroll);
    }

    private void SetButtonActive(Equipment equipmentToSet)
    {
        selectRelic.gameObject.SetActive(equipmentToSet is Relic);
        inventory.gameObject.SetActive(equipmentToSet is not Relic);
        equip.gameObject.SetActive(equipmentToSet is not Relic and not Consumable);

        selectRelic.interactable = equipmentToSet is Relic;
        inventory.interactable = equipmentToSet is not Relic;
        equip.interactable = equipmentToSet is not Relic and not Consumable;
    }

    private void SetCardBack()
    {
        Cardback.SetActive(true);
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    private void SetThisItemProperties(Equipment equipmentToSet)
    {
        myCharacter = CombatController._instance.Player.GetComponent<CombatEntity>();
        _equipment = equipmentToSet;
        item = equipmentToSet;

        icon.sprite = equipmentToSet.icon;
        title.color = rarity.color;        
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

        if (weaponToSet.spellType1 != SpellTypes.None)
        {
            _spellDisplay.gameObject.SetActive(true);
            _spellDisplay.UpdateValues(weaponToSet.spellType1, weaponToSet, myCharacter);
        }
        if (weaponToSet.spellType2 != SpellTypes.None) //do we have spellType2? any extra handling?
        {
            //stats[count].text = x.scalingInfo1[0].ToString();
            //stats[count].color = rarity.color;
            // activate tool tip on stats[count]

            //SpellToolTip(x.spellType1,x, count);

            //count += 1;
        }
        if (equipmentToSet.slot == Equipment.Slot.Scroll)
        {
            List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
            string Cost = DataTable[(int)weaponToSet.spellType1][2].ToString();
            ScrollEnergyDescription.text = Cost;
            
            ScrollDescription.text = TheSpellBook._instance.GetScrollDescription(weaponToSet.spellType1);
        }
        else
        {
            _spellDisplay.gameObject.SetActive(false);
            if (equipmentToSet.isPotion)
                rarityTutorial.SetActive(false);
        }
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

    public void InitializeSelectionItem(Equipment equipmentToSet)
    {
        SetDescriptionActive(equipmentToSet);

        SetButtonActive(equipmentToSet);

        SetCardBack();

        SetThisItemProperties(equipmentToSet);

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

        if(equipmentToSet.stats[Stats.Rarity] != 0)
            CardFront.color = title.color;
        else
            rarityTutorial.SetActive(false);
                
        StartCoroutine(RotateObjectForward());        
    }

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

        //move the handling of this to SelectionManager
        //if (SelectionManager._instance.selectionsLeft <= 0)
        //{
        //    SelectionManager._instance.ClearSelections();
        //}

        onFinish?.Invoke();
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

    public void SelectRelic()
    {
        available = false;
        UIController._instance.PlayUIClick();
        UIController._instance.PlayGetRelic();
        StatsTracker.Instance.TrackRelicSelected(item);

        //clear selections
        SelectionManager._instance.ClearSelections();
        // add to character
        CombatController._instance.Player._Relics.Add(item);
        //remove relic from seen relic list
        RelicManager._instance.SelectRelic(item);        
    }

    private void FinishedRotateBackCallback()
    {
        OnSelectionItemSelected?.Invoke(this);
        DisableButtons();
    }

    public void AddToInventory()
    {
        UIController._instance.PlayPlaceItem();

        bool canAddToInventory = EquipmentManager._instance.TryPutItemToInventoryFromSelection(item, this);
        if (canAddToInventory)
        {
            Debug.Log($"successfully add item to the inventory");
            StartCoroutine(RotateObjectBack(FinishedRotateBackCallback));
        }
        else
        {
            Debug.Log($"Should handle item can't go to inventory here");
        }
    }

    public void EquipedFromSelection()
    {
        UIController._instance.PlayPlaceItem();

        bool canEquipItem = EquipmentManager._instance.TryEquipItemFromSelection(item, this);
        if (canEquipItem)
        {
            Debug.Log($"successfully equip item from selection!");
            StartCoroutine(RotateObjectBack(FinishedRotateBackCallback));
        }
        else
        {
            Debug.Log($"Should handle cannot equip item here");
        }
    }

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
