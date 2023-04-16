using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public InventorySlot Item1;
    public InventorySlot Item2;
    public InventorySlot Item3;
    public InventorySlot Item4;
    
    public InventorySlot SellButton;
    public InventorySlot UpgradeButton;

    public EquipmentCreator EC;

    public InventorySlot.SellShopType ShopType;

    public void InitializeShop(int i)
    {
        InitializeShop((InventorySlot.SellShopType) i);
    }

    public void InitializeShop(InventorySlot.SellShopType type)
    {
        SellButton.SellType = type;
        int level = CombatController._instance.Player._level;
        Equipment e;
        // create drag items
        
        ClearItem(Item1);
        ClearItem(Item2);
        ClearItem(Item3);
        ClearItem(Item4);
        
        switch (type)
        {
            case InventorySlot.SellShopType.Weapons:
                e = EC.CreateRandomWeaponWithRarity(level, 0);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomWeaponWithRarity(level, 1);
                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomWeaponWithRarity(level, 2);
                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomWeaponWithRarity(level, 3);
                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.Scrolls:
                e = EC.CreateRandomSpellScrollWithRarity(level, 0);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomSpellScrollWithRarity(level, 1);

                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomSpellScrollWithRarity(level, 2);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomSpellScrollWithRarity(level, 3);

                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.Armor:
                e = EC.CreateRandomArmorWithRarity(level, 0);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomArmorWithRarity(level, 1);

                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomArmorWithRarity(level, 2);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                e = EC.CreateRandomArmorWithRarity(level, 3);

                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            case InventorySlot.SellShopType.FullHalfPrice:
                e = EC.CreateRandomWeapon(level, false);
                EquipmentManager._instance.CreateDragItemInShop(e, Item1);
                
                e = EC.CreateRandomArmorWithRarity(level, Random.Range(1,4));

                EquipmentManager._instance.CreateDragItemInShop(e, Item2);
                
                e = EC.CreateRandomSpellScroll(level);

                EquipmentManager._instance.CreateDragItemInShop(e, Item3);
                
                // todo create random potion
                e = EC.CreateRandomArmorWithRarity(level, 2);
                EquipmentManager._instance.CreateDragItemInShop(e, Item4);
                break;
            
        }
        AdjustGoldText(Item1);
        AdjustGoldText(Item2);
        AdjustGoldText(Item3);
        AdjustGoldText(Item4);
        
    }

    void AdjustGoldText(InventorySlot slot)
    {
        slot.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            ((slot.Item.e.stats[Equipment.Stats.Rarity] + 1) * 60).ToString();
    }

    void ClearItem(InventorySlot slot)
    {
        if (slot.Item == null)
        {
            return;
        }
        Destroy(slot.Item.gameObject);
        slot.Item = null;

    }
}
