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
    public ForgeMode ForgeMode => forgeMode;
    
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

    [SerializeField] private Toggle upgradeToggle;
    [SerializeField] private Toggle enhanceToggle;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button sellButton;

    [SerializeField] private List<Selectable> leftmostSelectables = new List<Selectable>();

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private ForgeMode forgeMode = ForgeMode.None;

    private InputHandler inputHandler;

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
            if(forgeMode == ForgeMode.Upgrade)
            {
                forgeMode = ForgeMode.None;
                HandleHighlighterOnToggle(false);
            }
            return;
        }

        forgeMode = ForgeMode.Upgrade;
        UIController._instance.StateMonitor.SetCursorMode(NavigationMode.Upgrade);
        followLabelImage.sprite = upgradeSprite;
        forgeLabel.SetParent(canvas.transform);
        HidePrice();
        amountOfClicks = -1;

        HandleHighlighterOnToggle(toOn);

        ToggleOff(enhanceToggle);

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
                slot.Item.ForgeableHighlighter.ToggleHighlighter(toOn);
            }
        }
    }

    private void OnEnhanceToggled(bool toOn)
    {
        if (!toOn)
        {
            if (forgeMode == ForgeMode.Enhance)
            {
                forgeMode = ForgeMode.None;
                HandleHighlighterOnToggle(false);
            }    
            return;
        }

        forgeMode = ForgeMode.Enhance;
        UIController._instance.StateMonitor.SetCursorMode(NavigationMode.Enhance);
        followLabelImage.sprite = enhanceSprite;
        forgeLabel.SetParent(canvas.transform);
        HidePrice();
        amountOfClicks = -1;

        HandleHighlighterOnToggle(toOn);

        ToggleOff(upgradeToggle);

        inputHandler.OnNo.RemoveListener(CleanupToggle);
        inputHandler.OnNo.AddListener(CleanupToggle);
    }

    private void ToggleOff(Toggle toggle, bool setModeNone = false)
    {
        if (setModeNone)
            forgeMode = ForgeMode.None;

        if (!toggle.isOn) return;

        toggle.isOn = false;
    }

    private void CleanupToggle()
    {
        inputHandler.OnNo.RemoveListener(CleanupToggle);

        if (upgradeToggle.isOn)
            upgradeToggle.isOn = false;
        if (enhanceToggle.isOn)
            enhanceToggle.isOn = false;

        forgeLabel.gameObject.SetActive(false);

        if (forgeMode != ForgeMode.None)
            forgeMode = ForgeMode.None;
    }

    public void ShowIcon()
    {
        if (forgeMode == ForgeMode.None) return;

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
    
    public int GetUpgradePrice(ForgeMode mode, Equipment equipment)
    {
        int upgradePrice = 0;
        if(mode == ForgeMode.Upgrade)
            upgradePrice = Mathf.RoundToInt((equipment.stats[Stats.ItemLevel] * (equipment.stats[Stats.Rarity] + 1)) * priceMod) * 4;
        else if(mode == ForgeMode.Enhance)
            upgradePrice = Mathf.RoundToInt((equipment.stats[Stats.ItemLevel] + 5) * (equipment.stats[Stats.Rarity] + 1) * priceMod) * 4;
        
        forgePriceText.text = upgradePrice.ToString();
        return upgradePrice;
    }

    public void ShowForgePrice(Equipment e)
    {
        GetUpgradePrice(forgeMode, e);           

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
        forgeMode = ForgeMode.Upgrade;

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
        forgeMode = ForgeMode.Enhance;

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

        forgePriceText = forgeLabel.GetComponentInChildren<TextMeshProUGUI>();
        forgePriceObject = forgePriceText.transform.parent.gameObject;

        forgePriceObject.SetActive(false);

        forgeLabel.gameObject.SetActive(false);

        upgradeToggle.onValueChanged.AddListener(OnUpgradeToggled);
        enhanceToggle.onValueChanged.AddListener(OnEnhanceToggled);
        leaveButton.onClick.AddListener(CleanupToggle);
        sellButton.onClick.AddListener(CleanupToggle);
       

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

public enum ForgeMode
{
    Upgrade,
    Enhance,
    None
}