using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatButtonController : MonoBehaviour
{
    [SerializeField] private SpellButton weapon1;
    [SerializeField] private SpellButton weapon2;
    [SerializeField] private SpellButton scroll1;
    [SerializeField] private SpellButton scroll2;

    [SerializeField] private DataReader _dataReader;
    private List<List<object>> DataTable;

    private void Start()
    {
        CombatTrigger.TriggerCombat += UpdateCombatUI;
        DataTable = _dataReader.GetWeaponScalingTable();
        weapon1.SetDataTable(DataTable);
        weapon2.SetDataTable(DataTable);
        scroll1.SetDataTable(DataTable);
        scroll2.SetDataTable(DataTable);
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
        Debug.Log(weaponSpells.Item1 + "    " + weaponSpells.Item2);

        weapon1.UpdateSpell(weaponSpells.Item1, weaponSpells.Item3);
        weapon2.UpdateSpell(weaponSpells.Item2, weaponSpells.Item4);
        scroll1.UpdateSpell(spellScolls.Item1, spellScolls.Item3);
        scroll2.UpdateSpell(spellScolls.Item2, spellScolls.Item4);
        

        



    }
}
