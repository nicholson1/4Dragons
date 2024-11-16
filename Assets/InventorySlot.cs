using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Equipment.Slot Slot;
    public DragItem Item = null;
    [SerializeField] public RectTransform _rt;
    private TextMeshProUGUI SlotLable;
    private Image background;
    [SerializeField] private Color baseColor;

    public SellShopType SellType = SellShopType.None;
    public bool CanDropHere = true;

    
    public static event Action<ErrorMessageManager.Errors> CombatMove;
    public static event Action<ErrorMessageManager.Errors, int> SellItem;
    public static event Action<ErrorMessageManager.Errors> NotEnoughGold;

    [SerializeField] private AudioClip dropItem;
    [SerializeField] private float dropItemVol;
    //[SerializeField] private float placePitch;

    

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
            //background.color = ToolTipManager._instance.rarityColors[Item.e.stats[Equipment.Stats.Rarity]];
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
            else if(Slot != Equipment.Slot.All)
            {
                SlotLable.text = Slot.ToString();
            }
            else
            {
                if (CanDropHere)
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

    public void OnDrop(PointerEventData eventData)
    {
        //AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy, 1,1);

        DragItem item = eventData.pointerDrag.GetComponent<DragItem>();
        if (item.canBeDragged == false && (Slot != Equipment.Slot.Drop || Slot != Equipment.Slot.Sell))
        {
            if (CombatController._instance.entitiesInCombat.Count > 1)
            {
                CombatMove(ErrorMessageManager.Errors.CombatMove);
                SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
                return;
            }
        }
        if (CanDropHere == false)
        {
            SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
            return;
        }
        
        if (eventData.pointerDrag != null)
        {
            DragItem di = eventData.pointerDrag.GetComponent<DragItem>();
            
            if (Slot == Equipment.Slot.Drop)
            {
                di.currentLocation.Item = null;
                di.currentLocation.LabelCheck();
                EquipmentManager._instance.DropItem(di.e);
                Destroy(di.gameObject);
                SoundManager.Instance.Play2DSFX(dropItem, dropItemVol, 1, .05f);
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
                //trigger notification event
                //SellItem(ErrorMessageManager.Errors.GetGold, gold);

                
                
                di.currentLocation.Item = null;
                di.currentLocation.LabelCheck();
                EquipmentManager._instance.DropItem(di.e);
                UIController._instance.PlaySellItem();
                
                Destroy(di.gameObject);
                
                
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
                int cost = (di.e.stats[Equipment.Stats.Rarity] + 1) * 60;

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
                di.transform.SetParent(_rt.parent);
                di._rectTransform.anchoredPosition = _rt.anchoredPosition;
                di._rectTransform.localScale = _rt.localScale;
                di.currentLocation.Item = null;
                di.currentLocation.LabelCheck();
                di.currentLocation = this;
                Item = di;

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
                    Debug.Log("from inventory slot potion");
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
        SellItem(ErrorMessageManager.Errors.LoseGold, -i);
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
                Item._rectTransform.localScale = _rt.localScale;
                Item.GetComponent<UIHoverEffect>().ResetScale();
            }
        }
        
    }

    int CalculateGold(Equipment e, InventorySlot SellButton)
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
        int rarity = e.stats[Equipment.Stats.Rarity] + 1;

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


