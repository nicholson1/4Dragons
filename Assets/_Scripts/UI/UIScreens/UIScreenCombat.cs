using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScreenCombat : UIScreen
{
    //we might need button assigned for toggling Combat and Potion mode

    //this screen should be considered !navigatable during CombatUINavigationMoce.Combat
    

    private CombatButtonController combatButtonController;

    private CombatUINavigationMode currentCombatNavigation = CombatUINavigationMode.Combat;

    private void BindCombatActionButtons()
    {

    }

    public void SetCombatUINavigation(CombatUINavigationMode mode)
    {
        inputHandler ??= EventSystem.current.GetComponent<InputHandler>();
        switch(mode)
        {
            case CombatUINavigationMode.Combat:
                inputHandler.SwitchActionMap(ActionMaps.Combat);
                currentCombatNavigation = CombatUINavigationMode.Combat;
                break;
            case CombatUINavigationMode.Potion:
                //Set potion buttons navigation in runtime now based on which slots are navigatable

                inputHandler.SwitchActionMap(ActionMaps.Menu);
                currentCombatNavigation = CombatUINavigationMode.Potion;
                break;
            case CombatUINavigationMode.Inspect:
                //set buttons navigation in runtime based on which icons are navigatable
                //relics
                //abilities
                //buffs&debuffs
                //enemy intentions
                //items

                inputHandler.SwitchActionMap(ActionMaps.Combat);
                currentCombatNavigation = CombatUINavigationMode.Inspect;
                break;


        }
    }
}

public enum CombatUINavigationMode
{
    Combat,
    Inspect,
    Potion,
    Tutorial,
    Disabled
}
