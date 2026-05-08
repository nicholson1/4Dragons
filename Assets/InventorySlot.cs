using DFG.UIHandling;
using ImportantStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static ImportantStuff.Equipment;
using static Unity.VisualScripting.Member;
//using UnityEngine.UIElements;

public class InventorySlot : ButtonDraggableListener
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
    private bool IsInCombat() => CombatController._instance.entitiesInCombat.Count > 1;

    public override bool CanBeginDrag()
    {
        NavigationMode currentUINavigationMode = stateMonitor.GetUINavigationMode();

        if (currentUINavigationMode != NavigationMode.Neutral)
        {
            Debug.LogError($"ERROR: Drag should begin only from NavigationMode.Neutral");
            return false;
        }

        if (Item == null || (IsInCombat() && !Item.CanBeDraggedInCombat()))
            return false;

        return true;
    }

    public override void BeginDrag(Button button, InputSource source)
    {
        Debug.LogError($"Can begin drag, therefore {this.gameObject.name} begin drag");
        UIController._instance.PlayStartDragItem();
        stateMonitor.SetItemOnGamepad(Item);
        Item.PrepItemForDrag();

        if(source == InputSource.Gamepad)
        {
            StartGamepadDrag(Item);
        }
    }

    private void StartGamepadDrag(DragItem itemToDrag)
    {
        if (draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }

        Debug.LogError($"{gameObject.name} starting gamepad drag routine");
        draggingRoutine = StartCoroutine(GamepadDragRoutine(itemToDrag));
    }

    private void EndGamepadDrag()
    {
        if(draggingRoutine != null)
        {
        Debug.LogError($"Stopping drag coroutine");
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }
    }

    private IEnumerator GamepadDragRoutine(DragItem itemToDrag)
    {
        itemToDrag._rectTransform.anchoredPosition += gamepadDragOffset;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        RectTransform currentRectTransform = currentSelected.GetComponent<RectTransform>();
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
    }

    public override void OnDrag(Vector2 dragPosition)
    {
        if (stateMonitor.GetUINavigationMode() == NavigationMode.MoveItem)
        {
            Item._rectTransform.position = dragPosition;
            return;
        }        
    }

    public override void FinalizeDragDrop(bool wasDropSuccess, GameObject origin, GameObject destination)
    {
        Debug.LogError($"FinalizeDragDrop from {origin.name} with result success = {wasDropSuccess} =========");
        if (draggingRoutine != null)
            EndGamepadDrag();
        if(!wasDropSuccess)
        {
            OnCancelPerformed();
        }
        else
        {
            var buttonToSelect = destination.GetComponentInChildren<Button>();
            EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
            stateMonitor.ClearItemOnGamepad();
        }
    }

    protected override bool GetDropResult(GameObject originSlotObj)
    {
        Debug.LogError($"{gameObject.name} - GetDropResult originSlotObj = {originSlotObj.name}");
        InventorySlot originSlot = null;
        originSlot = originSlotObj.GetComponentInParent<InventorySlot>();
        Debug.LogError($"{gameObject.name} - originSlot null? {originSlot == null}");

        if(originSlot != null)
        {
            var itemToDrop = originSlot.Item;
            if (itemToDrop != null)
            {
                bool canDrop = TryDrop(itemToDrop);
                return canDrop;
            }            
        }

        return false;
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
                if(source == InputSource.Gamepad && CanBeginDrag())
                    BeginDrag(selectable as Button, source);
                break;

            case NavigationMode.MoveItem:
                if(source == InputSource.Gamepad)
                {
                    if (UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
                    {
                        InitiateGamepadProcessDrop(itemOnGamepad);
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

    private void InitiateGamepadProcessDrop(DragItem itemToDrop)
    {
        var selected = EventSystem.current.currentSelectedGameObject;

        var targetDrop = selected.GetComponentInParent<InventorySlot>();
        Debug.LogError($"initiate process gamepad drop on {targetDrop.name}");
        var originSlot = itemToDrop.currentLocation;
        var origin = originSlot.GetComponentInChildren<DraggableButtonExtender>();
        Debug.LogError($"origin = {itemToDrop.currentLocation.gameObject.name}");

        bool canProcessDrop = targetDrop != null && origin != null;
        Debug.LogError($"canProcessDrop? {canProcessDrop}");

        if (canProcessDrop)
        {
            targetDrop.ProcessDrop(targetDrop.transform.position, origin.gameObject);

            originSlot.FinalizeDragDrop(origin.WasDropSuccess(targetDrop), originSlot.gameObject, targetDrop.gameObject);
            
        }

        else
        {
            EndGamepadDrag();
            OnCancelPerformed();
        }
        //else cancel
    }

    public override void OnCancelPerformed()
    {
        if (stateMonitor.GetUINavigationMode() != NavigationMode.MoveItem) return;

        inputHandler.OnNo.RemoveListener(OnCancelPerformed);

        if (!stateMonitor.TryGetItemOnGamepad(out var item))
            return;

        item.PrepItemForReturn();
        stateMonitor.ClearItemOnGamepad();
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
        inputHandler.OnNo.AddListener(OnCancelPerformed);
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

            // 3. we have enough gold
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
       

    private bool InitiateDroppingItem(DragItem itemToDrop)
    {
        if (!CanItemBeDroppedHere(itemToDrop))
        {
            UIController._instance.PlayUIError();
            return false;
        }

        inputHandler.OnNo.RemoveListener(OnCancelPerformed);

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

        Item = null;
    }

    private void AssignDroppedItemToSlot(DragItem di, InventorySlot destinationSlot)
    {
        Debug.LogError($"Assign dropped {di.e.name} to slot {destinationSlot.name}");

        Item = di;

        if (di.currentLocation.Item == di) //if not swapping with other item
        {
            di.currentLocation.RemoveItemFromSlot();
            di.currentLocation.LabelCheck();
        }


        di.PrepItemForDrop(destinationSlot);

        
        if (Slot != Equipment.Slot.All) //slot is equipment
        {
            EquipmentManager._instance.EquipFromInventory(Item.e);
        }

        if (di.slotType == Equipment.Slot.Consumable)
        {
            //Debug.Log("from inventory slot potion");
            EquipmentManager._instance.AddPotionToPotionBar((Consumable)di.e);
        }

        LabelCheck();

        UIController._instance.PlayPlaceItem();
    }

    public void HandleMouseDropOnOccupiedSlot()
    {
        if (stateMonitor.GetUINavigationMode() != NavigationMode.MoveItem)
            return;

        if (stateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
        {
            InitiateDroppingItem(itemOnGamepad);
            return;
        }
    }

    /*
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


