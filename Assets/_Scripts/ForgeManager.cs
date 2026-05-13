using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ForgeManager : UIInventorySubPanel
{
    public static ForgeManager _instance;
    //public bool Upgrading = false;
    //public bool Enhancing = false;
    
    [SerializeField, FormerlySerializedAs("UpgradingFollow")] private RectTransform forgeLabel;
    //[SerializeField] private RectTransform EnhancingFollow;
    [SerializeField] private Image followLabelImage;
    private GameObject forgePriceObject;
    private GameObject EnhancePrice;
    private TextMeshProUGUI forgePriceText;
    private TextMeshProUGUI EnhancePriceText;

    [SerializeField] private Sprite upgradeSprite;
    [SerializeField] private Sprite enhanceSprite;
        
    public Canvas canvas;
    public Vector2 UpgradingFollowOffset;
    public Vector2 EnhancingFollowOffset;

    public float priceMod = 1;
    public int amountOfClicks = 0;

    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private Toggle upgradeToggle;
    [SerializeField] private Toggle enhanceToggle;
    [SerializeField] private Toggle smeltToggle;
    [SerializeField] private Button leaveButton;

    [SerializeField] private List<Selectable> leftmostSelectables = new List<Selectable>();

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private InputHandler inputHandler;
    private UIStateMonitor stateMonitor;

    public override Selectable GetFirstInteractableSelectable()
    {
        return upgradeToggle;
    }

    public override void SetupLeftNavigationToMainPanel(List<Selectable> selectables)
    {
        foreach(var selectable in leftmostSelectables)
        {
            Selectable closestInventoryButton = selectables.OrderBy(b => Mathf.Abs(b.transform.position.y - selectable.transform.position.y)).FirstOrDefault();

            Navigation navi = selectable.navigation;
            navi.selectOnLeft = closestInventoryButton;
            selectable.navigation = navi;
        }

        EventSystem.current.SetSelectedGameObject(GetFirstInteractableSelectable().gameObject);
    }

    public override void SetSkipButtonInteractable(bool isInteractable)
    {
        leaveButton.interactable = isInteractable;
    }

    private void OnUpgradeToggled(bool toOn)
    {
        if (!toOn)
        {
            if(stateMonitor.GetUINavigationMode() == NavigationMode.Upgrade)
            {
                stateMonitor.SetUINavigationMode(NavigationMode.Neutral);
                HandleHighlighterOnToggle(false);
            }
            return;
        }
                
        stateMonitor.SetUINavigationMode(NavigationMode.Upgrade);
        followLabelImage.sprite = upgradeSprite;
        forgeLabel.SetParent(canvas.transform);
        HidePrice();
        amountOfClicks = -1;

        HandleHighlighterOnToggle(toOn);

        //ToggleOff(enhanceToggle);

        inputHandler.OnNo.RemoveListener(CleanupToggle);
        inputHandler.OnNo.AddListener(CleanupToggle);
    }

    public void CacheInventorySlots(List<InventorySlot> slots)
    {
        inventorySlots = slots;
    }

    private void HandleHighlighterOnToggle(bool toOn)
    {
        if (inventorySlots.Count <= 0) return;

        foreach(var slot in inventorySlots)
        {
            if(slot.Item != null && slot.Item.IsGear())
            {
                slot.Item.HandleItemHighlight(toOn);
            }
        }
    }

    private void OnEnhanceToggled(bool toOn)
    {
        if (!toOn)
        {
            if (stateMonitor.GetUINavigationMode() == NavigationMode.Enhance)
            {
                stateMonitor.SetUINavigationMode(NavigationMode.Neutral);
                HandleHighlighterOnToggle(false);
            }    
            return;
        }

        stateMonitor.SetUINavigationMode(NavigationMode.Enhance);
        followLabelImage.sprite = enhanceSprite;
        forgeLabel.SetParent(canvas.transform);
        HidePrice();
        amountOfClicks = -1;

        HandleHighlighterOnToggle(toOn);

        //ToggleOff(upgradeToggle);

        inputHandler.OnNo.RemoveListener(CleanupToggle);
        inputHandler.OnNo.AddListener(CleanupToggle);
    }

    private void OnSmeltToggled(bool toOn)
    {
        if (!toOn)
        {
            if (stateMonitor.GetUINavigationMode() == NavigationMode.Sell)
            {
                stateMonitor.SetUINavigationMode(NavigationMode.Neutral);
                HandleHighlighterOnToggle(false);
            }
            return;
        }

        stateMonitor.SetUINavigationMode(NavigationMode.Sell);
        followLabelImage.sprite = enhanceSprite;
        forgeLabel.SetParent(canvas.transform);
        
        amountOfClicks = -1;

        HandleHighlighterOnToggle(toOn);

        //ToggleOff(upgradeToggle);

        inputHandler.OnNo.RemoveListener(CleanupToggle);
        inputHandler.OnNo.AddListener(CleanupToggle);
    }



    private void CleanupToggle()
    {
        inputHandler.OnNo.RemoveListener(CleanupToggle);

        toggleGroup.SetAllTogglesOff();

        forgeLabel.gameObject.SetActive(false);


        if(stateMonitor.GetUINavigationMode() != NavigationMode.Neutral)
        {
            stateMonitor.SetUINavigationMode(NavigationMode.Neutral);
        }
    }

    public void ShowIcon()
    {
        if (stateMonitor.GetUINavigationMode() == NavigationMode.Neutral) return;

        forgePriceObject.SetActive(true);

    }

    public void AdjustAmountOfClicks(int clicks)
    {
        if (amountOfClicks == -1)
            amountOfClicks = clicks;
        else
        {
            amountOfClicks += clicks;
            if (amountOfClicks == 0)
            {
                forgeLabel.gameObject.SetActive(false);
            }
        }
    }
    
    public int GetUpgradePrice(NavigationMode mode, Equipment equipment)
    {
        int upgradePrice = 0;
        if(mode == NavigationMode.Upgrade)
            upgradePrice = Mathf.RoundToInt((equipment.stats[Stats.ItemLevel] * (equipment.stats[Stats.Rarity] + 1)) * priceMod) * 4;
        else if(mode == NavigationMode.Enhance)
            upgradePrice = Mathf.RoundToInt((equipment.stats[Stats.ItemLevel] + 5) * (equipment.stats[Stats.Rarity] + 1) * priceMod) * 4;
        
        forgePriceText.text = upgradePrice.ToString();
        return upgradePrice;
    }

    public void ShowForgePrice(NavigationMode mode, Equipment e)
    {
        GetUpgradePrice(mode, e);           

        forgePriceObject.SetActive(true);
        forgePriceText.gameObject.SetActive(true);
    }

    public void HidePrice()
    {
        //EnhancePriceText.gameObject.SetActive(false);
        forgePriceText.gameObject.SetActive(false);
    }


    public void ClickUpgradeButtonFromForge()
    {
        forgeLabel.gameObject.SetActive(true);
        //EnhancingFollow.gameObject.SetActive(false);

        forgeLabel.SetParent(canvas.transform);
        //add icon to the mouse
        //when mouse over item display the gold it costs
        // text red if too expensive
        // on click
        // upgrade item
        // take gold
        HidePrice();
        amountOfClicks = -1;
    }
    public void ClickEnhanceButtonFromForge()
    {
        forgeLabel.gameObject.SetActive(true);
        //EnhancingFollow.gameObject.SetActive(true);

        //EnhancingFollow.SetParent(canvas.transform);
        forgeLabel.SetParent(canvas.transform);

        //add icon to the mouse
        //when mouse over item display the gold it costs
        // text red if too expensive
        // on click
        // upgrade item
        // take gold
        HidePrice();
        amountOfClicks = -1;

    }

    public void SetForgeLabelPosition(RectTransform itemRt)
    {
        Vector2 offset = new Vector2(50f, 50f);
        forgeLabel.anchoredPosition = itemRt.anchoredPosition + offset; 
    }
       
    public void Leave()
    {
        CleanupToggle();

        UIController._instance.CloseInventoryWithExtraPanel(InventoryState.Forge);
        UIController._instance.ToggleMapNew(true, true);

        foreach (InventorySlot slot in EquipmentManager._instance.InventorySlotsRef)
        {
            if(slot.Item != null)
                slot.Item.TurnOffSellPrice();
        }

        amountOfClicks = 0;
        //CombatController._instance.NextCombatButton.gameObject.SetActive(true);
    }

    //private void LateUpdate()
    //{
    //    if (forgeMode != ForgeMode.None)
    //    {
    //        // Convert the screen position to canvas space (UI space)
    //        Vector3 mouseScreenPosition = Input.mousePosition;

    //        // Convert the screen space position to local space of the RectTransform
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //            canvas.transform as RectTransform,
    //            mouseScreenPosition,
    //            null, // No camera needed for Screen Space - Overlay
    //            out Vector2 localPoint
    //        );

    //        // Set the UI element's position to the local point
    //        UpgradingFollow.localPosition = localPoint + UpgradingFollowOffset;

    //        //if (forgeMode == ForgeMode.Enhance)
    //        //    EnhancingFollow.localPosition = localPoint + EnhancingFollowOffset;
    //        //else if (forgeMode == ForgeMode.Upgrade)
    //        //    UpgradingFollow.localPosition = localPoint + UpgradingFollowOffset;
    //    }

    //}

    private void Start()
    {
        inputHandler ??= EventSystem.current.GetComponentInChildren<InputHandler>();
        stateMonitor = UIController._instance.StateMonitor;
        forgePriceText = forgeLabel.GetComponentInChildren<TextMeshProUGUI>();
        forgePriceObject = forgePriceText.transform.parent.gameObject;

        forgePriceObject.SetActive(false);

        forgeLabel.gameObject.SetActive(false);

        upgradeToggle.onValueChanged.AddListener(OnUpgradeToggled);
        enhanceToggle.onValueChanged.AddListener(OnEnhanceToggled);
        smeltToggle.onValueChanged.AddListener(OnSmeltToggled);
        
        leaveButton.onClick.AddListener(CleanupToggle);      

    }

    private void OnDestroy()
    {
        upgradeToggle.onValueChanged.RemoveListener(OnUpgradeToggled);
        enhanceToggle.onValueChanged.RemoveListener(OnEnhanceToggled);
        smeltToggle.onValueChanged.RemoveListener(OnSmeltToggled);

        leaveButton.onClick.RemoveListener(CleanupToggle);
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


}

//public enum ForgeMode
//{
//    Upgrade,
//    Enhance,
//    Sell,
//    None
//}