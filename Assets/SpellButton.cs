using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ImportantStuff;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellButton : MonoBehaviour, IGamepadButtonListener
{    
    public SpellTypes spell;
    public Weapon weapon;
    public Image SpellIcon;
    public TextMeshProUGUI SpellText;
   
    [SerializeField]private ToolTip _toolTip;
    //public static event Action<CombatEntity, SpellTypes, int, int> AttackWithSpell;

    public bool isSpellUsable = true;

    //[SerializeField] private DataReader dataReader;
    List<List<object>> DataTable;

    private Character character;
    private Button button;
    private ButtonGlow buttonGlow;
    private RectTransform rt;

    private bool isSpellReady = false;

    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    public void SetDataTable(List<List<object>> WeaponScalingTable )
    {        
       DataTable = WeaponScalingTable;
    }

    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        if (!isSpellUsable)
        {
            return;
        }

        ReadyCastingSpell();
    }

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        CancelCastingSpell();
    }

    public void HandleGamepadButtonPressed(Selectable selectable)
    {
        if (!isSpellReady) return;
            
        InitiateCastSpell();
    }

    private void ReadyCastingSpell()
    {
        if (isSpellReady) return;

        _toolTip.ShowTipFromGamepadNavi(rt);
        isSpellReady = true;
    }

    private void CancelCastingSpell()
    {
        if (!isSpellReady) return;

        _toolTip.CloseTip();
        isSpellReady = false;
    }

    private void InitiateCastSpell()
    {
        //close tip
        if (!isSpellUsable)
        {
            //handle spell not usable effects?
            
            return;
        }

        _toolTip.CloseTip();
        character._combatEntity.CastTheAbility(spell, weapon);
        buttonGlow.TriggerEffect(_toolTip.IconColor);
        EventSystem.current.SetSelectedGameObject(null);
    }


            
    public void UpdateSpell(SpellTypes s, Weapon w)
    {
        spell = s;
        if (spell == SpellTypes.None)
        {
            _toolTip.icon =  null;
            _toolTip.IconColor = new Color(0,0,0,0);

            _toolTip.Message = "You have no Item Equipped in this slot";
            _toolTip.Title = "None";
            _toolTip.IconColor = Color.white;
            //_toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
            _toolTip.Cost = "";
        
            //iLVL
            //int a;
            //w.stats.TryGetValue(Stats.ItemLevel, out a);
            _toolTip.iLvl = "";
            //Rarity
            
            _toolTip.rarity = -1;

            _toolTip.e = null;
            SpellText.text = "None";
            _toolTip.is_spell = false;
            return;

        }
        else
        {
            this.GetComponent<Button>().interactable = true;
            _toolTip.is_spell = true;
        }
        List<int> power = TheSpellBook._instance.GetPowerValues(s, w, character._combatEntity);

        SpellText.text = DataTable[(int)spell][0].ToString();

        //Debug.Log(w.name + "--------------");


        weapon = w;
        
        //Debug.Log(weapon.name);

        string t = "";
        foreach (var i in DataTable[(int)spell])
        {
            t += i.ToString() + ", ";
        }
        //Debug.Log(t);

        (string, Sprite, Color, string) info = StatDisplayManager._instance.GetValuesFromSpell(s);
        
        spell = s;
        
        //Debug.Log(toolTip);
        _toolTip.icon =  info.Item2;
        _toolTip.IconColor = info.Item3;

        _toolTip.Message = info.Item4;
        _toolTip.Title = DataTable[(int)s][0].ToString();;
        
        _toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
        _toolTip.Cost = DataTable[(int)s][2].ToString();
        
        //iLVL
        int a;
        w.stats.TryGetValue(Stats.ItemLevel, out a);
        _toolTip.iLvl = a.ToString();
        //Rarity
        int r;
        w.stats.TryGetValue(Stats.Rarity, out r);
        _toolTip.rarity = r;

        _toolTip.e = w;


        (string, Sprite, Color, string) iconInfo = StatDisplayManager._instance.GetValuesFromSpell(s);
        SpellIcon.sprite = iconInfo.Item2;
        SpellIcon.color = iconInfo.Item3;
        SpellText.color =iconInfo.Item3;

        //Debug.Log(SpellText.text = DataTable[(int)spell][0].ToString());


        // get name and scaling from the type of spell, and the table, adjust the description via..... idk


    }

    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        //turns
        message = message.Replace("$", turns.ToString());
        //amount
        message = message.Replace("@", amount.ToString());
        //secondary amount
        string tempAmount = Mathf.RoundToInt(amount/2f).ToString();
        //Debug.Log("temp amount" + tempAmount);
        message = message.Replace("#", tempAmount);
        
        //Debug.Log($"amount = {amount}, amount/2 = {Mathf.RoundToInt(amount/2f)} ");
        //Debug.Log(message);
        return message;

    }

    public void ShowSpell()
    {

        //Debug.Log(weapon.name + "--------------");

        //Debug.Log(GetSpellDescription(spell));
    }

    public void DoSpell(CombatEntity target)
    {
        
    }
    
    private string GetSpellDescription(SpellTypes spell)
    {
        return DataTable[spell.GetHashCode()].Last().ToString() + "\n" + weapon.name + "\n Level:" +
               weapon.stats[Stats.ItemLevel] + "\n Rarity:" + weapon.stats[Stats.Rarity];
    }


    private void ActivateButton()
    {
        isSpellUsable = true;
        button.interactable = true;
    }

    private void DeactivateButton()
    {
        isSpellUsable = false;
        button.interactable = false;
    }


    public void SetUsability(Character player, int currentEnergy)
    {
        if (spell == SpellTypes.None)
        {
            //Debug.Log(" spell is false");
            DeactivateButton();
            return;
        }

        int requiredEnergy = int.Parse(DataTable[(int)spell][2].ToString());

        if (requiredEnergy > currentEnergy)
        {
            // if relic 24 is unsued and the spell is a buff
            if (TheSpellBook._instance.IsSpellType(TheSpellBook.SpellClass.Buff, spell) && !RelicManager._instance.UsedRelic23 && RelicManager._instance.CheckRelic(RelicType.Relic23))
            {
                ActivateButton();
            }
            else
            {
                DeactivateButton();
            }
        }

        else
        {
            ActivateButton();
        }

        //what's this for?
        if (isSpellUsable && requiredEnergy == 1)
        {
            if (RelicManager._instance.CheckRelic(RelicType.DragonRelic10))
            {
                DeactivateButton();
            }
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonGlow = GetComponentInChildren<ButtonGlow>();
        rt = GetComponent<RectTransform>();

        character = CombatController._instance.Player;
        character.UpdateEnergy += SetUsability;
    }

    


    private void OnDestroy()
    {
        character.UpdateEnergy -= SetUsability;
    }

    //private void OnEnable()
    //{
    //    button.onClick.AddListener(InitiateCastSpell);
    //}

    //private void OnDisable()
    //{
    //    button.onClick.RemoveListener(InitiateCastSpell);
    //}


}
