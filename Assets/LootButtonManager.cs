using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LootButtonManager : UIInventorySubPanel
{
    public EquipmentButton[] EquipmentButtons;
    public EquipmentButton[] GoldButtons;
    public EquipmentButton[] RelicButtons;

    [SerializeField] private Sprite[] EquipmentSprites;
    [SerializeField] private Transform layoutgroup;

    public List<EquipmentButton> CurrentActiveButtons => currentActiveButtons;

    public List<List<Equipment>> EquipmentLists = new List<List<Equipment>>();
    public List<List<Equipment>> RelicLists = new List<List<Equipment>>();

    public List<int> GoldList = new List<int>();

    [SerializeField] public GameObject SkipButton;
    [SerializeField] private Button leaveButton;

    public static LootButtonManager _instance;

    private List<EquipmentButton> currentActiveButtons = new List<EquipmentButton>();

    private Selectable leftSelectableAtInventoryUI = null;

    private bool stillSettingUpButtons = false;

    
    public void SetLootPanelButtonsLeftNavigation(Selectable selectable)
    {
        leftSelectableAtInventoryUI = selectable;
        StartCoroutine(SetPanelLeftNavigationRoutine(leftSelectableAtInventoryUI));   
    }

    public void RefreshLootButtonNavigation()
    {
        SetupLootPanelNavigation();
    }

    public Selectable GetTopMostInteractableLootButton()
    {
        var topMostLootButton = currentActiveButtons.Where(eb => eb.Button.interactable).FirstOrDefault();
        return topMostLootButton.Button as Selectable;
    }

    private IEnumerator SetPanelLeftNavigationRoutine(Selectable selectable)
    {
        while (stillSettingUpButtons)
            yield return null;

        SetPanelLeftNavigation(selectable);
    }

    private void SetPanelLeftNavigation(Selectable selectable)
    {
        for (int i = 0; i < currentActiveButtons.Count; i++)
        {
            if (i % 2 == 0)
            {
                var button = currentActiveButtons[i].Button;
                var navi = button.navigation;
                if (navi.mode != Navigation.Mode.Explicit)
                    navi.mode = Navigation.Mode.Explicit;

                navi.selectOnLeft = selectable;
                button.navigation = navi;
            }
        }
    }

    private Selectable GetVerticalSelectableTarget(int currentIndex, int direction)
    {
        int step = direction * 2;
        int index = currentIndex + step;

        while (index >= 0 && index < currentActiveButtons.Count)
        {
            var button = currentActiveButtons[index].Button;
            if (button.interactable)
                return button;

            index += step; 
        }

        return direction > 0 ? leaveButton : null;
    }

    private bool NextActiveButtonAvailable(int nextIndex)
    {
        return nextIndex < currentActiveButtons.Count && currentActiveButtons[nextIndex].gameObject.activeSelf && currentActiveButtons[nextIndex].Button.interactable;
    }

    private void SetupLootPanelNavigation()
    {
        for (int i = 0; i < currentActiveButtons.Count; i++)
        {
            var button = currentActiveButtons[i].Button;
            var navi = button.navigation;
            navi.mode = Navigation.Mode.Explicit;
            bool isLeftColumn = (i % 2 == 0);

            if (isLeftColumn)
            {
                int rightIndex = i + 1;
                navi.selectOnRight = NextActiveButtonAvailable(rightIndex) ? currentActiveButtons[rightIndex].Button : null;
                navi.selectOnLeft = leftSelectableAtInventoryUI;
            }
            else
            {
                int leftIndex = i - 1;
                navi.selectOnLeft = NextActiveButtonAvailable(leftIndex) ? currentActiveButtons[leftIndex].Button : null;
                navi.selectOnRight = null;
            }

            navi.selectOnDown = GetVerticalSelectableTarget(i, 1);
            navi.selectOnUp = GetVerticalSelectableTarget(i, -1);

            button.navigation = navi;
        }

        stillSettingUpButtons = false;
    }

    private void PopulateCurrentActiveButtons(List<EquipmentButton> eqButtons)
    {
        if (eqButtons.Count < 1)
        {
            Debug.LogError($"Error: No active loot button GameObject available!");
            return;
        }

        foreach (EquipmentButton button in eqButtons)
        {
            if (button.gameObject.activeSelf)
            {
                button.ActivateButton();
                currentActiveButtons.Add(button);
            }
        }

        if (currentActiveButtons.Count > 0)
        {
            SetupLootPanelNavigation();
        }

        
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public bool HasItems()
    {
        if (EquipmentLists.Count > 0)
            return true;
        if (GoldList.Count > 0)
            return true;
        if (RelicLists.Count > 0)
            return true;
        return false;
    }

    public void SetLootButtons(List<List<Equipment>> equipments = null, List<int> Golds = null, List<List<Equipment>> relics = null)
    {
        stillSettingUpButtons = true;
        ClearAll();
        EquipmentLists = equipments;
        GoldList = Golds;
        RelicLists = relics;
        List<EquipmentButton> cachedButtonGameObjects = new List<EquipmentButton>();

        if(equipments != null)
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                EquipmentButtons[i].ActivateButton();
                AdjustTextAndIcon( EquipmentButtons[i], equipments[i]);
                cachedButtonGameObjects.Add(EquipmentButtons[i]);
            }
        }
        if(relics != null)
        {
            for (int i = 0; i < relics.Count; i++)
            {
                RelicButtons[i].ActivateButton();
                RelicButtons[i].SetEquipmentButton(EquipmentSprites[5], "Relic");
                cachedButtonGameObjects.Add(RelicButtons[i]);
            }
        }
        if(Golds != null)
        {
            for (int i = 0; i < Golds.Count; i++)
            {
                GoldButtons[i].ActivateButton();
                GoldButtons[i].SetEquipmentButton(EquipmentSprites[4], Golds[i].ToString());
                //GoldButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = Golds[i] + " Gold";
                cachedButtonGameObjects.Add(GoldButtons[i]);
            }
        }

        PopulateCurrentActiveButtons(cachedButtonGameObjects);
    }

    private void AdjustTextAndIcon(EquipmentButton eqButton, List<Equipment> equipments)
    {
        bool allWeap = true;
        bool allPotion = true;

        bool allScoll = true;
        foreach (var e in equipments)
        {            
            if (e.slot != Equipment.Slot.Scroll)
            {
                allScoll = false;
            }
            if (e.slot != Equipment.Slot.OneHander)
            {
                allWeap = false;
            }
            if (e.slot != Equipment.Slot.Consumable)
            {
                allPotion = false;
            }
        }

        if (allWeap)
        {
            eqButton.SetEquipmentButton(EquipmentSprites[2], "Weapon");
            return;
        }

        if (allScoll)
        {
            eqButton.SetEquipmentButton(EquipmentSprites[1], "Scroll");
            return;
        }
        if (allPotion)
        {
            eqButton.SetEquipmentButton(EquipmentSprites[3], "Potion");
            return;
        }

        eqButton.SetEquipmentButton(EquipmentSprites[0], "Equipment");
        return;

        
    }

    private void ClearAll()
    {
        currentActiveButtons.Clear();

        foreach (EquipmentButton eButton in EquipmentButtons)
        {
            eButton.gameObject.SetActive(false);
            eButton.Button.interactable = true;
        }
        foreach (EquipmentButton eButton in RelicButtons)
        {
            eButton.gameObject.SetActive(false);
            eButton.Button.interactable = true;
        }
        foreach (EquipmentButton eButton in GoldButtons)
        {
            eButton.gameObject.SetActive(false);
            eButton.Button.interactable = true;
        }

    }

    private void SelectionFinishedCallback()
    {
        SelectionManager._instance.OnSelectionFinished -= SelectionFinishedCallback;

        Debug.Log($"LootButtonManager should refresh button navigation here");

        RefreshLootButtonNavigation();
    }

    public void EquipmentSelect(int i)
    {
        SelectionManager._instance.OnSelectionFinished += SelectionFinishedCallback;

        SelectionManager._instance.SelectionsFromList(EquipmentLists[i]);
        //UIController._instance.ToggleInventoryUI(1);
        EquipmentButtons[i].DeactivateButton();

        //RefreshLootButtonNavigation();

        if (EquipmentLists[i].Count == 1)
        {
            SelectionManager._instance.selectionsLeft = 1;
        }
    }

    public void RelicSelect(int i)
    {
        SelectionManager._instance.SelectionsFromList(RelicLists[i]);
        //UIController._instance.ToggleInventoryUI(1);
        RelicButtons[i].DeactivateButton();

        RefreshLootButtonNavigation();
    }

    public void GoldSelect(int i)
    {
        CombatController._instance.Player.GetGold(GoldList[i]);
        GoldButtons[i].DeactivateButton();

        RefreshLootButtonNavigation();
    }

   
    // public void AddRelicButton()
    // {
    //     GameObject b = Instantiate(GoldButtonPrefab, layoutgroup);
    // }


    public void EquipmentStartButton()
    {
        SelectionManager._instance.RandomSelectionBegging();
    }
    
    
}
