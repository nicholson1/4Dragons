using ImportantStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEditor.Progress;
//using UnityEngine.UIElements;

public class InventorySlot : MonoBehaviour, IDropHandler, IGamepadButtonListener
{
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    public event Action<InventorySlot> OnItemBought;
    public event Action<DragItem> OnItemSold;

    public Equipment.Slot Slot;
    public DragItem Item = null;
    [SerializeField] public RectTransform _rt;
    private TextMeshProUGUI SlotLable;
    private Image background;
    [SerializeField] private Color baseColor;

    public SellShopType SellType = SellShopType.None;
    public bool CanAcceptItem = true;

    
    public static event Action<ErrorMessageManager.Errors> OnCannotDragItemOnCombat;
    public static event Action<ErrorMessageManager.Errors, int> OnBoughtItem;
    public static event Action<ErrorMessageManager.Errors> NotEnoughGold;

    [SerializeField] private AudioClip dropItem;
    [SerializeField] private float dropItemVol;
    //[SerializeField] private float placePitch;

    private InputHandler inputHandler;
    private UIStateMonitor stateMonitor;

    public void LabelCheck()
    {
        if (SlotLable == null)
        {
            SlotLable = GetComponentInChildren<TextMeshProUGUI>();
            background = GetComponent<Image>();
            if(background != null)
                baseColor = background.color;
        }

        if (Slot == Equipment.Slot.Drop)
        {
            return;
        }
        if (Item != null)
        {
            SlotLable.gameObject.SetActive(false);
            //background.color = ToolTipManager._instance.rarityColors[Item.e.stats[Stats.Rarity]];
            //background.color = new Color(background.color.r, background.color.g,background.color.b, baseColor.a);
            Item._rectTransform.localScale = _rt.localScale;


            //change color based on the rarity

        }
        else
        {
            if (Slot == Equipment.Slot.OneHander)
            {
                SlotLable.text = "Weapon";
            }
            else if (Slot != Equipment.Slot.All)
            {
                SlotLable.text = Slot.ToString();
            }
            else
            {
                if (CanAcceptItem)
                {
                    SlotLable.text = "";
                }
                else
                {
                    SlotLable.text = "Sold!";

                }
            }
            SlotLable.gameObject.SetActive(true);

            if(background != null)
                background.color = baseColor;
        }

        timer = 2f;

    }

    public bool IsDiscardSlot() => Slot == Equipment.Slot.Drop;
    public bool IsSellSlot() => Slot == Equipment.Slot.Sell;
    public bool IsUpgradeSlot() => Slot == Equipment.Slot.Upgrade;
    public bool IsShopSlot() => Slot == Equipment.Slot.Merchant;
    public bool IsRelicSlot() => Slot == Equipment.Slot.Relic;
    public bool IsAllSlot() => Slot == Equipment.Slot.All;
    public bool IsAllKindSlot() => IsDiscardSlot() || IsSellSlot() || IsUpgradeSlot() || IsShopSlot() || IsRelicSlot() || IsAllSlot();
    private bool IsCharacterEquipmentSlot()
    {
        bool isEqSlot = Slot is Equipment.Slot.Boots or
                                Equipment.Slot.Chest or
                                Equipment.Slot.Legs or
                                Equipment.Slot.Head or
                                Equipment.Slot.Gloves or
                                Equipment.Slot.Shoulders or
                                Equipment.Slot.OneHander or
                                Equipment.Slot.TwoHander or
                                Equipment.Slot.Scroll;

        return isEqSlot;
    }

    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        OnGamepadButtonSelected?.Invoke();
        //if(Item != null && !UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem item))
        //    Item.HighlightItem(true);

        NavigationMode currentCursorMode = UIController._instance.StateMonitor.GetCursorMode();

        switch(currentCursorMode)
        {
            case NavigationMode.Neutral:
                if (Item == null) return;

                Item.HighlightItem(true);
                break;

            case NavigationMode.ItemDrag:

                break;

            case NavigationMode.Upgrade:
                if(Item != null)
                {
                    Item.HighlightItem(true);
                    Item.ShowForgePrice(true);
                }
                break;
            case NavigationMode.Enhance:
                if(Item != null)
                {
                    Item.HighlightItem(true);

                    if (Item.e.stats[Stats.Rarity] < 3)
                        Item.ShowForgePrice(true);
                }
                break;

            case NavigationMode.Sell:
                
                break;
        }
    }

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        OnGamepadButtonDeselected?.Invoke();

        var currentCursorMode = UIController._instance.StateMonitor.GetCursorMode();

        //this should close currently opened tooltip
        switch (currentCursorMode)
        {
            case NavigationMode.Neutral:
                if (Item == null) return;

                Item.HighlightItem(false);
                break;

            case NavigationMode.ItemDrag:

                break;

            case NavigationMode.Upgrade:
                if(Item != null)
                {
                    Item.HighlightItem(false);
                    Item.ShowForgePrice(false);
                }
                break;
            case NavigationMode.Enhance:
                if(Item != null)
                {
                    Item.HighlightItem(false);
                    Item.ShowForgePrice(false);
                }
                break;
            case NavigationMode.Sell:
                break;
        }
    }

    public void HandleGamepadButtonPressed(Selectable selectable)
    {
        NavigationMode currentCursorMode = UIController._instance.StateMonitor.GetCursorMode();

        switch(currentCursorMode)
        {
            case NavigationMode.Neutral:
                if (Item == null) return;
                TryDragOrSell();
                break;

            case NavigationMode.ItemDrag:
                if (UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
                {
                    TryDrop(itemOnGamepad);
                }
                break;

            case NavigationMode.Upgrade:
                if (Item == null) return;
                Item.TryUpgrade();                
                break;

            case NavigationMode.Enhance:
                if (Item == null) return;
                Item.TryEnhance();
                break;

            case NavigationMode.Sell:
                if (Item == null) return;
                Debug.LogError($"Should add popup notification for selling");
                SellItem(Item);
                break;
        }         
    }

    private void CancelGamepadDrag()
    {
        inputHandler.OnNo.RemoveListener(CancelGamepadDrag);

        if (!UIController._instance.StateMonitor.TryGetItemOnGamepad(out var item)) return;

        item.GamepadFinishDrag(false);
        UIController._instance.PlayUIClick();
    }


    private int GetItemCost(DragItem itemToBuy)
    {
        int cost = (itemToBuy.e.stats[Stats.Rarity] + 1) * 60;

        if (CombatController._instance.Difficulty >= 7)
            cost += Mathf.RoundToInt(cost * .2f);

        return cost;
    }

    private bool IsInCombat() => CombatController._instance.entitiesInCombat.Count > 1;

    private void TryDrop(DragItem itemOnGamepad)
    {
        if (itemOnGamepad == Item)
        {
            itemOnGamepad.GamepadFinishDrag();
            return;
        }

        InitiateDroppingItem(itemOnGamepad);
        return;
    }

    private void TryDragOrSell()
    {
        if (Slot == Equipment.Slot.Merchant)
        {
            Debug.LogError($"Try buying item from shop");
            TryBuyItem(Item);
            return;
        }

        Item.GamepadStartDrag(this);
        inputHandler.OnNo.AddListener(CancelGamepadDrag);
    }

    private bool CanItemBeDroppedHere(DragItem itemToDrop)
    {
        if(IsInCombat() && !itemToDrop.canBeDragged)
        {
            OnCannotDragItemOnCombat(ErrorMessageManager.Errors.CombatMove); //consider invoke
            return false;
        }

        if (itemToDrop.IsShopItem())
        {
            // if item is coming from shop
            // check if we have enough gold AND the target slot is empty
            if(Item != null)
            {
                return false;
            }

            int currentGold = CombatController._instance.Player._gold;
            if (currentGold < GetItemCost(itemToDrop))
            {
                NotEnoughGoldEvent();
                return false;
            }
        }       

        if (!itemToDrop.HasSameSlotType(Slot) && IsCharacterEquipmentSlot())
        {
            return false;
        }

        if (Item != null && !Item.HasSameSlotType(itemToDrop.slotType) && (!itemToDrop.IsItemFromInventory() || Item.HasSameSlotType(itemToDrop.currentLocation.Slot)))
        {
            return false;
        }

        return CanAcceptItem;
    }
       

    private void InitiateDroppingItem(DragItem itemToDrop)
    {
        if (!CanItemBeDroppedHere(itemToDrop))
        {
            SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
            return;
        }

        inputHandler.OnNo.RemoveListener(CancelGamepadDrag);

        if (Slot == Equipment.Slot.Drop)
        {
            DiscardItem(itemToDrop);
            return;
        }
        else if(Slot == Equipment.Slot.Sell)
        {
            SellItem(itemToDrop);
            return;
        }

        if (itemToDrop.IsShopItem())
        {
            BuyItem(itemToDrop);
            return;
        }

        if (Item != null && Item != itemToDrop)
        {
            MoveItemOnThisSlotToItemToDropOriginSlot(Item, itemToDrop.currentLocation);                
        }

        AssignDroppedItemToSlot(itemToDrop);

        if (Slot != Equipment.Slot.All) //slot is equipment
        {
            //Debug.LogError($"This slot is not Slot.All. It's {Slot}. So we attempt to equip from inventory here!");
            EquipmentManager._instance.EquipFromInventory(Item.e);
            //di._rectTransform.position = _rt.position;
        }

        if (itemToDrop.slotType == Equipment.Slot.Consumable)
        {
            //Debug.Log("from inventory slot potion");
            EquipmentManager._instance.AddPotionToPotionBar((Consumable)itemToDrop.e);
        }
                
        LabelCheck();
        UIController._instance.PlayPlaceItem();
    }

    private bool TryBuyItem(DragItem itemToBuy)
    {
        //check if equipment slot for the item is empty
        var equipmentManager = EquipmentManager._instance;

        int currentGold = CombatController._instance.Player._gold;
        if (currentGold < GetItemCost(itemToBuy))
        {
            NotEnoughGoldEvent();
            return false;
        }

        if (equipmentManager.TryEquipItem(itemToBuy.e) || equipmentManager.TryPutItemToInventory(itemToBuy.e))
        {
            BuyItem(itemToBuy);
            return true;
        }

        Debug.LogError($"Inventory is full!");
        return false;
    }

    //for gamepad version, we shouldn't drag/drop the item to buy it, or should we?
    private void BuyItem(DragItem itemToDrop)
    {
        // if we do - gold
        CombatController._instance.Player._gold -= GetItemCost(itemToDrop);
        OnItemBought?.Invoke(this);
        BuyItemEvent(-GetItemCost(itemToDrop));
        
    }    

    private void SellItem(DragItem itemToSell)
    {
        if (itemToSell.currentLocation.Slot == Equipment.Slot.Merchant)
            return;

        int gold = CalculateGold(itemToSell.e, this);
        CombatController._instance.Player.GetGold(gold);

        itemToSell.currentLocation.Item = null;
        itemToSell.currentLocation.LabelCheck();
        EquipmentManager._instance.PoolItem(itemToSell);
        itemToSell.GamepadFinishDrag();
        UIController._instance.PlaySellItem();
    }

    private void DiscardItem(DragItem itemToDrop)
    {
        Debug.LogError($"Discarding item: {itemToDrop.name}");
        itemToDrop.currentLocation.Item = null;
        itemToDrop.currentLocation.LabelCheck();
        EquipmentManager._instance.PoolItem(itemToDrop);
        itemToDrop.GamepadFinishDrag();
        SoundManager.Instance.Play2DSFX(dropItem, dropItemVol, 1, .05f);
    }

    private void MoveItemOnThisSlotToItemToDropOriginSlot(DragItem itemToMove, InventorySlot targetSlot)
    {
        targetSlot.RemoveItemFromSlot();
        targetSlot.Item = itemToMove;

        itemToMove.transform.SetParent(targetSlot.transform.parent);
        itemToMove._rectTransform.anchoredPosition = targetSlot._rt.anchoredPosition;
        itemToMove._rectTransform.localScale = targetSlot._rt.localScale;
        
        itemToMove.currentLocation.RemoveItemFromSlot();
        itemToMove.currentLocation.LabelCheck();
        itemToMove.currentLocation = targetSlot;

        if(targetSlot.IsCharacterEquipmentSlot())
        {
            if(targetSlot.Slot != itemToMove.slotType)
            {
                Debug.LogError($"Invalid item move, targetSlot is not All or {itemToMove.slotType}");
                return;
            }

            EquipmentManager._instance.EquipFromInventory(itemToMove.e);
        }
    }



    private void RemoveItemFromSlot()
    {
        if(IsCharacterEquipmentSlot())
        {
            EquipmentManager._instance.UnEquipItem(Item.e);
        }

        Item = null;
    }

    private void AssignDroppedItemToSlot(DragItem di)
    {
        di.transform.SetParent(_rt.parent);
        di._rectTransform.anchoredPosition = _rt.anchoredPosition;
        di._rectTransform.localScale = _rt.localScale;

        if(di.currentLocation.Item == di) //if not swapping with other item
        {
            di.currentLocation.RemoveItemFromSlot();
            di.currentLocation.LabelCheck();
        }

        di.currentLocation = this;
        Item = di;
        di.GamepadFinishDrag();
    }

    public void HandleMouseDropOnOccupiedSlot()
    {
        if (stateMonitor.GetCursorMode() != NavigationMode.ItemDrag)
            return;

        if (stateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
        {
            InitiateDroppingItem(itemOnGamepad);
            return;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        NavigationMode currentCursorMode = stateMonitor.GetCursorMode();

        switch (currentCursorMode)
        {
            case NavigationMode.ItemDrag:
                if (stateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
                {
                    TryDrop(itemOnGamepad);
                }
                break;
        }

        return;
        //AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy, 1,1);
        DragItem item = eventData.pointerDrag.GetComponent<DragItem>();

        if (item.canBeDragged == false && (Slot != Equipment.Slot.Drop || Slot != Equipment.Slot.Sell))
        {
            if (CombatController._instance.entitiesInCombat.Count > 1)
            {
                OnCannotDragItemOnCombat(ErrorMessageManager.Errors.CombatMove);
                SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
                return;
            }
        }
        if (CanAcceptItem == false)
        {
            SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
            return;
        }
        
        if (eventData.pointerDrag != null)
        {
            DragItem di = eventData.pointerDrag.GetComponent<DragItem>();
            
            if (Slot == Equipment.Slot.Drop)
            {
                DiscardItem(di);
                return;
            }
            if (Slot == Equipment.Slot.Sell)
            {
                if(item.currentLocation.Slot == Equipment.Slot.Merchant)
                    return;

                Debug.LogError($"Should add popup notification for selling!");
                // calculate gold
                int gold = CalculateGold(di.e, this);
                //character add gold
                CombatController._instance.Player.GetGold(gold);
                
                di.currentLocation.Item = null;
                di.currentLocation.LabelCheck();
                EquipmentManager._instance.PoolItem(di);
                UIController._instance.PlaySellItem();
                

                return;
            }
            

            if (Slot != di.slotType && Slot != Equipment.Slot.All)
            {
                SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
                return;
            }

            if (di.currentLocation.Slot == Equipment.Slot.Merchant)
            {
                // if item is coming from shop
                // check if we have enough gold if we dont return
                if (Item != null)
                {
                    Debug.Log(Item.name);
                    return;
                }
                    
                
                int currentGold = CombatController._instance.Player._gold;
                int cost = (di.e.stats[Stats.Rarity] + 1) * 60;

                if (CombatController._instance.Difficulty >= 7)
                    cost += Mathf.RoundToInt(cost * .2f);
                
                if (currentGold < cost)
                {
                    SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
                    NotEnoughGoldEvent();
                    return;
                }
                // if we do - gold
                CombatController._instance.Player._gold -= cost;
                BuyItemEvent(-cost);
            }
            
            if (Item == null)
            {
                AssignDroppedItemToSlot(di);

                if (Slot != Equipment.Slot.All)
                {
                    EquipmentManager._instance.EquipFromInventory(Item.e);
                    //di._rectTransform.position = _rt.position;
                }
                else
                {
                    // unequip
                    EquipmentManager._instance.UnEquipItem(Item.e);
                }

                if (di.slotType == Equipment.Slot.Consumable)
                {
                    //Debug.Log("from inventory slot potion");
                    EquipmentManager._instance.AddPotionToPotionBar((Consumable)di.e);
                }
                
            }
            LabelCheck();
            UIController._instance.PlayPlaceItem();

        }
    }



    public void NotEnoughGoldEvent()
    {
        NotEnoughGold(ErrorMessageManager.Errors.NotEnoughGold);
    }

    public void BuyItemEvent(int i)
    {
        OnBoughtItem(ErrorMessageManager.Errors.LoseGold, -i);
        
    }

    // public bool canBeDragged = true;
    // private void AdjustDragabilityBasedOnEnergy( Character c, int cur, int max, int amount)
    // {
    //     if (!c.isPlayerCharacter)
    //     {
    //         return;
    //     }
    //     if (CombatController._instance.entitiesInCombat.Count <= 1)
    //     {
    //         canBeDragged = true;
    //         return;
    //     }
    //
    //     if (Slot == Equipment.Slot.Scroll && RelicManager._instance.CheckRelic(RelicType.Relic1))
    //     {
    //         cur = 1;
    //     }
    //     if (Slot == Equipment.Slot.OneHander && RelicManager._instance.CheckRelic(RelicType.Relic2))
    //     {
    //         cur = 1;
    //     }
    //     
    //     if (cur <= 0)
    //     {
    //         canBeDragged = false;
    //     }
    //     else
    //     {
    //         canBeDragged = true;
    //
    //     }
    // }
   
    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        //Character.UpdateEnergy += AdjustDragabilityBasedOnEnergy;
        LabelCheck();
        //CombatController.EndCombatEvent += EndCombat;
        stateMonitor = UIController._instance.StateMonitor;
    }
    private void OnDestroy()
    {
        //Character.UpdateEnergy -= AdjustDragabilityBasedOnEnergy;

        //CombatController.EndCombatEvent -= EndCombat;

    }
    
    public enum SellShopType
    {
        Armor,
        Scrolls,
        Weapons,
        FullHalfPrice,
        Relics,
        Potions,
        Blacksmith,
        None,


    }

    private float timer = -1;
    private void LateUpdate()
    {
        if (timer < 0)
            return;
        if(Item == null)
            return;
        if (Item._rectTransform.localScale != _rt.localScale)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                if(Item != null)
                {
                    Item._rectTransform.localScale = _rt.localScale;
                    //Item.GetComponent<DragItemHoverEffect>().ResetScale();
                }
            }
        }
        
    }

    public int CalculateGold(Equipment e, InventorySlot SellButton)
    {
        // get rarity

        float costReduction= .25f;
        
        switch (SellButton.SellType)
        {
            case SellShopType.Armor:
                costReduction = .5f;
                if (e.slot == Equipment.Slot.Consumable || e.slot == Equipment.Slot.Scroll || e.slot == Equipment.Slot.OneHander)
                {
                    costReduction = .25f;
                }
                break;

            case SellShopType.Weapons:
                if (e.slot == Equipment.Slot.OneHander)
                {
                    costReduction = .5f;
                }
                break;
            case SellShopType.Scrolls:
                if (e.slot == Equipment.Slot.Scroll)
                {
                    costReduction = .5f;
                }
                break;
            case SellShopType.Potions:
                if (e.slot == Equipment.Slot.Consumable)
                {
                    costReduction = .5f;
                }
                break;
            case SellShopType.FullHalfPrice:
                costReduction = .5f;
                break;



        }
        int rarity = e.stats[Stats.Rarity] + 1;

        return Mathf.RoundToInt((60 * rarity) * costReduction);


    }




    // private void StartCombat()
    // {
    //     canBeDragged = false;
    //     //.Log("can no longer drag");
    // }
    // private void EndCombat()
    // {
    //     canBeDragged = true;
    // }
    // private void Start()
    // {
    //     LabelCheck();
    //
    //     CombatController.StartCombatEvent += StartCombat;
    //     CombatController.EndCombatEvent += EndCombat;
    // }
    // private void OnDestroy()
    // {
    //     CombatController.StartCombatEvent -= StartCombat;
    //     CombatController.EndCombatEvent -= EndCombat;
    //
    // }
}


