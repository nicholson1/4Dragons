using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScreenCombat : UIScreen
{
    //we might need button assigned for toggling Combat and Potion mode

    //this screen should be considered !navigatable during CombatUINavigationMoce.Combat
    public event Action<CombatUINavigationMode> OnCombatUINavigationChanged;

    private CombatButtonController combatButtonController;

    private CombatUINavigationMode currentCombatNavigation = CombatUINavigationMode.Combat;

    [ContextMenu("Log Selectables")]
    private void LogSelectables()
    {
        foreach(var s in selectables)
        {
            int index = 0;
            Debug.Log($"selectable on CombatScreen - {index} - {s.gameObject.name}");
            index++;
        }
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

        OnCombatUINavigationChanged?.Invoke(currentCombatNavigation);
    }

    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate(false);

        SetCombatUINavigation(CombatUINavigationMode.Combat);
    }

    protected override void HandleTutorialClosed(TutorialNames tutorial)
    {
        SetNavigatable(false);

        if (inputHandler.CurrentActionMap != ActionMaps.Combat)
            inputHandler.SwitchActionMap(ActionMaps.Combat);
    }

    private void Awake()
    {
        combatButtonController = GetComponentInParent<CombatButtonController>();
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
