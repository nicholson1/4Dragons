using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatButtonController : MonoBehaviour
{
    [SerializeField] private SpellButton weapon1;
    [SerializeField] private SpellButton weapon2;
    [SerializeField] private SpellButton scroll1;
    [SerializeField] private SpellButton scroll2;

    [SerializeField] private TextMeshProUGUI energyText;
    

    [SerializeField] private DataReader _dataReader;
    private List<List<object>> DataTable;
    private int currentEnergy = 0;
    private void Start()
    {
        //CombatTrigger.TriggerCombat += UpdateCombatUI;
        Character.UpdateEnergy += UpdateEnergy;
        CombatController.UpdateUIButtons += UpdateCombatUI;
        
        DataTable = _dataReader.GetWeaponScalingTable();
        if(weapon1 != null)
            weapon1.SetDataTable(DataTable);
        if(weapon2 != null)
            weapon2.SetDataTable(DataTable);
        if(scroll1 != null)
            scroll1.SetDataTable(DataTable);
        if(scroll2 != null)
            scroll2.SetDataTable(DataTable);
    }

    private void OnDestroy()
    {
        //CombatTrigger.TriggerCombat -= UpdateCombatUI;
        CombatController.UpdateUIButtons -= UpdateCombatUI;

        Character.UpdateEnergy -= UpdateEnergy;
    }

    private void UpdateEnergy(Character c, int current, int max, int change)
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

        UpdateSpellButtons(c);
        // if change is > 0 flash the numbers or something


        // update which spell buttons are viable

    }
    

    private void UpdateCombatUI(Character player, Character enemy)
    {
        //adjust spell names based on the weapons in the players inventory
        //Debug.Log("one here");
        UpdateSpellButtons(player);
    }

    private void UpdateSpellButtons(Character player)
    {
        List<(Weapon.SpellTypes, Weapon)> weaponSpells = player.GetWeaponSpells();
        //(Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) spellScolls = player.GetScollSpells();

        List<(Weapon.SpellTypes, Weapon)> spellScrolls = player.GetSpells();
        // refence the datatable with these spells as int
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

        switch (spellScrolls.Count)
        {
            case 0:
                scroll1.UpdateSpell(Weapon.SpellTypes.None, null);
                scroll2.UpdateSpell(Weapon.SpellTypes.None, null);
                break;
            case 1:
                scroll1.UpdateSpell(spellScrolls[0].Item1, spellScrolls[0].Item2);
                scroll2.UpdateSpell(Weapon.SpellTypes.None, null);
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
                weapon1.UpdateSpell(Weapon.SpellTypes.None, null);
                weapon2.UpdateSpell(Weapon.SpellTypes.None, null);
                break;
            case 1:
                weapon1.UpdateSpell(weaponSpells[0].Item1, weaponSpells[0].Item2);
                weapon2.UpdateSpell(Weapon.SpellTypes.None, null);
                break;
            case 2:
                weapon1.UpdateSpell(weaponSpells[0].Item1, weaponSpells[0].Item2);
                weapon2.UpdateSpell(weaponSpells[1].Item1, weaponSpells[1].Item2);
                break;
            default:
                Debug.LogWarning("WE HAVE weird number of weapon SPELLS");
                break;
        }
        

        CheckIfSpellCanBeUsed(weapon1, player);
        CheckIfSpellCanBeUsed(weapon2, player);
        CheckIfSpellCanBeUsed(scroll1, player);
        CheckIfSpellCanBeUsed(scroll2, player);

    }

    private void CheckIfSpellCanBeUsed(SpellButton button, Character player)
    {
        Button b = button.GetComponent<Button>();

        if (button.spell == Weapon.SpellTypes.None)
        {
            //Debug.Log(" spell is false");
            b.interactable = false;
            return;
        }
        int energyAmount = int.Parse(DataTable[(int)button.spell][2].ToString());
        
        
        
        //Debug.Log(energyAmount);
        if (energyAmount > currentEnergy)
        {
            b.interactable = false;
        }

        else
        {
            b.interactable = true;
        }
    }
}
