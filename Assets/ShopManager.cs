using ImportantStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopManager : UIInventorySubPanel
{
    public InventorySlot Item1;
    public InventorySlot Item2;
    public InventorySlot Item3;
    public InventorySlot Item4;

    
    public Button RerollButton;
    public TextMeshProUGUI ShopTitle;

    public GameObject[] relicBuyButtons;

    public EquipmentCreator EC;

    public InventorySlot.SellShopType ShopType;
    public int shopPrice = 25;

    public static ShopManager _instance;

    [SerializeField] private Button leaveButton;
    [SerializeField] private AudioClip reRollShop;
    [SerializeField] private float reRollVol;
    [SerializeField] private float reRollpitch;
    [SerializeField] private TextMeshProUGUI rerollPrice;

    [SerializeField] private AudioClip openShop;
    [SerializeField] private float openShopVol;

    [SerializeField] private InventorySlot sellSlot; 
    private Toggle sellToggle;

    //private List<Selectable> cachedNavigationLeftTargets = new List<Selectable>();
    private List<InventorySlot> currentShopItems = new List<InventorySlot>();
    private List<InventorySlot> cachedInventorySlots = new List<InventorySlot>();

    private UIStateMonitor stateMonitor;
    private InputHandler inputHandler;

    public override void SetSkipButtonInteractable(bool isInteractable)
    {
        leaveButton.interactable = isInteractable;
    }

    public override Selectable GetFirstInteractableSelectable()
    {
        return Item1.GetComponentInChildren<Selectable>();
    }

    public void RefreshButtonNavigation()
    {
        SetupLeftNavigationToMainPanel();
    }

    private void SetupLeftNavigationToMainPanel()
    {
        var buySlot = GetFirstInteractableSelectable();
        var buySlotNavi = buySlot.navigation;

        var targetLeft = cachedRightmostInventoryButtons.OrderBy(b => Mathf.Abs(b.transform.position.y - buySlot.transform.position.y)).FirstOrDefault();
        //var sellToggle = sellSlot.GetComponentInChildren<Selectable>();
        var sellSlotNavi = sellToggle.navigation;

        if (targetLeft != null)
        {
            buySlotNavi.selectOnLeft = targetLeft;
            sellSlotNavi.selectOnLeft = targetLeft;
            buySlot.navigation = buySlotNavi;
            sellToggle.navigation = sellSlotNavi;
        }

        EventSystem.current.SetSelectedGameObject(GetFirstInteractableSelectable().gameObject);
    }

    private void SetupRerollButtonRelatedNavigation()
    {
        var leaveButtonNavi = leaveButton.navigation;
        leaveButtonNavi.selectOnUp = RerollButton.interactable ? RerollButton : Item4.GetComponentInChildren<Selectable>();
        leaveButton.navigation = leaveButtonNavi;

        //var sellSlotSelectable = sellSlot.GetComponentInChildren<Selectable>();
        var sellSlotNavi = sellToggle.navigation;
        sellSlotNavi.selectOnRight = RerollButton.interactable ? RerollButton : leaveButton;
        sellToggle.navigation = sellSlotNavi;

        var item4Button = Item4.GetComponentInChildren<Selectable>();
        var item4Nav = item4Button.navigation;
        item4Nav.selectOnDown = RerollButton.interactable ? RerollButton : leaveButton;
        item4Button.navigation = item4Nav;

        var item3Button = Item3.GetComponentInChildren<Selectable>();
        var item3Nav = item3Button.navigation;
        item3Nav.selectOnDown = RerollButton.interactable ? RerollButton : leaveButton;
        item3Button.navigation = item3Nav;

        BindLeaveAndRollButton();
    }

    public override void SetupLeftNavigationToMainPanel(List<Selectable> selectables)
    {
        cachedRightmostInventoryButtons = selectables;

        SetupLeftNavigationToMainPanel();
    }

    private void SetupSellModeNavigation()
    {
        for(int i = 0; i < cachedRightmostInventoryButtons.Count; i++)
        {
            var selectable = cachedRightmostInventoryButtons[i];
            var navi = selectable.navigation;

            navi.selectOnRight = sellToggle;
            selectable.navigation = navi;
        }

        var sellToggleNavi = sellToggle.navigation;
        sellToggleNavi.selectOnUp = null;
        sellToggleNavi.selectOnRight = null;

        sellToggle.navigation = sellToggleNavi;
    }

    private void RevertFromSellModeNavigation()
    {

        for(int i = 0; i<cachedRightmostInventoryButtons.Count; i++)
        {
            var selectable = cachedRightmostInventoryButtons[i];
            var navi = selectable.navigation;
            navi.selectOnRight = GetFirstInteractableSelectable();
            selectable.navigation = navi;
        }

        var sellToggleNavi = sellToggle.navigation;
        sellToggleNavi.selectOnRight = leaveButton;
        sellToggleNavi.selectOnUp = Item1.GetComponentInChildren<Selectable>();
        sellToggle.navigation = sellToggleNavi;
    }

    private void OnSellToggled(bool toOn)
    {
        if (!toOn)
        {
            if (stateMonitor.GetUINavigationMode() == NavigationMode.Sell)
            {
                stateMonitor.SetUINavigationMode(NavigationMode.Neutral);
                RevertFromSellModeNavigation();
                HandleHighlighterOnToggle(false);
                inputHandler.OnNo.RemoveListener(CleanupToggle);
            }
            return;
        }

        stateMonitor.SetUINavigationMode(NavigationMode.Sell);

        SetupSellModeNavigation();
        HandleHighlighterOnToggle(toOn);

        inputHandler.OnNo.RemoveListener(CleanupToggle);
        inputHandler.OnNo.AddListener(CleanupToggle);
    }

    private void CleanupToggle()
    {
        if (sellToggle.isOn)
            sellToggle.isOn = false;

        if (stateMonitor.GetUINavigationMode() != NavigationMode.Neutral)
        {
            stateMonitor.SetUINavigationMode(NavigationMode.Neutral);
        }
    }

    public void CacheInventorySlots(List<InventorySlot> slots)
    {
        cachedInventorySlots = slots;
    }

    private void HandleHighlighterOnToggle(bool toOn)
    {
        if (cachedInventorySlots.Count <= 0) return;

        foreach (var slot in cachedInventorySlots)
        {
            if (slot.Item != null && slot.Item.IsGear())
            {
                slot.Item.HandleItemHighlight(toOn);
            }
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
        UIController._instance.ToggleInventoryUINew(true, InventoryState.Merchant);
        //UIController._instance.ToggleInventoryUI(1);

        SoundManager.Instance.Play2DSFX(openShop, openShopVol);
    }

    public void BlacksmithShop()
    {
        InitializeShop(6);
        UIController._instance.ToggleInventoryUINew(true, InventoryState.Merchant);

        SoundManager.Instance.Play2DSFX(openShop, openShopVol);
    }

    private IEnumerator AwaitShopMenuOpen()
    {
        while (UIController._instance.IsAnyPanelTransitioning())
            yield return null;

        BroadcastPanelOpen();

        BindLeaveAndRollButton();
    }

    private void BindLeaveAndRollButton()
    {
        UnbindLeaveAndRerollButtons();

        leaveButton.onClick.AddListener(Leave);
        RerollButton.onClick.AddListener(ReRollShop);
    }

    private void UnbindLeaveAndRerollButtons()
    {
        leaveButton.onClick.RemoveListener(Leave);
        RerollButton.onClick.RemoveListener(ReRollShop);
    }


    public void InitializeShop(int i)
    {
        //DEBUG always relic shop
        InitializeShop(InventorySlot.SellShopType.Relics);
        Debug.LogError($"DEBUG!!! always initialize relic here!!!");
        return;

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

    private void ItemBoughtCallback(InventorySlot item)
    {
        UnregisterItem(item);
    }

    private void RegisterItem(InventorySlot slot)
    {
        slot.OnItemBought += ItemBoughtCallback;
        AdjustGoldText(slot);
        currentShopItems.Add(slot);
    }

    private void UnregisterItem(InventorySlot slot)
    {
        slot.OnItemBought -= ItemBoughtCallback;
        currentShopItems.Remove(slot);
        ClearItem(slot);
        
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

        sellSlot.SellType = type;



        foreach (InventorySlot slot in EquipmentManager._instance.InventorySlotsRef)
        {
            if(slot.Item != null)
                slot.Item.TurnOnSellPrice(slot.CalculateGold(slot.Item.e, sellSlot));
        }
        
        int level = CombatController._instance.Player._level;
        Equipment e;
        // create drag items

        rerollPrice.text = shopPrice.ToString();

        UnregisterItem(Item1);
        UnregisterItem(Item2);
        UnregisterItem(Item3);
        UnregisterItem(Item4);

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
                //relicBuyButtons[3].gameObject.SetActive(true);
                break;

            case InventorySlot.SellShopType.Relics:
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                //relicBuyButtons[0].gameObject.SetActive(true);

                
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                //relicBuyButtons[1].gameObject.SetActive(true);

                
                e = RelicManager._instance.GetCommonRelic();
                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                //relicBuyButtons[2].gameObject.SetActive(true);
                
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
        RegisterItem(Item1);
        RegisterItem(Item2);
        RegisterItem(Item3);
        RegisterItem(Item4);

        if (Modifiers._instance.CurrentMods.Contains(Mods.NoShopRerolls))
        {
            RerollButton.interactable = false;
            SetupRerollButtonRelatedNavigation();
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

        //if (slot.Item.e.isRelic)
        //{
        //    goldText.gameObject.SetActive(false);
        //}
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
        if (!leaveButton.interactable)
            return;

        CleanupToggle();
        Debug.LogError($"ShopManager Leave was triggered!");
        UIController._instance.CloseInventoryWithExtraPanel(InventoryState.Merchant);
        //UIController._instance.ToggleMapUI(1);
        foreach (InventorySlot slot in EquipmentManager._instance.InventorySlotsRef)
        {
            if(slot.Item != null)
                slot.Item.TurnOffSellPrice();
        }

        UnbindLeaveAndRerollButtons();
        UIController._instance.ToggleMapNew(true, true);

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
                UnregisterItem(Item1);
                relicBuyButtons[0].SetActive(false);
                break;
            case 1:
                RelicManager._instance.SelectRelic(Item2.Item.e);
                relicBuyButtons[1].SetActive(false);
                UnregisterItem(Item2);
                break;
            case 2:
                RelicManager._instance.SelectRelic(Item3.Item.e);
                relicBuyButtons[2].SetActive(false);
                UnregisterItem(Item3);
                break;
            case 3:
                RelicManager._instance.SelectRelic(Item4.Item.e);
                relicBuyButtons[3].SetActive(false);
                UnregisterItem(Item4);
                break;
        }

        CombatController._instance.Player.UpdateStats();

    }

    private void UpdateRerollButton(Character c)
    {
        RerollButton.interactable = shopPrice <= CombatController._instance.Player._gold;
        SetupRerollButtonRelatedNavigation();
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

    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        stateMonitor = UIController._instance.StateMonitor;
        sellToggle = sellSlot.GetComponentInChildren<Toggle>();
        sellToggle.onValueChanged.AddListener(OnSellToggled);

        leaveButton.onClick.AddListener(Leave);

        Character.UpdateStatsEvent += UpdateRerollButton;
    }

    private void OnDestroy()
    {
        sellToggle.onValueChanged.RemoveListener(OnSellToggled);
        leaveButton.onClick.AddListener(Leave);
        Character.UpdateStatsEvent -= UpdateRerollButton;

    }
}
