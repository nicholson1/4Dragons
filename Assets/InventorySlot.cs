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

    public void LabelCheck()
    {
        if (SlotLable == null)
        {
            SlotLable = GetComponentInChildren<TextMeshProUGUI>();
            background = GetComponent<Image>();
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

            background.color = baseColor;
        }

        timer = 2f;

    }

    public bool IsDiscardSlot() => Slot == Equipment.Slot.Drop;
    public bool IsSellSlot() => Slot == Equipment.Slot.Sell;
    public bool IsUpgradeSlot() => Slot == Equipment.Slot.Upgrade;
    public bool IsShopSlot() => Slot == Equipment.Slot.Sold;
    public bool IsRelicSlot() => Slot == Equipment.Slot.Relic;
    public bool IsAllSlot() => Slot == Equipment.Slot.All;
    public bool IsAllKindSlot() => IsDiscardSlot() || IsSellSlot() || IsUpgradeSlot() || IsShopSlot() || IsRelicSlot() || IsAllSlot();


    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        OnGamepadButtonSelected?.Invoke();
        if(Item != null && !UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem item))
            Item.HighlightItem(true);

        //this should open tooltip if it's available
    }

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        OnGamepadButtonDeselected?.Invoke();
        if (Item != null && !UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem item))
            Item.HighlightItem(false);

        //this should close currently opened tooltip
    }

    public void HandleGamepadButtonPressed(Selectable selectable)
    {
        Debug.Log($"{gameObject.name} - {Slot} button PRESSED");
        
        //pressing inventory slot gamepad button while dragging item
        if (UIController._instance.StateMonitor.TryGetItemOnGamepad(out DragItem itemOnGamepad))
        {
            if(itemOnGamepad == Item)
            {
                itemOnGamepad.GamepadFinishDrag();
                return;
            }

            //try drop here
            InitiateDroppingItem(itemOnGamepad);
        }
        else
        {
            if (Item == null) return;
            
            Debug.Log($"ButtonPress detected on slot {gameObject.name} - {Slot} with item {Item.e.name} ");
            Item.GamepadStartDrag(this);

            inputHandler.OnNo.AddListener(CancelGamepadDrag);
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

    private bool CanItemBeDroppedHere(DragItem itemToDrop)
    {
        if(IsInCombat() && !itemToDrop.canBeDragged)
        {
            Debug.LogError($"Item can't be dropped: combat related");
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

    //for gamepad version, we shouldn't drag/drop the item to buy it
    private void BuyItem(DragItem itemToDrop)
    {
        // if we do - gold
        CombatController._instance.Player._gold -= GetItemCost(itemToDrop);
        BuyItemEvent(-GetItemCost(itemToDrop));
    }    

    private void SellItem(DragItem itemToSell)
    {
        if (itemToSell.currentLocation.Slot == Equipment.Slot.Sold)
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

    public void OnDrop(PointerEventData eventData)
    {
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
                if(item.currentLocation.Slot == Equipment.Slot.Sold)
                    return;
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

            if (di.currentLocation.Slot == Equipment.Slot.Sold)
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


