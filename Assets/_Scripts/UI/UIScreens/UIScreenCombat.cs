using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenCombat : UIScreen
{
    //we might need button assigned for toggling Combat and Potion mode

    //this screen should be considered !navigatable during CombatUINavigationMoce.Combat
    public event Action<CombatEntity> OnTargetSelected;
    public event Action<CombatUINavigationMode> OnCombatUINavigationChanged;
    public CombatUINavigationMode CurrentCombatNavigation => currentCombatNavigationMode;

    private CombatController combatController;
    private CombatUINavigationMode currentCombatNavigationMode = CombatUINavigationMode.Combat;

    private Character player;
    private Character currentEnemy;

    public PotionHolder[] potionHolders = new PotionHolder[6];
    private List<PotionHolder> activePotionHolders = new List<PotionHolder>();
    private List<HealthBar> currentHealthbars = new List<HealthBar>();
    private List<TargettingButtonListener> targettingButtonListeners = new List<TargettingButtonListener>();

    [SerializeField] private Button endTurn;
    [SerializeField] private Button skipButton;
    [SerializeField] private Transform playerStatsParent, enemyStatsParent, enemyIntentsParent, relicHolder;

    [SerializeField] private List<Button> playerActionButtons = new List<Button>();
    private List<IInspectableElement> playerStats = new List<IInspectableElement>();
    private List<IInspectableElement> enemyStats = new List<IInspectableElement>();
    private List<IInspectableElement> enemyIntents = new List<IInspectableElement>();
    private List<IInspectableElement> relicDisplays = new List<IInspectableElement>();

    #region Inspect Mode related
    //hook this to inspectmode button, make inspect mode button only visible when last input detected = gamepad
    public void ToggleInspectMode()
    {
        if (!player._combatEntity.isMyTurn)
            return;

        //only go to inspect mode from and to combat
        if (currentCombatNavigationMode == CombatUINavigationMode.Combat)
        {            
            SetCombatUINavigationMode(CombatUINavigationMode.Inspect);
            inputHandler.OnNo.AddListener(HandleCancelPressed);
        }
        else if (currentCombatNavigationMode == CombatUINavigationMode.Inspect)
        {

            HandleCancelPressed();
        }
    }

    private List<IInspectableElement> GetInspectableElements(Transform parent)
    {
        if (parent == null || parent.childCount == 0)
            return new List<IInspectableElement>(); //empty list

        var elements = parent.GetComponentsInChildren<IInspectableElement>(false).ToList();
        SetGroupNavigation(elements);

        return elements;
    }

    private void SetupInspectModeNavigation()
    {        
        //Populate inspectable elements
        playerStats = GetInspectableElements(playerStatsParent);
        enemyStats = GetInspectableElements(enemyStatsParent);
        enemyIntents = GetInspectableElements(enemyIntentsParent);
        relicDisplays = GetInspectableElements(relicHolder);
        
        //connect each group
        var lastButtonTargetRight = FirstButton(enemyStats) ?? (FirstButton(enemyIntents) ?? FirstButton(relicDisplays));
        if(lastButtonTargetRight != null)
            LinkGroupLastButton(LastButton(playerStats), lastButtonTargetRight);

        var firstButtonTargetLeft = LastButton(playerStats) ?? playerActionButtons[0];
        LinkGroupFirstButton(FirstButton(enemyIntents), firstButtonTargetLeft);
        LinkGroupFirstButton(FirstButton(enemyStats), firstButtonTargetLeft);
     

        var statsTargetUp = FirstButton(enemyIntents) ?? FirstButton(relicDisplays);
        LinkVerticalGroup(playerStats, statsTargetUp, null);
        LinkVerticalGroup(enemyStats, statsTargetUp, null);

        var intentsTargetUp = FirstButton(relicDisplays);
        var intentsTargetDown = FirstButton(enemyStats) ?? FirstButton(playerStats);
        LinkVerticalGroup(enemyIntents, intentsTargetUp, intentsTargetDown);

        var relicTargetDown = FirstButton(enemyIntents) ?? (FirstButton(enemyStats) ?? FirstButton(playerStats));
        LinkVerticalGroup(relicDisplays, null, relicTargetDown);

        var firstSelectable = GetFirstInspectableToSelect();

        if(firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private Selectable GetFirstInspectableToSelect()
    {
        return FirstButton(relicDisplays)
            ?? FirstButton(playerStats)
            ?? FirstButton(enemyStats)
            ?? FirstButton(enemyIntents)
            ?? playerActionButtons[0];
    }

    private Selectable FirstButton(List<IInspectableElement> group)
    {
        return group != null && group.Count > 0 ? group[0].GetGamepadButton() : null;
    }

    private Selectable LastButton(List<IInspectableElement> group)
    {
        return group != null && group.Count > 0 ? group[group.Count - 1].GetGamepadButton() : null;
    }

    private void LinkGroupFirstButton(Selectable buttonToLink, Selectable targetLink)
    {
        if (buttonToLink == null) 
            return;

        var navi = buttonToLink.navigation;
        navi.selectOnLeft = targetLink;
        buttonToLink.navigation = navi;

        if(targetLink == playerActionButtons[0])
        {
            LinkActionButtonsToLeftMostInspectable(buttonToLink);
        }
    }

    private void LinkActionButtonsToLeftMostInspectable(Selectable selectable)
    {
        foreach(var actionButton in playerActionButtons)
        {
            var actionButtonNavi = actionButton.navigation;
            actionButtonNavi.selectOnRight = selectable;
            actionButton.navigation = actionButtonNavi;
        }
    }

    private void LinkGroupLastButton(Selectable buttonToLink, Selectable targetLink)
    {
        if (buttonToLink == null) return;

        var navi = buttonToLink.navigation;
        navi.selectOnRight = targetLink;
        buttonToLink.navigation = navi;
    }

    private void LinkVerticalGroup(List<IInspectableElement> elementGroup, Selectable targetUp, Selectable targetDown)
    {
        if (elementGroup == null || elementGroup.Count == 0)
            return;

        foreach(var element in elementGroup)
        {
            var button = element.GetGamepadButton();
            if (button == null) continue;

            var navi = button.navigation;
            navi.selectOnUp = targetUp;
            navi.selectOnDown = targetDown;
            button.navigation = navi;
        }
    }

    private void SetGroupNavigation(List<IInspectableElement> elements)
    {
        if (elements.Count < 1) return;

        for (int i=0; i<elements.Count; i++)
        {
            var element = elements[i];
            var button = element.GetGamepadButton();
            var navi = button.navigation;

            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            navi.selectOnLeft = i - 1 >= 0 ? elements[i-1].GetGamepadButton() : null;
            navi.selectOnRight = i + 1 < elements.Count ? elements[i + 1].GetGamepadButton() : null;

            button.navigation = navi;
        }
    }


    #endregion

    public void SetCombatUINavigationMode(CombatUINavigationMode mode)
    {
        endTurn.interactable = false;
        currentCombatNavigationMode = mode;
        switch(currentCombatNavigationMode)
        {
            case CombatUINavigationMode.Combat:
                inputHandler.SwitchActionMap(ActionMaps.Combat);
                endTurn.interactable = true;
                EventSystem.current.SetSelectedGameObject(null);
                break;
            case CombatUINavigationMode.Potion:
                //Set potion buttons navigation in runtime now based on which slots are navigatable
                inputHandler.SwitchActionMap(ActionMaps.Menu);

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
                SetupInspectModeNavigation();
                break;
            case CombatUINavigationMode.PostCombat:

                break;
            case CombatUINavigationMode.Disabled:
                inputHandler.SwitchActionMap(ActionMaps.Menu);
                break;

        }

        //EventSystem.current.sendNavigationEvents = currentCombatNavigationMode != CombatUINavigationMode.Combat;
        OnCombatUINavigationChanged?.Invoke(currentCombatNavigationMode);
    }


    #region Potion Mode Related


    private void TogglePotionMode()
    {
        if (player == null)
        {
            Debug.LogError($"Error Toggling to PotionMode: Player NOT found!");
            return;
        }

        if (!player._combatEntity.isMyTurn)
            return;

        //can only switch between combat and potion!
        if (currentCombatNavigationMode == CombatUINavigationMode.Potion)
        {
            HandleCancelPressed();
        }
        else if(currentCombatNavigationMode == CombatUINavigationMode.Combat && PlayerHasPotion())
        {
            SetCombatUINavigationMode(CombatUINavigationMode.Potion);
            HandlePotionSelectionNavigation();
            BindPotionButtons();
            inputHandler.OnNo.AddListener(HandleCancelPressed);
        }
      
    }

    private void StorePotionInEmptyHolder(PotionDrag potion)
    {
        foreach(var holder in potionHolders)
        {
            if (holder.IsOccupied) continue;

            holder.StorePotion(potion);
            
            return;
        }

        Debug.LogError($"POTION ERROR: something wrong, there should be only maximum 6 potions in inventory. probably we haven't remove used or sold potions");

    }

    public void RegisterActivePotion(PotionDrag potion)
    {
        if (potionHolders.Any(ph => ph.Potion == potion))
            return;
       
        StorePotionInEmptyHolder(potion);
        activePotionHolders = potionHolders.Where(ph => ph.Potion != null).ToList();

        HandlePotionSelectionNavigation();
    }
    

    public void RemoveActivePotion(PotionDrag potion)
    {
        activePotionHolders.RemoveAll(ph => ph.Potion == potion);               

        HandlePotionSelectionNavigation();
        HandleCancelPressed();
    }

    private bool PlayerHasPotion()
    {
        return activePotionHolders.Count > 0;
    }

    private void BindPotionButtons()
    {
        foreach(var potion in activePotionHolders)
        {
            Debug.LogError($"binding button for {potion.name}");
            var bindingHandler = potion.GetComponentInChildren<ButtonBindingHandler>();
            bindingHandler.ManualBindInput(false);
            bindingHandler.ManualBindInput(true);
        }
    }

    private void HandlePotionSelectionNavigation()
    {
        if(activePotionHolders.Count < 1)
        {
            return;
        }

        for(int i = 0; i < activePotionHolders.Count; i++)
        {
            var potionHolder = activePotionHolders[i];
            var selectable = potionHolder.GetComponentInChildren<Selectable>();
                        
            var navi = selectable.navigation;
            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            int prevIndex = i - 1;
            int nextIndex = i + 1;
            var prevSelectable = prevIndex >= 0 ? activePotionHolders[prevIndex].GetComponentInChildren<Selectable>() : null;
            var nextSelectable = i + 1 < activePotionHolders.Count ? activePotionHolders[nextIndex].GetComponentInChildren<Selectable>() : null;
            navi.selectOnLeft = prevSelectable;
            navi.selectOnRight = nextSelectable;

            selectable.navigation = navi;
        }

        EventSystem.current.SetSelectedGameObject(activePotionHolders[0].GetComponentInChildren<Selectable>().gameObject);
    }

           

    public void InitiateGamepadPotionTargetting(PotionDrag potion)
    {
        Debug.LogError($"UIScreenCombat: InitiatePotionTargetting for {potion.potion.name}");
        RegisterTargettingListeners();
        SetCombatUINavigationMode(CombatUINavigationMode.Targetting);
        HandleTargetSelectionNavigation(potion);



        //inputHandler.OnNo.AddListener(HandleCancelPressed);        
    }

    public void SetMousePotionTargettingMode(bool toOn, PotionDrag potion = null)
    {
        if (player == null)
        {
            Debug.LogError($"Error Toggling to PotionMode: Player NOT found!");
            return;
        }

        if (toOn)
        {
            if (currentCombatNavigationMode != CombatUINavigationMode.Combat) return;

            RegisterTargettingListeners();
            SetCombatUINavigationMode(CombatUINavigationMode.Targetting);
            HandleTargetSelectionNavigation(potion);
            //inputHandler.OnNo.AddListener(HandleCancelPressed);
        }
        else
        {
            if (currentCombatNavigationMode != CombatUINavigationMode.Targetting) return;

            HandleCancelPressed();
        }
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

    private void RegisterTargettingListeners()
    {
        targettingButtonListeners.Clear();

        foreach (var bar in currentHealthbars)
        {
            var buttonListener = bar.GetComponentInChildren<TargettingButtonListener>();

            if (buttonListener != null)
            {
                targettingButtonListeners.Add(buttonListener);
            }
        }

        foreach (var buttonListener in targettingButtonListeners)
        {
            var bindHandler = buttonListener.GetComponentInChildren<ButtonBindingHandler>();
            bindHandler.ManualBindInput(false);
            bindHandler.ManualBindInput(true);
        }
    }
    #endregion


    #region Combat Mode Related

    #endregion

    private void HandleCancelPressed()
    {
        SetCombatUINavigationMode(CombatUINavigationMode.Combat);
        inputHandler.OnNo.RemoveListener(HandleCancelPressed);
    }

    //protected override void HandleTutorialClosed(TutorialNames tutorial)
    //{
    //    SetNavigatable(false);

    //    if (inputHandler.CurrentActionMap != ActionMaps.Combat)
    //        inputHandler.SwitchActionMap(ActionMaps.Combat);
    //}

    

    private HealthBar GetPlayerHealthBar()
    {
        return currentHealthbars.Where(b => b.displayCharacter == player).FirstOrDefault();
    }

    private HealthBar GetEnemyHealthBar()
    {
        return currentHealthbars.Where(b => b.displayCharacter == currentEnemy).FirstOrDefault();
    }

    private void UpdateHealthbars()
    {
        HealthBar[] healthBars = FindObjectsOfType<HealthBar>();

        currentHealthbars.RemoveAll(h => h == null);

        foreach(var healthBar in healthBars)
        {
            if (!currentHealthbars.Contains(healthBar))
                currentHealthbars.Add(healthBar);
        }
    }
            


    private void SetupInspectableDisplayParents()
    {
        var enemyHealthBar = GetEnemyHealthBar();
        enemyIntentsParent = enemyHealthBar.IntentDisplay;
        enemyStatsParent = enemyHealthBar.BuffDebuffDisplay;
        playerStatsParent = GetPlayerHealthBar().BuffDebuffDisplay;
    }

    private void InitiateEndTurn()
    {
        if (!player._combatEntity.isMyTurn)
            return;

        player._combatEntity.EndTurn();
    }

    private void PressEndTurnButton()
    {
        if (currentCombatNavigationMode != CombatUINavigationMode.Combat)
            return;


        endTurn.onClick.Invoke();
    }

    private void BindGamepadListener()
    {
        inputHandler.OnL1.AddListener(TogglePotionMode);
        inputHandler.OnR1.AddListener(ToggleInspectMode);
        inputHandler.OnR2.AddListener(PressEndTurnButton);
        endTurn.onClick.AddListener(InitiateEndTurn);
    }

    private void UnbindGamepadListener()
    {
        inputHandler.OnL1.RemoveListener(TogglePotionMode);
        inputHandler.OnR1.RemoveListener(ToggleInspectMode);
        inputHandler.OnR2.RemoveListener(PressEndTurnButton);
        endTurn.onClick.RemoveListener(InitiateEndTurn);
    }

    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate(navigatableOnActivated);

        inputHandler ??= EventSystem.current.GetComponent<InputHandler>();

        UnbindGamepadListener();
        BindGamepadListener();

        SetCombatUINavigationMode(CombatUINavigationMode.Combat);       

    }

    public override void Deactivate()
    {
        if (!isScreenActive) return;

        SetCombatUINavigationMode(CombatUINavigationMode.Disabled);
        base.Deactivate();

        UnbindGamepadListener();
    }

    private void RegisterEnemy(Character enemyCharacter)
    {
        currentEnemy = enemyCharacter;
        UpdateHealthbars();
        SetupInspectableDisplayParents();
    }

    private void UnregisterEnemy(Character enemyCharacter)
    {
        if (currentEnemy != null && currentEnemy == enemyCharacter)
        {
            skipButton.gameObject.SetActive(true);
            
            currentEnemy = null;
        }
    }

    private void Awake()
    {
        combatController ??= CombatController._instance;
        combatController.OnEnemySpawned += RegisterEnemy;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        combatController.OnEnemySpawned -= RegisterEnemy;
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
    PostCombat,
    Disabled
}
