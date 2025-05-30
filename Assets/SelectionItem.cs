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
    public Equipment item;
    [SerializeField] private CombatEntity myCharacter;
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI rarity;
    [SerializeField] private TextMeshProUGUI slot;
    [SerializeField] private TextMeshProUGUI RelicDescription;

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

    public void InitializeSelectionItem(Equipment e)
    {
        RelicDescription.gameObject.SetActive(false);
        selectRelic.gameObject.SetActive(false);
        equip.gameObject.SetActive(true);
        inventory.gameObject.SetActive(true);

        equip.interactable = true;
        inventory.interactable = true;
        Cardback.SetActive(true);
        transform.rotation = Quaternion.Euler( 0,180, 0);

        _equipment = e;

        myCharacter = CombatController._instance.Player.GetComponent<CombatEntity>();

        _toolTip.is_spell = false;
        item = e;
        title.text = e.name;
        if (e.slot == Equipment.Slot.OneHander)
        {
            slot.text = "Weapon"; //+GetWeaponType(x.spellType1);
            _toolTip.is_spell = true;
        }
        else if (e.slot == Equipment.Slot.Scroll )
        {
            slot.text = "Scroll"; //+GetWeaponType(x.spellType1);
            _toolTip.is_spell = true;
            //_toolTip.is_item = false;
            statsTutorial.SetActive(false);

        }
        else
        {
            //Weapon x = (Weapon)e;
            slot.text = e.slot.ToString();
            abilityTutorial.SetActive(false);

        }
        SetRarityText(e.stats[Stats.Rarity], e);

        icon.sprite = e.icon;
        title.color = rarity.color;

        

        int count = 0;
        foreach (var kvp in e.stats)
        {
            if (kvp.Key != Stats.Rarity && kvp.Key != Stats.ItemLevel)
            {
                stats[count].UpdateValues(kvp.Key, kvp.Value);
                count += 1;
                
            }
        }
        
        for (int i = stats.Length -1; i > count-1 ; i--)
        {
            stats[i].gameObject.SetActive(false);

        }

        if (e.isWeapon)
        {
            Weapon x = (Weapon) e;
            if (x.spellType1 != SpellTypes.None)
            {
                _spellDisplay.gameObject.SetActive(true);
                _spellDisplay.UpdateValues(x.spellType1, x, myCharacter);
                //stats[count].text = x.scalingInfo1[0].ToString();
                //stats[count].color = rarity.color;
                
                
                //SpellToolTip(x.spellType1,x, count);

                // activate tool tip on stats[count]
                
                //count += 1;
            }
            if (x.spellType2 != SpellTypes.None)
            {
                //stats[count].text = x.scalingInfo1[0].ToString();
                //stats[count].color = rarity.color;
                // activate tool tip on stats[count]
                
                //SpellToolTip(x.spellType1,x, count);

                //count += 1;
            }
        }
        else
        {
            _spellDisplay.gameObject.SetActive(false);
            if(e.isPotion)
                rarityTutorial.SetActive(false);
        }
        if(e.stats.ContainsKey(Stats.ItemLevel))
            _toolTip.iLvl = e.stats[Stats.ItemLevel].ToString();
        _toolTip.rarity = e.stats[Stats.Rarity];
        _toolTip.Cost = "";
        _toolTip.Title = e.name;
        _toolTip.e = e;

        if (e.isRelic)
        {
            _toolTip.is_item = false;
            _toolTip.is_relic = true;
            selectRelic.interactable = true;
            inventory.gameObject.SetActive(false);
            equip.gameObject.SetActive(false);
            selectRelic.gameObject.SetActive(true);
            slot.color = title.color;
            Relic r = (Relic)e;
            _toolTip.Message = r.relicDescription;
            RelicDescription.text = r.relicDescription;
            RelicDescription.gameObject.SetActive(true);
            
            statsTutorial.SetActive(false);
        }

        if (e.slot == Equipment.Slot.Consumable)
        {
            _toolTip.is_item = false;
            equip.gameObject.SetActive(false);
            slot.color = title.color;
            RelicDescription.text = ((Consumable)e).description;
            RelicDescription.gameObject.SetActive(true);
        }

        if(e.stats[Stats.Rarity] != 0)
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
    IEnumerator RotateObjectBack()
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
        if (SelectionManager._instance.selectionsLeft <= 0)
        {
            SelectionManager._instance.ClearSelections();
        }



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

    public void AddToInventory()
    {
        UIController._instance.PlayUIClick();
        UIController._instance.PlayPlaceItem();
        EquipmentManager._instance.AddItemToInventoryFromSelection(item, this);
    }

    public void EquipedFromSelection()
    {
        UIController._instance.PlayUIClick();
        UIController._instance.PlayPlaceItem();
        EquipmentManager._instance.EquipItemFromSelection(item, this);
    }

    public void RemoveSelection()
    {
        // disable interation on buttons
        SelectionManager._instance.SelectionMade(this);

        
        StartCoroutine(RotateObjectBack());
        //Destroy(gameObject);
    }

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
