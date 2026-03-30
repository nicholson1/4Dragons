using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenCombat : UIScreen
{
    //we might need button assigned for toggling Combat and Potion mode

    //this screen should be considered !navigatable during CombatUINavigationMoce.Combat
    public event Action<CombatEntity> OnTargetSelected;
    public event Action<CombatUINavigationMode> OnCombatUINavigationChanged;

    private CombatButtonController combatButtonController;

    private CombatController combatController;
    private CombatUINavigationMode currentCombatNavigationMode = CombatUINavigationMode.Combat;

    private Character player;

    private List<PotionDrag> activePotions = new List<PotionDrag>();
    private List<TargettingButtonListener> targettingButtonListeners = new List<TargettingButtonListener>();

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

    public void SetCombatUINavigationMode(CombatUINavigationMode mode)
    {
        currentCombatNavigationMode = mode;
        switch(currentCombatNavigationMode)
        {
            case CombatUINavigationMode.Combat:
                inputHandler.SwitchActionMap(ActionMaps.Combat);
                EventSystem.current.SetSelectedGameObject(null);
                break;
            case CombatUINavigationMode.Potion:
                //Set potion buttons navigation in runtime now based on which slots are navigatable
                inputHandler.SwitchActionMap(ActionMaps.Menu);
                HandlePotionSelectionNavigation();
                break;
            case CombatUINavigationMode.Targetting:
                inputHandler.SwitchActionMap(ActionMaps.Menu);                
                break;
            case CombatUINavigationMode.Inspect:
                //set buttons navigation in runtime based on which icons are navigatable
                //relics
                //abilities
                //buffs&debuffs
                //enemy intentions
                //items
                inputHandler.SwitchActionMap(ActionMaps.Menu);
                break;
            case CombatUINavigationMode.Disabled:
                inputHandler.SwitchActionMap(ActionMaps.Menu);
                break;

        }

        EventSystem.current.sendNavigationEvents = currentCombatNavigationMode != CombatUINavigationMode.Combat;
        OnCombatUINavigationChanged?.Invoke(currentCombatNavigationMode);
    }



    public void TogglePotionMode()
    {
        if (player == null)
        {
            Debug.LogError($"Error Toggling to PotionMode: Player NOT found!");
            return;
        }

        //can only switch between combat and potion!
        if (currentCombatNavigationMode is CombatUINavigationMode.Potion)
        {
            SetCombatUINavigationMode(CombatUINavigationMode.Combat);
        }
        else if(currentCombatNavigationMode is CombatUINavigationMode.Combat && PlayerHasPotion())
        {
            SetCombatUINavigationMode(CombatUINavigationMode.Potion);
            inputHandler.OnNo.AddListener(HandleCancelPressed);
        }
        else
        {
            Debug.Log($"CombatUINavigationMode is not in Potion or Combat, this function won't do anything!");
        }        
    }

    public void RegisterActivePotionDrag(PotionDrag potion)
    {
        if(activePotions.Contains(potion))
        {
            Debug.LogError($"Error: PotionDrag to register was already registered!");
            return;
        }

        activePotions.Add(potion);

        HandlePotionSelectionNavigation();
    }

    

    public void RemoveActivePotionDrag(PotionDrag potion)
    {
        if(!activePotions.Contains(potion))
        {
            Debug.LogError($"Error: PotionDrag to remove doesn't exist in the activePotionDrag list!");
            return;
        }

        activePotions.Remove(potion);
        HandlePotionSelectionNavigation();
    }

    private bool PlayerHasPotion()
    {
        return activePotions.Count > 0;
    }

    private void HandlePotionSelectionNavigation()
    {
        for(int i = 0; i<activePotions.Count; i++)
        {
            var potion = activePotions[i];
            var button = potion.GamepadButton;
            var navi = button.navigation;
            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            navi.selectOnLeft = i - 1 >= 0 ? activePotions[i - 1].GamepadButton : null;
            navi.selectOnRight = i + 1 < activePotions.Count ? activePotions[i + 1].GamepadButton : null;

            button.navigation = navi;
        }

        EventSystem.current.SetSelectedGameObject(activePotions[0].GamepadButton.gameObject);
    }
       

    public void InitiatePotionTargetting(PotionDrag potion)
    {
        RegisterTargettingListeners();
        HandleTargetSelectionNavigation(potion);
        SetCombatUINavigationMode(CombatUINavigationMode.Targetting);

        inputHandler.OnNo.AddListener(HandleCancelPressed);        
    }

    private void HandleTargetSelectionNavigation(PotionDrag potion)
    {
        for (int i = 0; i < targettingButtonListeners.Count; i++)
        {
            var listener = targettingButtonListeners[i];
            listener.InitializeButton(potion);

            var button = listener.GamepadButton;
            var navi = button.navigation;
            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            navi.selectOnLeft = i - 1 >= 0 ? targettingButtonListeners[i - 1].GamepadButton : null;
            navi.selectOnRight = i + 1 < targettingButtonListeners.Count ? targettingButtonListeners[i + 1].GamepadButton : null;

            button.navigation = navi;
        }

        EventSystem.current.SetSelectedGameObject(targettingButtonListeners[0].GamepadButton.gameObject);
    }

    private void HandleCancelPressed()
    {
        SetCombatUINavigationMode(CombatUINavigationMode.Combat);
        inputHandler.OnNo.RemoveListener(HandleCancelPressed);
    }

    protected override void HandleTutorialClosed(TutorialNames tutorial)
    {
        SetNavigatable(false);

        if (inputHandler.CurrentActionMap != ActionMaps.Combat)
            inputHandler.SwitchActionMap(ActionMaps.Combat);
    }

    private void RegisterTargettingListeners()
    {
        targettingButtonListeners.Clear();
        HealthBar[] healthBars = FindObjectsOfType<HealthBar>(); 

        foreach(var bar in healthBars)
        {
            var buttonListener = bar.GetComponentInChildren<TargettingButtonListener>();

            if(buttonListener != null)
            {
                targettingButtonListeners.Add(buttonListener);
            }
        }
    }

    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate(navigatableOnActivated);

        inputHandler ??= EventSystem.current.GetComponent<InputHandler>();
        inputHandler.OnL2.AddListener(TogglePotionMode);

        SetCombatUINavigationMode(CombatUINavigationMode.Combat);

    }

    public override void Deactivate()
    {
        SetCombatUINavigationMode(CombatUINavigationMode.Disabled);
        base.Deactivate();

        inputHandler.OnL2.RemoveListener(TogglePotionMode);

    }

    private void Awake()
    {
        combatButtonController = GetComponentInParent<CombatButtonController>();
        combatController ??= CombatController._instance;
    }

    private void OnEnable()
    {
        player ??= CombatController._instance.Player;
    }

}

public enum CombatUINavigationMode
{
    Combat,
    Inspect,
    Potion,
    Targetting,
    Tutorial,
    Disabled
}
