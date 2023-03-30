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
        
        if (!c.isPlayerCharacter)
        {
            return;
        }

        currentEnergy = current;
        energyText.text = current.ToString();
        UpdateSpellButtons(c);
        // if change is > 0 flash the numbers or something


        // update which spell buttons are viable

    }
    

    private void UpdateCombatUI(Character player, Character enemy)
    {
        //adjust spell names based on the weapons in the players inventory
        UpdateSpellButtons(player);
    }

    private void UpdateSpellButtons(Character player)
    {
        (Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) weaponSpells = player.GetWeaponSpells();
        (Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) spellScolls = player.GetScollSpells();

        // refence the datatable with these spells as int
        //Debug.Log(weaponSpells.Item1 + "    " + weaponSpells.Item2);

        //Debug.Log("heeeey");
        weapon1.UpdateSpell(weaponSpells.Item1, weaponSpells.Item3);
        weapon2.UpdateSpell(weaponSpells.Item2, weaponSpells.Item4);
        scroll1.UpdateSpell(spellScolls.Item1, spellScolls.Item3);
        scroll2.UpdateSpell(spellScolls.Item2, spellScolls.Item4);
     
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
