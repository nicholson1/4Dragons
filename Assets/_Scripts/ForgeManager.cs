using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ForgeManager : MonoBehaviour
{

    public static ForgeManager _instance;
    public bool Upgrading = false;
    public bool Enhancing = false;

    [SerializeField] private RectTransform UpgradingFollow;
    [SerializeField] private RectTransform EnhancingFollow;
    private GameObject UpgradePrice;
    private GameObject EnhancePrice;
    private TextMeshProUGUI UpgradePriceText;
    private TextMeshProUGUI EnhancePriceText;
    
    public Canvas canvas;
    public Vector2 UpgradingFollowOffset;
    public Vector2 EnhancingFollowOffset;

    public float priceMod = 1;
    public int amountOfClicks = 0;

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

    public void ShowIcon()
    {
        if(Upgrading)
        {
            UpgradePrice.SetActive(true);
        }
        if(Enhancing)
        {
            EnhancePrice.SetActive(true);
        }
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
                Upgrading = false;
                Enhancing = false;
                UpgradingFollow.gameObject.SetActive(false);
                EnhancingFollow.gameObject.SetActive(false);
            }
        }
    }
    

    public void ShowPrice(Equipment e)
    {
        int Upgrade = Mathf.RoundToInt((e.stats[Stats.ItemLevel] * (e.stats[Stats.Rarity] + 1)) * priceMod);
        int Enchance = Mathf.RoundToInt((e.stats[Stats.ItemLevel] + 5)* (e.stats[Stats.Rarity] + 1) * priceMod);
        
        if(Upgrading)
        {
            UpgradePriceText.gameObject.SetActive(true);
            UpgradePriceText.text = Upgrade.ToString();
        }
        if(Enhancing)
        {
            EnhancePriceText.gameObject.SetActive(true);
            EnhancePriceText.text = Enchance.ToString();
        }
    }
    public void HidePrice()
    {
        EnhancePriceText.gameObject.SetActive(false);
        UpgradePriceText.gameObject.SetActive(false);
    }

    private void Start()
    {
        UpgradePriceText = UpgradingFollow.GetComponentInChildren<TextMeshProUGUI>();
        UpgradePrice = UpgradePriceText.transform.parent.gameObject;
        EnhancePriceText = EnhancingFollow.GetComponentInChildren<TextMeshProUGUI>();
        EnhancePrice = EnhancePriceText.transform.parent.gameObject;
        
        UpgradePrice.SetActive(false);
        EnhancePrice.SetActive(false);
        UpgradingFollow.gameObject.SetActive(false);
        EnhancingFollow.gameObject.SetActive(false);
        
    }

    private void LateUpdate()
    {
        if (Enhancing || Upgrading)
        {
            // Convert the screen position to canvas space (UI space)
            Vector3 mouseScreenPosition = Input.mousePosition;

            // Convert the screen space position to local space of the RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                mouseScreenPosition, 
                null, // No camera needed for Screen Space - Overlay
                out Vector2 localPoint
            );

            // Set the UI element's position to the local point
            if (Enhancing)
                EnhancingFollow.localPosition = localPoint + EnhancingFollowOffset;
            if (Upgrading)
                UpgradingFollow.localPosition = localPoint + UpgradingFollowOffset;
        }
        
    }

    public void ClickUpgradeButtonFromForge()
    {
        Upgrading = true;
        Enhancing = false;
        UpgradingFollow.gameObject.SetActive(true);
        EnhancingFollow.gameObject.SetActive(false);

        UpgradingFollow.SetParent(canvas.transform);
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
        Upgrading = false;
        Enhancing = true;
        UpgradingFollow.gameObject.SetActive(false);
        EnhancingFollow.gameObject.SetActive(true);
        
        EnhancingFollow.SetParent(canvas.transform);

        //add icon to the mouse
        //when mouse over item display the gold it costs
        // text red if too expensive
        // on click
        // upgrade item
        // take gold
        HidePrice();
        amountOfClicks = -1;

    }

   
    public void Leave()
    {
        Upgrading = false;
        Enhancing = false;
        UpgradingFollow.gameObject.SetActive(false);
        EnhancingFollow.gameObject.SetActive(false);
        UIController._instance.ToggleForgeUI(0);
        UIController._instance.ToggleInventoryUI(0);
        UIController._instance.ToggleMapUI(1);
        foreach (InventorySlot slot in EquipmentManager._instance.InventorySlotsRef)
        {
            if(slot.Item != null)
                slot.Item.TurnOffSellPrice();
        }

        amountOfClicks = 0;
        //CombatController._instance.NextCombatButton.gameObject.SetActive(true);
    }
}
