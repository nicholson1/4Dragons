using DFG.UIHandling;
using ImportantStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zak.UISystem;
using static ImportantStuff.Equipment;
using static UnityEditor.Progress;

public class InventorySlot : ButtonListener, IDragListener, IDropListener
{
    public override event Action OnGamepadButtonSelected;
    public override event Action OnGamepadButtonDeselected;

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

    private Vector2 gamepadDragOffset = new Vector2(-50f, 50f);
    private Coroutine draggingRoutine = null;

    public void LabelCheck()
    {
        if (SlotLable == null)
        {
            SlotLable = GetComponentInChildren<TextMeshProUGUI>();
            background = GetComponent<Image>();
            if(background != null)
                baseColor = background.color;
        }

        if (Slot == Equipment.Slot.Drop || Slot == Equipment.Slot.Sell)
        {
            return;
        }

        if (Item != null)
        {
            SlotLable.gameObject.SetActive(false);
            background.color = ToolTipManager._instance.rarityColors[Item.e.stats[Stats.Rarity]];
            background.color = new Color(background.color.r, background.color.g,background.color.b, baseColor.a);
            Item._rectTransform.localScale = _rt.localScale;


            //change color based on the rarity

        }
        else
        {
            if (Slot == Equipment.Slot.OneHander)
            {
                SlotLable.text = "Weapon";
            }
            else if(Slot == Equipment.Slot.Merchant)
            {
                SlotLable.text = "Sold!";
            }
            else if (Slot == Equipment.Slot.All)
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
            else
            {
                SlotLable.text = Slot.ToString();
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
    public bool IsMerchantSlot() => Slot == Equipment.Slot.Merchant;
    public bool IsRelicSlot() => Slot == Equipment.Slot.Relic;
    public bool IsAllSlot() => Slot == Equipment.Slot.All;
    public bool IsAllKindSlot() => IsDiscardSlot() || IsSellSlot() || IsUpgradeSlot() || IsMerchantSlot() || IsRelicSlot() || IsAllSlot();
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
    private bool IsInCombat() => CombatController._instance.entitiesInCombat.Count > 1;

  
    private void StartGamepadDrag(IDraggablePayload itemToDrag)
    {
        if (draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }

        stateMonitor.SetItemOnGamepad(itemToDrag);
        inputHandler.OnNo.AddListener(OnCancelPerformed);
        draggingRoutine = StartCoroutine(GamepadDragRoutine(itemToDrag));
    }

    private void EndGamepadDragRoutine()
    {
        if(draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }
    }

    public void OnHandleInterruption()
    {
        if(stateMonitor.GetUINavigationMode() == NavigationMode.MoveItem)
        {
            OnCancelPerformed();
        }
    }

    private IEnumerator GamepadDragRoutine(IDraggablePayload payload)
    {
        var itemToDrag = payload as DragItem;
        itemToDrag._rectTransform.anchoredPosition += gamepadDragOffset;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        //RectTransform currentRectTransform = currentSelected.transform as RectTransform;
        while (stateMonitor.GetUINavigationMode() == NavigationMode.MoveItem)
        {
            
            //TODO: handle if during this process, mouse detected
            //if mouse detected, cancel
            if (currentSelected != EventSystem.current.currentSelectedGameObject)
            {
                currentSelected = EventSystem.current.currentSelectedGameObject;

                itemToDrag.transform.position = currentSelected.transform.position;
                itemToDrag._rectTransform.anchoredPosition += gamepadDragOffset;
            }

            yield return null;
        }

        EndGamepadDragRoutine();
    }

    public override void OnButtonSelected(Selectable selectable)
    {
        OnGamepadButtonSelected?.Invoke();
        //if(Item != null && !UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem item))
        //    Item.HighlightItem(true);

        NavigationMode currentCursorMode = UIController._instance.StateMonitor.GetUINavigationMode();

        switch(currentCursorMode)
        {
            case NavigationMode.Neutral:
                if (Item == null) return;

                Item.HighlightItem(true);
                break;

            case NavigationMode.MoveItem:

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

    public override void OnButtonDeselected(Selectable selectable)
    {
        OnGamepadButtonDeselected?.Invoke();

        var currentCursorMode = stateMonitor.GetUINavigationMode();

        //this should close currently opened tooltip
        switch (currentCursorMode)
        {
            case NavigationMode.Neutral:
                if (Item == null) return;

                Item.HighlightItem(false);
                break;

            case NavigationMode.MoveItem:

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

    public override void OnButtonPressed(Selectable selectable, InputSource source)
    {

        NavigationMode currentCursorMode = stateMonitor.GetUINavigationMode();

        switch(currentCursorMode)
        {
            case NavigationMode.Neutral:
                if (Item == null) return;
                if(source == InputSource.Gamepad && CanBeginDrag(out IDraggablePayload payload))
                {
                    DragItem item = payload as DragItem;

                    if(IsMerchantSlot())
                    {
                        TryBuyItem(item);
                    }

                    StartGamepadDrag(item);
                }

                break;

            case NavigationMode.MoveItem:
                if(source == InputSource.Gamepad)
                {
                    if (UIController._instance.StateMonitor.TryGetItemOnGamepad(out IDraggablePayload itemOnGamepad) && itemOnGamepad is DragItem)
                    {
                        var item = itemOnGamepad as DragItem;
                        InitiateGamepadProcessDrop(itemOnGamepad, item.currentLocation);
                    }
                }
                break;

            case NavigationMode.Upgrade:
                if (Item == null) return;
                //popup onYes tryUpgrade
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

    //Called by the recipient
    private void InitiateGamepadProcessDrop(IDraggablePayload payload, IButtonListener origin)
    {
        var originSlot = origin as InventorySlot;
        originSlot.EndGamepadDragRoutine();

        var selected = EventSystem.current.currentSelectedGameObject;

        var targetDrop = selected.GetComponentInParent<InventorySlot>();

        if(targetDrop.CanAcceptDrop(payload))
        {
            var dragResult = new DragResult(true, payload, targetDrop as IDropListener);
            originSlot.OnDragCompleted(dragResult);
        }
        else
        {
            var dragResult = new DragResult(false, payload, targetDrop as IDropListener);
            originSlot.OnDragCompleted(dragResult);
        }

    }


    public void OnCancelPerformed()
    {
        inputHandler.OnNo.RemoveListener(OnCancelPerformed);

        var navigationMode = stateMonitor.GetUINavigationMode();

        if (stateMonitor.GetUINavigationMode() != NavigationMode.MoveItem) return;

        if (!stateMonitor.TryGetItemOnGamepad(out var item)) return;

        var result = new DragResult(false, item as IDraggablePayload, null);
        OnDragCompleted(result);
        //item.PrepItemForReturn();
        //stateMonitor.ClearItemOnGamepad();
        UIController._instance.PlayUIClick();
    }

    private int GetItemCost(DragItem itemToBuy)
    {
        int cost = (itemToBuy.e.stats[Stats.Rarity] + 1) * 60;

        if (CombatController._instance.Difficulty >= 7)
            cost += Mathf.RoundToInt(cost * .2f);

        return cost;
    }
      
    private bool TryDrop(DragItem itemToDrop)
    {
        if (Item != null && itemToDrop == Item)
        {
            Debug.LogError($"itemToDrop == Item or Item is NULL, TryDrop returns false");
            return false;
        }
                
        return InitiateDroppingItem(itemToDrop);
    }

    private void TryDragOrBuy(InputSource source)
    {
        if (Slot == Equipment.Slot.Merchant)
        {
            Debug.LogError($"Try buying item from shop");
            TryBuyItem(Item);
            return;
        }

        Item.GamepadStartDrag(this, source);
        //inputHandler.OnNo.AddListener(OnCancelPerformed);
    }

    private bool CanItemBeDroppedHere(DragItem itemToDrop)
    {
        if(IsInCombat() && !itemToDrop.CanBeDraggedInCombat())
        {
            OnCannotDragItemOnCombat(ErrorMessageManager.Errors.CombatMove); //consider invoke
            Debug.LogError($"CanItemBeDropped false - is in combat");
            return false;
        }

        //item comes from merchant
        if (itemToDrop.IsShopItem())
        {
            // check if 1. the target slot is not merchant, drop, or sell
            if (this.Slot is Equipment.Slot.Merchant or Equipment.Slot.Sell or Equipment.Slot.Drop) 
                return false;

            // 2. the target slot is empty
            if (Item != null)
                return false;

            // 3. we don't have enough gold
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
       
    //check if this is still  in use
    private bool InitiateDroppingItem(DragItem itemToDrop)
    {
        if (!CanItemBeDroppedHere(itemToDrop))
        {
            UIController._instance.PlayUIError();
            return false;
        }

        //inputHandler.OnNo.RemoveListener(OnCancelPerformed);

        if (Slot == Equipment.Slot.Drop)
        {
            DiscardItem(itemToDrop);            
            return true;
        }
        else if(Slot == Equipment.Slot.Sell)
        {
            SellItem(itemToDrop);
            return true;
        }

        if (itemToDrop.IsShopItem())
        {
            BuyItem(itemToDrop);
            return true;
        }

        if (Item != null && Item != itemToDrop)
        {
            MoveItemOnThisSlotToItemToDropOriginSlot(Item, itemToDrop.currentLocation);                
        }

        AssignDroppedItemToSlot(itemToDrop, this);

        return true;
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

        if (itemToBuy.e.isRelic)
        {
            RelicManager._instance.SelectRelic(itemToBuy.e);
            CombatController._instance.Player.UpdateStats();
            UIController._instance.PlayGetRelic();
            BuyItem(itemToBuy);
            return true;
        }
        

        if (equipmentManager.TryEquipItem(itemToBuy.e) || equipmentManager.TryCreateItemInInventory(itemToBuy.e))
        {
            BuyItem(itemToBuy);
            return true;
        }

        Debug.LogError($"Inventory is full!");
        return false;
    }

    private void BuyDroppedItem(DragItem itemToBuy, InventorySlot destinationSlot)
    {
        //at this point we validate the buy eligibility
        Debug.LogError($"BuyDroppedItem() should be called once!");

        itemToBuy.currentLocation.RemoveItemFromSlot();
        itemToBuy.currentLocation.LabelCheck();

        itemToBuy.PrepItemForDrop(destinationSlot);

        if (destinationSlot.IsCharacterEquipmentSlot()) //slot is equipment
        {
            EquipmentManager._instance.EquipItem(itemToBuy.e);
        }
        else if (destinationSlot.IsAllSlot())
        {
            EquipmentManager._instance.AddItemToInventory(itemToBuy.e);
        }

        BuyItem(itemToBuy);
    }


    //for gamepad version, we shouldn't drag/drop the item to buy it, or should we?
    private void BuyItem(DragItem itemToDrop)
    {
        // if we do - gold
        Debug.LogError($"BuyItem - should be called once!");
        CombatController._instance.Player._gold -= GetItemCost(itemToDrop);
        OnItemBought?.Invoke(this);
        BuyItemEvent(-GetItemCost(itemToDrop));
        LabelCheck();        
        
    }    

    private void SellItem(DragItem itemToSell)
    {
        if (itemToSell.currentLocation.Slot == Equipment.Slot.Merchant) //guard on selling Items come from Merchant slot
            return;

        int gold = CalculateGold(itemToSell.e, this);
        CombatController._instance.Player.GetGold(gold);

        itemToSell.currentLocation.Item = null;
        itemToSell.currentLocation.LabelCheck();
        itemToSell.PrepForRemove();
        EquipmentManager._instance.PoolItem(itemToSell);
        UIController._instance.PlaySellItem();
    }

    private void DiscardItem(DragItem itemToDrop)
    {
        Debug.LogError($"Discarding item: {itemToDrop.name}");
        itemToDrop.currentLocation.Item = null;
        itemToDrop.currentLocation.LabelCheck();
        itemToDrop.PrepForRemove();
        EquipmentManager._instance.PoolItem(itemToDrop);
        UIController._instance.PlayDiscardItem();
        
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
        //if(IsAllSlot())
        //{
        //    EquipmentManager._instance.PoolItem(Item);
        //}

        Item = null;
    }

    private void AssignDroppedItemToSlot(DragItem di, InventorySlot destinationSlot)
    {
        Debug.LogError($"{gameObject.name} - Assign dropped {di.e.name} to slot {destinationSlot.name}");

        destinationSlot.Item = di;

        string itemName = Item != null ? Item.name : "NULL";
        Debug.LogError($"{gameObject.name} - Item = {itemName}");


        if (di.currentLocation.Item == di) //if not swapping with other item
        {
            di.currentLocation.RemoveItemFromSlot();
            di.currentLocation.LabelCheck();
        }

        di.currentLocation = destinationSlot;

        if (destinationSlot.IsCharacterEquipmentSlot()) //slot is equipment
        {            
            EquipmentManager._instance.EquipItem(di.e);
        }
        else if(destinationSlot.IsAllSlot() && !this.IsAllSlot())
        {
            EquipmentManager._instance.AddItemToInventory(di.e);
        }

        di.PrepItemForDrop(destinationSlot);
        EventSystem.current.SetSelectedGameObject(destinationSlot.GetComponentInChildren<Selectable>().gameObject);

        //if (di.slotType == Equipment.Slot.Consumable)
        //{
        //    //Debug.Log("from inventory slot potion");
        //    EquipmentManager._instance.AddPotionToPotionBar((Consumable)di.e);
        //}

        LabelCheck();
        UIController._instance.PlayPlaceItem();
    }


    /* Old version OnDrop 
    //public void OnDrop(PointerEventData eventData)
    //{
    //    NavigationMode currentCursorMode = stateMonitor.GetUINavigationMode();

    //    switch (currentCursorMode)
    //    {
    //        case NavigationMode.MoveItem:
    //            if (stateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
    //            {
    //                TryDrop(itemOnGamepad);
    //            }
    //            break;
    //    }

    //    return;
    //    //AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy, 1,1);
    //    DragItem item = eventData.pointerDrag.GetComponent<DragItem>();

    //    if (item.canBeDragged == false && (Slot != Equipment.Slot.Drop || Slot != Equipment.Slot.Sell))
    //    {
    //        if (CombatController._instance.entitiesInCombat.Count > 1)
    //        {
    //            OnCannotDragItemOnCombat(ErrorMessageManager.Errors.CombatMove);
    //            SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
    //            return;
    //        }
    //    }
    //    if (CanAcceptItem == false)
    //    {
    //        SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
    //        return;
    //    }
        
    //    if (eventData.pointerDrag != null)
    //    {
    //        DragItem di = eventData.pointerDrag.GetComponent<DragItem>();
            
    //        if (Slot == Equipment.Slot.Drop)
    //        {
    //            DiscardItem(di);
    //            return;
    //        }
    //        if (Slot == Equipment.Slot.Sell)
    //        {
    //            if(item.currentLocation.Slot == Equipment.Slot.Merchant)
    //                return;

    //            Debug.LogError($"Should add popup notification for selling!");
    //            // calculate gold
    //            int gold = CalculateGold(di.e, this);
    //            //character add gold
    //            CombatController._instance.Player.GetGold(gold);
                
    //            di.currentLocation.Item = null;
    //            di.currentLocation.LabelCheck();
    //            EquipmentManager._instance.PoolItem(di);
    //            UIController._instance.PlaySellItem();
                

    //            return;
    //        }
            

    //        if (Slot != di.slotType && Slot != Equipment.Slot.All)
    //        {
    //            SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
    //            return;
    //        }

    //        if (di.currentLocation.Slot == Equipment.Slot.Merchant)
    //        {
    //            // if item is coming from shop
    //            // check if we have enough gold if we dont return
    //            if (Item != null)
    //            {
    //                Debug.Log(Item.name);
    //                return;
    //            }
                    
                
    //            int currentGold = CombatController._instance.Player._gold;
    //            int cost = (di.e.stats[Stats.Rarity] + 1) * 60;

    //            if (CombatController._instance.Difficulty >= 7)
    //                cost += Mathf.RoundToInt(cost * .2f);
                
    //            if (currentGold < cost)
    //            {
    //                SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
    //                NotEnoughGoldEvent();
    //                return;
    //            }
    //            // if we do - gold
    //            CombatController._instance.Player._gold -= cost;
    //            BuyItemEvent(-cost);
    //        }
            
    //        if (Item == null)
    //        {
    //            AssignDroppedItemToSlot(di);

    //            if (Slot != Equipment.Slot.All)
    //            {
    //                EquipmentManager._instance.EquipFromInventory(Item.e);
    //                //di._rectTransform.position = _rt.position;
    //            }
    //            else
    //            {
    //                // unequip
    //                EquipmentManager._instance.UnEquipItem(Item.e);
    //            }

    //            if (di.slotType == Equipment.Slot.Consumable)
    //            {
    //                //Debug.Log("from inventory slot potion");
    //                EquipmentManager._instance.AddPotionToPotionBar((Consumable)di.e);
    //            }
                
    //        }
    //        LabelCheck();
    //        UIController._instance.PlayPlaceItem();

    //    }
    //}
    */


    public void NotEnoughGoldEvent()
    {
        NotEnoughGold(ErrorMessageManager.Errors.NotEnoughGold);
    }

    public void BuyItemEvent(int i)
    {
        OnBoughtItem(ErrorMessageManager.Errors.LoseGold, -i);
        
    }

    /*
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
   */

    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        stateMonitor = UIController._instance.StateMonitor;
        //Character.UpdateEnergy += AdjustDragabilityBasedOnEnergy;
        LabelCheck();
        //CombatController.EndCombatEvent += EndCombat;

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


    //INTERFACE IMPLEMENTATION
    public bool CanBeginDrag(out IDraggablePayload payload)
    {
        NavigationMode currentUINavigationMode = stateMonitor.GetUINavigationMode();
        payload = null;

        if (currentUINavigationMode != NavigationMode.Neutral)
        {
            Debug.LogError($"ERROR: Drag should begin only from NavigationMode.Neutral");
            return false;
        }

        if (Item == null || (IsInCombat() && !Item.CanBeDraggedInCombat()))
            return false;

        if (Item.e.isRelic) return false;

        payload = Item.GetComponent<IDraggablePayload>();
        
        stateMonitor.SetItemOnGamepad(Item);
        return true;
    }

    //handle the finalization of the drag/drop
    public void OnDragCompleted(DragResult result)
    {
        stateMonitor.ClearItemOnGamepad();

        var droppedItem = result.Payload as DragItem;

        if (!result.Success)
        {            
            droppedItem.PrepItemForReturn();
            EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Selectable>().gameObject);
            return;
        }

        var destinationSlot = result.DropDestination as InventorySlot;

        //here the swap is already eligible, so we just need to know if destination has item
        if(destinationSlot.Item == null) 
        {
            // callback to a popup here!
            if(IsMerchantSlot())
            {
                Debug.LogError($"ITEM BOUGHT!");
                destinationSlot.BuyDroppedItem(droppedItem, destinationSlot);
                return;
            }

            //just drop, sell, or discard
            if(destinationSlot.IsSellSlot())
            {
                Debug.LogError($"ITEM SOLD");
                destinationSlot.SellItem(droppedItem);
            }
            else if(destinationSlot.IsDiscardSlot())
            {
                Debug.LogError($"ITEM DUMPED!");
                destinationSlot.DiscardItem(droppedItem);
            }
            else if(destinationSlot.IsAllSlot() || droppedItem.slotType == destinationSlot.Slot)
            {
                Debug.LogError($"ITEM EQUIPPED OR PUT IN INVENTORY!");
                AssignDroppedItemToSlot(droppedItem, destinationSlot);
            }
        }
        else 
        {
            Debug.LogError($"{gameObject.name} successfully SWAPPING item with {destinationSlot.name}");

            var cachedOriginSlot = this;
            var cachedDroppedItem = droppedItem;
            destinationSlot.MoveItemOnThisSlotToItemToDropOriginSlot(destinationSlot.Item, cachedOriginSlot);
            destinationSlot.AssignDroppedItemToSlot(cachedDroppedItem, destinationSlot);

        }
    }


    //DROP FUNCTIONALITIES
    public bool CanAcceptDrop(IDraggablePayload payload)
    {
        if (payload is not DragItem droppedItem)
        {
            Debug.LogError($"FALSE: Payload is not DragItem!");
            return false;
        }

        if (droppedItem == Item)
        {
            Debug.LogError($"FALSE: droppedItem is itself!");
            return false; // Cannot drop to self
        }

        if (IsRelicSlot())
        {
            Debug.LogError($"FALSE: destination is relic slot!");
            return false;
        }
        if (IsMerchantSlot())
        {
            Debug.LogError($"FALSE: destination is merchant slot");
            return false;
        }

        if (droppedItem.currentLocation.IsMerchantSlot())
            return CanBuyDropCandidate(droppedItem);

        if (IsInCombat() && !droppedItem.CanBeDraggedInCombat())
        {
            OnCannotDragItemOnCombat(ErrorMessageManager.Errors.CombatMove);
            Debug.LogError($"FALSE: CanItemBeDropped false - is in combat");
            return false;
        }

        bool slotMatches = IsAllSlot() || IsDiscardSlot() || IsSellSlot() || Slot == droppedItem.slotType;
        if (!slotMatches)
        {
            Debug.LogError($"FALSE: destination slot is not ALL or droppedItem slottype");
            return false;
        }

        return Item == null || CanSwap(droppedItem, Item);
    }



    private bool CanSwap(DragItem droppedItem, DragItem currentItem)
    {
        InventorySlot droppedItemOrigin = droppedItem.currentLocation;
                   
        return CanHoldItem(currentItem);
    }

    private bool CanHoldItem(DragItem item)
    {
        return Slot == Slot.All || Slot == item.slotType;
    }

    private bool CanBuyDropCandidate(DragItem itemToBuy)
    {
        Debug.LogError($"CanBuyDropCandidate is being called!");
        //Can buy?

        //1. Target slot (this dropListener) is not merchant, sell, or discard?
        if (IsMerchantSlot() || IsSellSlot() || IsDiscardSlot()) return false;

        //2. Target slot is empty?
        if (Item != null) return false;

        //3. Target slot is inventory slot (All) or same slot type?
        if (!IsAllSlot() && !itemToBuy.HasSameSlotType(this.Slot))
            return false;

        //4. Have enough gold?
        var equipmentManager = EquipmentManager._instance;

        int currentGold = CombatController._instance.Player._gold;
        if (currentGold < GetItemCost(itemToBuy))
        {
            NotEnoughGoldEvent();
            return false;
        }

        return true;
    }
}


