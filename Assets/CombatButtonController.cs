using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatButtonController : MonoBehaviour
{   

    [SerializeField] private SpellButton weapon1;
    [SerializeField] private SpellButton weapon2;
    [SerializeField] private SpellButton scroll1;
    [SerializeField] private SpellButton scroll2;
    [SerializeField] private Button endTurn;


    [SerializeField] private TextMeshProUGUI energyText;    

    [SerializeField] private DataReader _dataReader;
    private List<List<object>> DataTable;
    private int currentEnergy = 0;

    private Character character = null;
    private CombatEntity combatEntity => character != null ? character._combatEntity : null;

    public void CastAbility(int abilityIndex)
    {
        combatEntity.CastAbility(abilityIndex);
    }

        
    private void Start()
    {
        //CombatTrigger.TriggerCombat += UpdateCombatUI;
        //BindGamepadToCombatButton();

        
        CombatController.UpdateUIButtons += UpdateCombatUI;    

        character ??= CombatController._instance.Player;
        
        character.UpdateEnergy += UpdateEnergy;

        DataTable = _dataReader.GetWeaponScalingTable();
        if (weapon1 != null)
            weapon1.SetDataTable(DataTable);
        if (weapon2 != null)
            weapon2.SetDataTable(DataTable);
        if (scroll1 != null)
            scroll1.SetDataTable(DataTable);
        if (scroll2 != null)
            scroll2.SetDataTable(DataTable);
    }

    private void OnDestroy()
    {
        //CombatTrigger.TriggerCombat -= UpdateCombatUI;
        CombatController.UpdateUIButtons -= UpdateCombatUI;

        character.UpdateEnergy -= UpdateEnergy;
    }

    //private void Update()
    //{
        
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        if (weapon1.isSpellUsable)
    //        {
    //            combatEntity.CastAbility(0);
    //            //weapon1.TriggerButtonGlow();
    //        }
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        if (weapon2.isSpellUsable)
    //        {
    //            combatEntity.CastAbility(1);
    //            //weapon2.TriggerButtonGlow();

    //        }
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        if (scroll1.isSpellUsable)
    //        {
    //            combatEntity.CastAbility(2);
    //            //scroll1.TriggerButtonGlow();
    //        }
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        if (scroll2.isSpellUsable)
    //        {
    //            combatEntity.CastAbility(3);
    //            //scroll2.TriggerButtonGlow();

    //        }
    //    }
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        if (endTurn.isActiveAndEnabled && endTurn.interactable)
    //        {
    //            combatEntity.EndTurn();
    //        }
    //    }
    //}

    private void UpdateEnergy(Character c, int current)
    {
        //Debug.Log("This is called twice");
        if (!c.isPlayerCharacter)
        {
            return;
        }

        //Debug.Log(current + "current in comButCont");
        currentEnergy = current;
        energyText.text = current.ToString();
        //Debug.Log("2nd here here");

        //SetupSpellButtons(c);
        // if change is > 0 flash the numbers or something


        // update which spell buttons are viable

    }
    

    private void UpdateCombatUI(Character player, Character enemy)
    {
        //adjust spell names based on the weapons in the players inventory
        //Debug.Log("one here");
        SetupSpellButtons(player);
    }

    private void SetupSpellButtons(Character player)
    {
        List<(SpellTypes, Weapon)> weaponSpells = player.GetWeaponSpells();

        List<(SpellTypes, Weapon)> spellScrolls = player.GetSpells();
        // refence the datatable with these spells as int
        
        /*
        //Debug.Log(weaponSpells.Item1 + "    " + weaponSpells.Item2);

        // Debug.Log(player._weapons.Count);
        // foreach (var VARIABLE in player._weapons)
        // {
        //     Debug.Log(VARIABLE.spellType1);
        // }
        // Debug.Log(player._spellScrolls.Count);
        // foreach (var VARIABLE in player._spellScrolls)
        // {
        //     Debug.Log(VARIABLE.spellType1);
        // }
        */

        switch (spellScrolls.Count)
        {
            case 0:
                scroll1.UpdateSpell(SpellTypes.None, null);
                scroll2.UpdateSpell(SpellTypes.None, null);
                break;
            case 1:
                scroll1.UpdateSpell(spellScrolls[0].Item1, spellScrolls[0].Item2);
                scroll2.UpdateSpell(SpellTypes.None, null);
                break;
            case 2:
                scroll1.UpdateSpell(spellScrolls[0].Item1, spellScrolls[0].Item2);
                scroll2.UpdateSpell(spellScrolls[1].Item1, spellScrolls[1].Item2);
                break;
            case 3:
                scroll1.UpdateSpell(spellScrolls[0].Item1, spellScrolls[0].Item2);
                scroll2.UpdateSpell(spellScrolls[1].Item1, spellScrolls[1].Item2);
                Debug.LogWarning("WE HAVE 3 SPELLS IN SPELL SCROLLS");
                break;
                
        }

        switch (weaponSpells.Count)
        {
            case 0:
                weapon1.UpdateSpell(SpellTypes.None, null);
                weapon2.UpdateSpell(SpellTypes.None, null);
                break;
            case 1:
                weapon1.UpdateSpell(weaponSpells[0].Item1, weaponSpells[0].Item2);
                weapon2.UpdateSpell(SpellTypes.None, null);
                break;
            case 2:
                weapon1.UpdateSpell(weaponSpells[0].Item1, weaponSpells[0].Item2);
                weapon2.UpdateSpell(weaponSpells[1].Item1, weaponSpells[1].Item2);
                break;
            default:
                Debug.LogWarning("WE HAVE weird number of weapon SPELLS");
                break;
        }

        weapon1.SetUsability(player, currentEnergy);
        weapon2.SetUsability(player, currentEnergy);
        scroll1.SetUsability(player, currentEnergy);
        scroll2.SetUsability(player, currentEnergy);
        //CheckIfSpellCanBeUsed(weapon1, player);
        //CheckIfSpellCanBeUsed(weapon2, player);
        //CheckIfSpellCanBeUsed(scroll1, player);
        //CheckIfSpellCanBeUsed(scroll2, player);
        
        //check if all spells are in different spell schools (for specific relic check)
        if (CheckForUniqueSpellSchools(new[] { weapon1.spell, weapon2.spell, scroll1.spell, scroll2.spell }))
        {
            if(!RelicManager._instance.HasRelic4Buff && RelicManager._instance.CheckRelic(RelicType.Relic4))
            {
                //if we dont have the blessing give the player one
                //Debug.Log("add blessing");
                CombatController._instance.Player._combatEntity.GetHitWithBlessingDirect(CombatEntity.BlessingTypes.SpellPower, 1, 10 * CombatController._instance.Player._level);
                RelicManager._instance.HasRelic4Buff = true;
            }
        }
        else
        {
            if (RelicManager._instance.HasRelic4Buff)
            {
                //remove blessing
                //Debug.Log("remove blessing");

                int blessingIndex = CombatController._instance.Player.GetIndexOfBlessing(CombatEntity.BlessingTypes.SpellPower);
                CombatController._instance.Player._combatEntity.GetHitWithBlessingDirect(CombatEntity.BlessingTypes.SpellPower, 1, -CombatController._instance.Player.Blessings[blessingIndex].Item3 );
                RelicManager._instance.HasRelic4Buff = false;
            }
        }

    }

    private void CheckIfSpellCanBeUsed(SpellButton button, Character player)
    {
        Button b = button.GetComponent<Button>();

        if (button.spell == SpellTypes.None)
        {
            //Debug.Log(" spell is false");
            b.interactable = false;
            button.isSpellUsable = false;
            return;
        }
        int energyAmount = int.Parse(DataTable[(int)button.spell][2].ToString());
        
        
        //Debug.Log(energyAmount);
        if (energyAmount > currentEnergy)
        {
            // if relic 24 is unsued and the spell is a buff
            if(TheSpellBook._instance.IsSpellType(TheSpellBook.SpellClass.Buff, button.spell) && !RelicManager._instance.UsedRelic23 && RelicManager._instance.CheckRelic(RelicType.Relic23))
            {
                b.interactable = true;
                button.isSpellUsable = true;
            }
                
            else
            {
                b.interactable = false;
                button.isSpellUsable = false;
            }
        }

        else
        {
            b.interactable = true;
            button.isSpellUsable = true;
        }
        
        if (b.interactable && energyAmount == 1)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic10))
            {
                b.interactable = false;
                button.isSpellUsable = false;
            }
        }
    }

    private bool CheckForUniqueSpellSchools(SpellTypes[] spells)
    {
        List<string> types = new List<string>();
        foreach (var spell in spells)
        {
            if (spell == SpellTypes.None)
                return false;
            if ((int)spell < 15)
                return false;

            string s = spell.ToString().Substring(0, 3);
            if (!types.Contains(s))
            {
                types.Add(s);
            }
            else
            {
                return false;
            }

        }

        return true;
    }
    private bool CheckForUniquePhysicalSchools(SpellTypes[] spells)
    {
        List<string> types = new List<string>();
        foreach (var spell in spells)
        {
            if (spell == SpellTypes.None)
                return false;
            if ((int)spell > 14)
                return false;

            string s = spell.ToString().Substring(0, 3);
            if (!types.Contains(s))
            {
                types.Add(s);
            }
            else
            {
                return false;
            }

        }

        return true;
    }
    
}
