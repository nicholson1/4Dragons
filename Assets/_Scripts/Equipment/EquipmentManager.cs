using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] public Character c;
    
    public static EquipmentManager _instance;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goldText;


    [SerializeField] private DragItem _dragItemPrefab;
    [SerializeField] private InventorySlot[] InventorySlots;
    [SerializeField] private Transform inventoryTransform;

    public InventorySlot[] InventorySlotsRef => InventorySlots;
    
    public static event Action<ErrorMessageManager.Errors> InventoryNotifications;
    public static event Action<Equipment> PotionCollected;


    [SerializeField] private StatDisplay[] _statDisplays;
    [SerializeField] private GameObject _potionHolder;
    [SerializeField] private PotionDrag PotionPrefab;
    private List<PotionDrag> PotionPool = new List<PotionDrag>();
    private List<PotionDrag> ActivePotions = new List<PotionDrag>();
    
    private List<DragItem> ItemPool = new List<DragItem>();
    private List<DragItem> ActiveItems = new List<DragItem>();

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
        Character.UpdateStatsEvent += UpdateStats;
    }
    private void OnDestroy()
    {
        Character.UpdateStatsEvent -= UpdateStats;
    }

    private void UpdateStats(Character c)
    {
        foreach (var kvp in c.GetStats())
        {
            if (kvp.Key != Stats.Rarity && kvp.Key != Stats.ItemLevel )
            {
                _statDisplays[((int)kvp.Key)-2].UpdateValues(kvp.Key, kvp.Value);
            }
        }

        levelText.text = "Level: " + c._level;
        if (c._gold < 0)
        {
            c._gold = 0;
        }
        goldText.text = c._gold.ToString();

    }

    public void EquipItemFromSelection(Equipment e, SelectionItem si)
    {
        
        // loop through char inventory
        bool equippedSlot = false;

        for (int invSloti = 0; invSloti < InventorySlots.Length; invSloti++)
        {
            //find the slot that has the item
            if (InventorySlots[invSloti].Slot == e.slot)
            {
                if (InventorySlots[invSloti].Slot == Equipment.Slot.OneHander || InventorySlots[invSloti].Slot == Equipment.Slot.Scroll)
                {
                    if (InventorySlots[invSloti + 1].Slot == InventorySlots[invSloti].Slot && InventorySlots[invSloti + 1].Item == null)
                    {
                        invSloti += 1;
                    }
                }
                if (InventorySlots[invSloti].Item == null )
                {
                    DragItem di = GetDragItem();
                    di.InitializeDragItem(e, InventorySlots[invSloti]);
                    //Debug.Log(c.GetStats()[Stats.CritChance]);
                    c._equipment.Add(e);
                    if (e.isWeapon)
                    {
                        Weapon x = (Weapon) e;
                        if (x.slot == Equipment.Slot.Scroll)
                        {
                            c._spellScrolls.Add(x);
                        }
                        else
                        {
                            c._weapons.Add(x);
                        }

                        if (c._weapons.Count > 1)
                        {
                            c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
                        }
                        else if (c._weapons.Count == 1)
                        {
                            c.EqMM.UpdateWeapon(c._weapons[0], null);
                        }
                        else
                        {
                            c.EqMM.UpdateWeapon(null, null);

                        }

                    }
                    c.EqMM.UpdateSlot(e);
                    c.UpdateStats();
                    //Debug.Log(c.GetStats()[Stats.CritChance]);

                    
                    si.RemoveSelection();
                    return;
                }
                
                InventorySlot slot = null;
                // check if we have an empty, if we do save that one
                for (int i = 10; i < InventorySlots.Length; i++)
                {
                    if (InventorySlots[i].Item == null)
                    {
                        slot = InventorySlots[i];
                        break;
                    }
                }

                if (slot == null)
                {
                    InventoryNotifications(ErrorMessageManager.Errors.InventoryFull);
                    return;
                }
                else
                {
                    //move current equiped item to inventory
                    //if weapon check slot + 1

                    if (e.isWeapon)
                    {
                        if (InventorySlots[Array.IndexOf(InventorySlots, InventorySlots[invSloti]) + 1].Item == null)
                        {
                            DragItem wep = GetDragItem();
                            wep.InitializeDragItem(e, InventorySlots[Array.IndexOf(InventorySlots, InventorySlots[invSloti]) + 1]);
                            c._equipment.Add(e);
                            if (e.isWeapon)
                            {
                                Weapon x = (Weapon) e;
                                if (x.slot == Equipment.Slot.Scroll)
                                {
                                    c._spellScrolls.Add(x);
                                }
                                else
                                {
                                    c._weapons.Add(x);
                                }
                                if (c._weapons.Count > 1)
                                {
                                    c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
                                }
                                else if (c._weapons.Count == 1)
                                {
                                    c.EqMM.UpdateWeapon(c._weapons[0], null);
                                }
                                else
                                {
                                    c.EqMM.UpdateWeapon(null, null);

                                }

                            }
                            c.UpdateStats();
                            c.EqMM.UpdateSlot(e);

                    
                            si.RemoveSelection();
                            return;
                        }
                    }

                    
                    DragItem equiped = InventorySlots[invSloti].Item;
                    equiped.currentLocation = slot;
                    equiped._rectTransform.anchoredPosition = slot._rt.anchoredPosition;
                    equiped.currentLocation.Item = equiped;
                    slot.LabelCheck();
                    UnEquipItem(InventorySlots[invSloti].Item.e);
                    //c._equipment.Remove(equiped.e);
                    
                    
                    
                    DragItem di = GetDragItem();
                    di.InitializeDragItem(e, InventorySlots[invSloti]);
                    c._equipment.Add(e);
                    if (e.isWeapon)
                    {
                        Weapon eq = (Weapon)equiped.e;
                        Weapon x = (Weapon) e;
                        if (x.slot == Equipment.Slot.Scroll)
                        {
                            c._spellScrolls.Add(x);
                            c._spellScrolls.Remove(eq);
                        }
                        else
                        {
                            c._weapons.Add(x);
                            c._weapons.Remove(eq);

                        }
                        if (c._weapons.Count > 1)
                        {
                            c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
                        }
                        else if (c._weapons.Count == 1)
                        {
                            c.EqMM.UpdateWeapon(c._weapons[0], null);
                        }
                        else
                        {
                            c.EqMM.UpdateWeapon(null, null);

                        }

                    }
                    c.UpdateStats();
                    c.EqMM.UpdateSlot(e);
                    si.RemoveSelection();
                    return;

                }

                
            }
        }

        if (equippedSlot == false)
        {
            // no item in same slot
            c._equipment.Add(e);
            c._inventory.Remove(e);
            if (e.isWeapon)
            {
                Weapon x = (Weapon) e;
                if (x.slot == Equipment.Slot.Scroll)
                {
                    c._spellScrolls.Add(x);
                }
                else
                {
                    c._weapons.Add(x);
                }
                if (c._weapons.Count > 1)
                {
                    c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
                }
                else if (c._weapons.Count == 1)
                {
                    c.EqMM.UpdateWeapon(c._weapons[0], null);
                }
                else
                {
                    c.EqMM.UpdateWeapon(null, null);

                }

            }
            c.EqMM.UpdateSlot(e);

        }
        c.UpdateStats();

    }

    public void UnEquipItem(Equipment e)
    {
        c._equipment.Remove(e);
        if (!c._inventory.Contains(e))
        {
            c._inventory.Add(e);
        }

        if (e.isWeapon)
        {
            Weapon x = (Weapon) e;
            if (x.slot == Equipment.Slot.Scroll)
            {
                c._spellScrolls.Remove(x);
            }
            else
            {
                c._weapons.Remove(x);
            }
            if (c._weapons.Count > 1)
            {
                c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
            }
            else if (c._weapons.Count == 1)
            {
                c.EqMM.UpdateWeapon(c._weapons[0], null);
            }
            else
            {
                c.EqMM.UpdateWeapon(null, null);

            }
        }
        c.EqMM.UpdateSlot(e, true);

        c.UpdateStats();

    }

    public void EquipFromInventory(Equipment e)
    {
        //Debug.Log("i only ran once" + e.name) ;
        c._inventory.Remove(e);
        if (!c._equipment.Contains(e))
        {
            c._equipment.Add(e);
            if (e.isWeapon)
            {
                Weapon x = (Weapon) e;
                if (x.slot == Equipment.Slot.Scroll)
                {
                    c._spellScrolls.Add(x);
                }
                else
                {
                    c._weapons.Add(x);
                }
                if (c._weapons.Count > 1)
                {
                    c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
                }
                else if (c._weapons.Count == 1)
                {
                    c.EqMM.UpdateWeapon(c._weapons[0], null);
                }
                else
                {
                    c.EqMM.UpdateWeapon(null, null);

                }
            }

        }
        c.UpdateStats();
        c.EqMM.UpdateSlot(e);

        // if incombat
        if (CombatController._instance.entitiesInCombat.Count > 1 && !e.isPotion)
        {
            
            //Debug.Log("three times?");
            if (e.slot == Equipment.Slot.Scroll && RelicManager._instance.CheckRelic(RelicType.Relic1))
            {
                return;
            }
            if (e.slot == Equipment.Slot.OneHander && RelicManager._instance.CheckRelic(RelicType.Relic2))
            {
                return;
            }
            c.UpdateEnergyCount(-1);
            
        }

    }
    public void DropItem(Equipment e)
    {
        if (c._equipment.Contains(e))
        {
            c.EqMM.UpdateSlot(e, true);
            c._equipment.Remove(e);

        }
        c._inventory.Remove(e);
        
        if (e.isWeapon)
        {
            Weapon x = (Weapon) e;
            if (x.slot == Equipment.Slot.Scroll)
            {
                c._spellScrolls.Remove(x);
            }
            else
            {
                c._weapons.Remove(x);
            }
            if (c._weapons.Count > 1)
            {
                c.EqMM.UpdateWeapon(c._weapons[0], c._weapons[1]);
            }
            else if (c._weapons.Count == 1)
            {
                c.EqMM.UpdateWeapon(c._weapons[0], null);
            }
            else
            {
                c.EqMM.UpdateWeapon(null, null);

            }
        }
        
        if (e.isPotion)
        {
            foreach (var potionDrag in ActivePotions)
            {
                if (potionDrag.potion == e)
                {
                    PoolPotion(potionDrag);
                    break;
                }
            }
        }

        
        c.UpdateStats();
        // if (CombatController._instance.entitiesInCombat.Count > 1 && !e.isPotion)
        // {
        //     UpdateUi();
        // }
        
        
    }
    public void CreateDragItemInShop(Equipment e, InventorySlot slot)
    {
        DragItem di = GetDragItem();
        di.InitializeDragItem(e, slot);
        di.transform.SetParent(slot._rt.parent);
        di.transform.position = slot.transform.position;
    }

    public void AddItemToInventoryFromSelection(Equipment e, SelectionItem si)
    {
        InventorySlot slot = null;
        // check if we have an empty, if we do save that one
        for (int i = 10; i < InventorySlots.Length; i++)
        {
            if (InventorySlots[i].Item == null)
            {
                slot = InventorySlots[i];
                break;
            }
        }

        if (slot == null)
        {
            InventoryNotifications(ErrorMessageManager.Errors.InventoryFull);
            return;
        }
        else
        {
            DragItem di = Instantiate(_dragItemPrefab, inventoryTransform);
            di.InitializeDragItem(e, slot);
            c._inventory.Add(e);
            
            si.RemoveSelection();

            if (di.slotType == Equipment.Slot.Consumable)
            {
                AddPotionToPotionBar((Consumable)e);
            }

        }
    }

    private bool hasInitialized = false;

    public void InitializeEquipmentAndInventoryItems()
    {
        if (hasInitialized)
        {
            return;
        }
        bool placedWep = false;
        bool placedScroll = false;
        
        foreach (var e in c._equipment)
        {
            InventorySlot currentSlot = null;
            if (!e.isWeapon)
            {
                // figure out which slot
                currentSlot = InventorySlots[(int)e.slot];
            }
            else
            {
                if (e.slot == Equipment.Slot.Scroll)
                {
                    if (!placedScroll)
                    {
                        currentSlot = InventorySlots[8];
                        placedScroll = true;
                    }
                    else
                    {
                        currentSlot = InventorySlots[9];

                    }
                }
                else // if we wep
                {
                    if (!placedWep)
                    {
                        currentSlot = InventorySlots[6];
                        placedWep = true;
                    }
                    else
                    {
                        currentSlot = InventorySlots[7];
                    }
                    
                }
               
            }

            DragItem di = Instantiate(_dragItemPrefab, inventoryTransform);
            di.InitializeDragItem(e, currentSlot);

        }

        hasInitialized = true;
    }

    // private void UpdateUi()
    // {
    //     //CombatController._instance.UpdateUiButtons();
    //     
    //     // if in combat take one energy
    //     if (CombatController._instance.entitiesInCombat.Count > 1)
    //     {
    //         //Debug.Log("three times?");
    //         c.UpdateEnergyCount(-1);
    //     }
    // }

    public void AddPotionToPotionBar(Consumable consume)
    {
        PotionDrag p = GetPotionDrag();
        ActivePotions.Add(p);
        p.InitializePotion(consume);
    }

    private PotionDrag GetPotionDrag()
    {
        PotionDrag p;
        if (PotionPool.Count > 0)
        {
            p = PotionPool[0];
            PotionPool.RemoveAt(0);
            p.gameObject.SetActive(true);
        }
        else
        {
            p = Instantiate(PotionPrefab, _potionHolder.transform);
        }
        return p;
    }

    public void PoolPotion(PotionDrag p)
    {
        PotionPool.Add(p);
        ActivePotions.Remove(p);

        c._inventory.Remove(p.potion);
        foreach (var slot in InventorySlots)
        {
            if(slot.Item == null)
                continue;
            if (slot.Item.e == p.potion)
            {
                DragItem DI = slot.Item;
                //we found the potion
                slot.Item.currentLocation.Item = null;
                slot.LabelCheck();
                PoolItem(DI);
                break;
            }
        }

        p.gameObject.SetActive(false);
    }
    
    
    private DragItem GetDragItem()
    {
        DragItem di;
        if (ItemPool.Count > 0)
        {
            di = ItemPool[0];
            ItemPool.RemoveAt(0);
            di.gameObject.SetActive(true);
        }
        else
        {
            di = Instantiate(_dragItemPrefab, inventoryTransform);
        }
        return di;
    }
    public void PoolItem(DragItem di)
    {
        
        ItemPool.Add(di);
        ActiveItems.Remove(di);
        DropItem(di.e);
        //di.e = null;
        di.gameObject.SetActive(false);
    }
    
    public DragItem UpgradeEquipment(DragItem item)
    {
        //check if we are at the forge(ie paying for it or free) 
        //check for money
        //if no money play sound
        // if money
        // do the thing
        //if not at the store set upgrade to false (so you can only do 1) maybe check int value
        //int Upgrade = Mathf.RoundToInt((e.stats[Stats.ItemLevel] * (e.stats[Stats.Rarity] + 1)) * priceMod);

        Equipment e = item.e;
        
        if (ForgeManager._instance.amountOfClicks == 0)
        {
            Debug.LogError("YOU SHOULD NOT BE ABLE TO BE ENHANCING NOW");
            return item;
        }
        
        float priceMod = ForgeManager._instance.priceMod;
        int upgrade = Mathf.RoundToInt((e.stats[Stats.ItemLevel] * (e.stats[Stats.Rarity] + 1)) * priceMod) * 4;
        //it is not free
        if ( priceMod > 0)
        {
            //check if we have the money
            if (CombatController._instance.Player._gold <= upgrade)
            {
                // not enough money
                item.currentLocation.NotEnoughGoldEvent();
                UIController._instance.PlayUIError();
                return item;
            }
            else
            {
                //take gold
                CombatController._instance.Player.GetGold(-upgrade);
            }
        }
        if (ForgeManager._instance.amountOfClicks != -1)
        {
            ForgeManager._instance.AdjustAmountOfClicks(-1);
        }
        
        item._toolTip.CloseTip();

        //e.PrettyPrintStats();
        e.Upgrade();
        //e.PrettyPrintStats();

        item._toolTip.e = e;
        
        UIController._instance.PlayUpgradeSound();

        item.InitializeDragItem(e, item.currentLocation);

        ForgeManager._instance.ShowIcon();
        ForgeManager._instance.ShowPrice(e);

        return item;
    }
    public DragItem EnhanceEquipment(DragItem item)
    {
        //check if we are at the forge(ie paying for it or free) 
        //check for money
        //if no money play sound
        // if money
        // do the thing
        //if not at the store set upgrade to false (so you can only do 1) maybe check int value

        Equipment e = item.e;

        if (e.stats[Stats.Rarity] >= 3)
        {
            UIController._instance.PlayUIError();
            return item;
        }

        if (ForgeManager._instance.amountOfClicks == 0)
        {
            Debug.LogError("YOU SHOULD NOT BE ABLE TO BE ENHANCING NOW");
            return item;
        }

        float priceMod = ForgeManager._instance.priceMod;
        int enhance = Mathf.RoundToInt((e.stats[Stats.ItemLevel] + 5)* (e.stats[Stats.Rarity] + 1) * priceMod) * 4;
        //it is not free
        if ( priceMod > 0)
        {
            //check if we have the money
            if (CombatController._instance.Player._gold <= enhance)
            {
                // not enough money
                item.currentLocation.NotEnoughGoldEvent();
                UIController._instance.PlayUIError();
                return item;
            }
            else
            {
                //take gold
                CombatController._instance.Player.GetGold(-enhance);
            }
        }
        
        if (ForgeManager._instance.amountOfClicks != -1)
        {
            ForgeManager._instance.AdjustAmountOfClicks(-1);
        }
        
        UIController._instance.PlayEnhanceSound();

        item._toolTip.CloseTip();

        //e.PrettyPrintStats();
        e.Enhance();
        //e.PrettyPrintStats();
        
        item._toolTip.e = e;


        item.InitializeDragItem(e, item.currentLocation);
        ForgeManager._instance.ShowIcon();
        ForgeManager._instance.ShowPrice(e);
        return item;
    }

    public void EnhanceRandom(Equipment.Slot slot = Equipment.Slot.All)
    {
        List<DragItem> possibleUpgrades = new List<DragItem>();

        foreach (var inventorySlot in InventorySlots)
        {
            if(inventorySlot.Slot == Equipment.Slot.All || inventorySlot.Item == null)
                continue;
            Equipment e = inventorySlot.Item.e;
            if (slot == Equipment.Slot.All || e.slot == slot)
            {
                // cant enhance epic items
                if(e.stats[Stats.Rarity] < 3)
                    possibleUpgrades.Add(inventorySlot.Item);
            }
        }

        if (possibleUpgrades.Count > 0)
        {
            DragItem i = possibleUpgrades[Random.Range(0, possibleUpgrades.Count)];
            i.e.Enhance();
            i._toolTip.e = i.e;
            i.InitializeDragItem(i.e, i.currentLocation);
        }
        UIController._instance.PlayEnhanceSound();
    }
    public void UpgradeRandom(Equipment.Slot slot = Equipment.Slot.All)
    {
        List<DragItem> possibleUpgrades = new List<DragItem>();

        foreach (var inventorySlot in InventorySlots)
        {
            if(inventorySlot.Slot == Equipment.Slot.All || inventorySlot.Item == null)
                continue;
            Equipment e = inventorySlot.Item.e;
            if (slot == Equipment.Slot.All || e.slot == slot)
            {
                // cant enhance epic items
                possibleUpgrades.Add(inventorySlot.Item);
            }
        }

        if (possibleUpgrades.Count > 0)
        {
            DragItem i = possibleUpgrades[Random.Range(0, possibleUpgrades.Count)];
            i.e.Upgrade();
            i._toolTip.e = i.e;
            i.InitializeDragItem(i.e, i.currentLocation);
        }

        UIController._instance.PlayUpgradeSound();
    }

    public void BreakWeapon()
    {
        List<DragItem> possibleWeapons = new List<DragItem>();

        foreach (var inventorySlot in InventorySlots)
        {
            if (inventorySlot.Slot == Equipment.Slot.OneHander && inventorySlot.Item != null)
            {
                possibleWeapons.Add(inventorySlot.Item);
            }
        }

        if (possibleWeapons.Count == 0)
        {
            foreach (var inventorySlot in InventorySlots)
            {
                if (inventorySlot.Slot == Equipment.Slot.All && inventorySlot.Item != null &&inventorySlot.Item.e.slot == Equipment.Slot.OneHander)
                {
                    possibleWeapons.Add(inventorySlot.Item);
                }
            }
        }

        if (possibleWeapons.Count > 0)
        {
            DragItem i = possibleWeapons[Random.Range(0, possibleWeapons.Count)];
        
            i.currentLocation.Item = null;
            i.currentLocation.LabelCheck();
            _instance.PoolItem(i);
        }
        
        
        //todo PLAY BREAKING SOUND EFFECT
    }




}
