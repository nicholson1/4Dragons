using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    public InventorySlot Item1;
    public InventorySlot Item2;
    public InventorySlot Item3;
    public InventorySlot Item4;
    
    public InventorySlot SellButton;
    public TextMeshProUGUI RerollButton;
    public TextMeshProUGUI ShopTitle;

    public GameObject[] relicBuyButtons;

    public EquipmentCreator EC;

    public InventorySlot.SellShopType ShopType;
    public int shopPrice = 25;

    public static ShopManager _instance;

    [SerializeField] private AudioClip reRollShop;
    [SerializeField] private float reRollVol;
    [SerializeField] private float reRollpitch;

    [SerializeField] private AudioClip openShop;
    [SerializeField] private float openShopVol;
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

    public void RandomShop()
    {
        // get random shop
        //Armor,
        // Scrolls,
        // Weapons,
        // FullHalfPrice,
        
        Random.InitState(CombatController._instance.CurrentSeed);

        int roll;
        if (CombatController._instance.Player._gold < 200)
        {
            roll = Random.Range(0, 4);
        }
        else
        {
            roll = Random.Range(0, 6);
        }
        
        InitializeShop(roll);
        UIController._instance.ToggleShopUI();
        UIController._instance.ToggleInventoryUI(1);
        
        SoundManager.Instance.Play2DSFX(openShop, openShopVol);

    }

    public void InitializeShop(int i)
    {
        InitializeShop((InventorySlot.SellShopType) i);
    }

    public void ReRollShop()
    {
        SoundManager.Instance.Play2DSFX(reRollShop, reRollVol, reRollpitch, .05f);

        CombatController._instance.Player._gold -= shopPrice;

        
        if (RelicManager._instance.CheckRelic(RelicType.DragonRelic3))
        {
            // do not increase the price of reroll
        }
        else
        {
            shopPrice += 25;
        }

        InitializeShop(ShopType, true);
        CombatController._instance.Player.UpdateStats();


    }
    

    public void InitializeShop(InventorySlot.SellShopType type, bool reroll = false)
    {
        CombatController._instance.NextCombatButton.gameObject.SetActive(false);
        if(!reroll)
            Random.InitState(CombatController._instance.CurrentSeed);

        //////////////////////////////////////////////////////////////////////////////////////////////////
        //TODO REMOVE THIS ITS ONLY FOR TESTING THE POTIONS
        //type = InventorySlot.SellShopType.Relics;
        //////////////////////////////////////////////////////////////////////////////////////////////////

        
        ShopType = type;
        SellButton.SellType = type;

        foreach (InventorySlot slot in EquipmentManager._instance.InventorySlotsRef)
        {
            if(slot.Item != null)
                slot.Item.TurnOnSellPrice(slot.CalculateGold(slot.Item.e, SellButton));
        }
        
        int level = CombatController._instance.Player._level;
        Equipment e;
        // create drag items

        RerollButton.text = "Reroll - " + shopPrice;

        ClearItem(Item1);
        ClearItem(Item2);
        ClearItem(Item3);
        ClearItem(Item4);

        foreach (var buyButton in relicBuyButtons)
        {
            buyButton.gameObject.SetActive(false);
        }

        ShopTitle.text = GetShopName(type);
        
        switch (type)
        {
            case InventorySlot.SellShopType.Weapons:

                e = EC.CreateRandomWeapon(level, false);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomWeapon(level, false);
                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomWeapon(level, false);
                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomWeapon(level, false);
                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.Scrolls:

                e = EC.CreateRandomSpellScroll(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomSpellScroll(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomSpellScroll(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomSpellScroll(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.Armor:
                e = EC.CreateRandomArmor(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomArmor(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomArmor(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomArmor(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.Blacksmith:
                e = EC.CreateRandomArmor(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomArmor(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomWeapon(level, false);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomWeapon(level, false);

                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.FullHalfPrice:
                e = EC.CreateRandomWeapon(level, false);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomPotion(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomSpellScroll(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                relicBuyButtons[3].gameObject.SetActive(true);
                break;
            case InventorySlot.SellShopType.Relics:
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                relicBuyButtons[0].gameObject.SetActive(true);

                
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                relicBuyButtons[1].gameObject.SetActive(true);

                
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                relicBuyButtons[2].gameObject.SetActive(true);
                
                e = EC.CreateRandomPotion(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.Potions:
                e = EC.CreateRandomPotion(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomPotion(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomPotion(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomPotion(level);
                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            
        }
        AdjustGoldText(Item1);
        AdjustGoldText(Item2);
        AdjustGoldText(Item3);
        AdjustGoldText(Item4);

        if (Modifiers._instance.CurrentMods.Contains(Mods.NoShopRerolls))
        {
            RerollButton.GetComponentInParent<Button>().interactable = false;
        }

        
        
        
        
    }

    void AdjustGoldText(InventorySlot slot)
    {
        
        TextMeshProUGUI goldText = slot.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>() ;
        int cost = (slot.Item.e.stats[Stats.Rarity] + 1) * 60;
        if (CombatController._instance.Difficulty >= 7)
            cost += Mathf.RoundToInt(cost * .2f);  
        goldText.text = (cost).ToString();
        goldText.gameObject.SetActive(true);

        if (slot.Item.e.isRelic)
        {
            goldText.gameObject.SetActive(false);
        }
    }

    void ClearItem(InventorySlot slot)
    {
        if (slot.Item == null)
        {
            return;
        }
        Destroy(slot.Item.gameObject);
        slot.Item = null;
        slot.transform.GetChild(0).gameObject.SetActive(true);

    }

    private string GetShopName(InventorySlot.SellShopType type)
    {

        switch (type)
        {
            case InventorySlot.SellShopType.FullHalfPrice:
                return "The Traveling Merchant";
            case InventorySlot.SellShopType.Weapons:
                return "The Weaponsmith";
            case InventorySlot.SellShopType.Armor:
                return "The Armory";
            case InventorySlot.SellShopType.Scrolls:
                return "The Scribe";
            case InventorySlot.SellShopType.Relics:
                return "The Antiquitist";
            case InventorySlot.SellShopType.Potions:
                return "The Alchemist";
            case InventorySlot.SellShopType.Blacksmith:
                return "The Blacksmith";
        }

        return "";
    }

    public void Leave()
    {
        UIController._instance.ToggleShopUI(0);
        UIController._instance.ToggleMapUI(1);
        foreach (InventorySlot slot in EquipmentManager._instance.InventorySlotsRef)
        {
            if(slot.Item != null)
                slot.Item.TurnOffSellPrice();
        }
        

        //CombatController._instance.NextCombatButton.gameObject.SetActive(true);

    }

    public void BuyRelic(int index)
    {
        int currentGold = CombatController._instance.Player._gold;
        
        
        int cost = 300;
        if (CombatController._instance.Difficulty >= 7)
            cost += Mathf.RoundToInt(cost * .2f);
        
        if (currentGold < cost)
        {
            Item1.NotEnoughGoldEvent();
            return;
        }
        
        UIController._instance.PlayGetRelic();
        //UIController._instance.play();

        // if we do - gold
        CombatController._instance.Player._gold -= cost;
        Item1.BuyItemEvent(-cost);
        switch (index)
        {
            case 0:
                RelicManager._instance.SelectRelic(Item1.Item.e);
                ClearItem(Item1);
                relicBuyButtons[0].SetActive(false);
                break;
            case 1:
                RelicManager._instance.SelectRelic(Item2.Item.e);
                relicBuyButtons[1].SetActive(false);
                ClearItem(Item2);
                break;
            case 2:
                RelicManager._instance.SelectRelic(Item3.Item.e);
                relicBuyButtons[2].SetActive(false);
                ClearItem(Item3);
                break;
            case 3:
                RelicManager._instance.SelectRelic(Item4.Item.e);
                relicBuyButtons[3].SetActive(false);
                ClearItem(Item4);
                break;
        }

        CombatController._instance.Player.UpdateStats();

    }

    private void UpdateRerollButton(Character c)
    {
        RerollButton.GetComponentInParent<Button>().interactable = shopPrice <= CombatController._instance.Player._gold;
    }

    private void Start()
    {
        Character.UpdateStatsEvent += UpdateRerollButton;
    }

    private void OnDestroy()
    {
        Character.UpdateStatsEvent -= UpdateRerollButton;

    }
}
