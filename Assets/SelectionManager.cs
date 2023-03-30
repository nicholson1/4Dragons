using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private SelectionItem selectionItemPrefab;

    
    public int selectionsLeft = 2;
    public static SelectionManager _instance;
    public Button SkipButton;

    private bool startingSelections = true;

    private int startingSelectionCount = 4;
    [SerializeField] private EquipmentCreator EC;
    [SerializeField] private GameObject BeginAdventureButton;
    [SerializeField] private GameObject inventoryButton;
    [SerializeField] private GameObject selectionScreen;
    [SerializeField] private TextMeshProUGUI selectionText;




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
    
    public void RandomSelectionFromEquipment(Character c)
    {
        SkipButton.gameObject.SetActive(true);
        // get 4 random ints 0-c.equip.count
        List<int> index = new List<int>();
        while (index.Count < 4)
        {
            int temp = Random.Range(0, c._equipment.Count);
            if (!index.Contains(temp))
            {
                index.Add(temp);
            }
        }
        foreach (var i in index)
        {
            SelectionItem item = Instantiate(selectionItemPrefab, this.transform);
            item.InitializeSelectionItem(c._equipment[i]);
        }
        
    }

    public void SelectionMade(SelectionItem si)
    {
        //todo pool these
        
        si.DisableButtons();
        selectionsLeft -= 1;
        if (selectionsLeft <= 0)
        {
            SelectionItem[] selectionItems = GetComponentsInChildren<SelectionItem>();
            foreach (var i in selectionItems)
            {
                i.DisableButtons();
            }
        }
    }

    public void ClearSelections()
    {
        
        
        SelectionItem[] selectionItems = GetComponentsInChildren<SelectionItem>();
        foreach (var si in selectionItems)
        {
            if (si.isFlipping)
            {
                return;
            }
        }
        //selectionsLeft = 10;
        for (int i = selectionItems.Length -1; i >= 0; i--)
        {
            Destroy(selectionItems[i].gameObject);
        }
        

        if (selectionsLeft == 2)
        {
            UIController._instance.ToggleInventoryUI();

        }
        selectionsLeft = 2;
        SkipButton.gameObject.SetActive(false);


        if (startingSelections)
        {
            RandomSelectionBegging();
        }
        else
        {
            selectionScreen.SetActive(false);
            CombatController._instance.NextCombatButton.gameObject.SetActive(true);
        }
    }
    
    public void RandomSelectionBegging()
    {
        if (!startingSelections)
        {
            return;
        }
        List<Equipment> equipments = new List<Equipment>();

        if (startingSelectionCount == 4)
        {
            // present 4 weapons, 1 must be a blocking shield
            UIController._instance.ToggleInventoryUI(1);
            selectionScreen.gameObject.SetActive(true);
            selectionText.text = "Selection (1/4)";
            inventoryButton.gameObject.SetActive(true);
            BeginAdventureButton.SetActive(false);
            equipments.Add(EC.CreateRandomWeaponWithSpell(1, Weapon.SpellTypes.Shield2));
            equipments.Add(EC.CreateRandomWeapon(1, false));
            equipments.Add(EC.CreateRandomWeapon(1, false));
            equipments.Add(EC.CreateRandomWeapon(1, false));

        }

        else if (startingSelectionCount == 3)
        {
            // present 4 spells
            selectionText.text = "Selection (2/4)";
            equipments.Add(EC.CreateRandomSpellScroll(1));
            equipments.Add(EC.CreateRandomSpellScroll(1));
            equipments.Add(EC.CreateRandomSpellScroll(1));
            equipments.Add(EC.CreateRandomSpellScroll(1));

            
        }
        else if (startingSelectionCount == 2)
        {
            //head, shoulder, chest, random
            selectionText.text = "Selection (3/4)";
            equipments.Add(EC.CreateArmor(1, Equipment.Slot.Head));
            equipments.Add(EC.CreateArmor(1, Equipment.Slot.Shoulders));
            equipments.Add(EC.CreateArmor(1, Equipment.Slot.Chest));
            equipments.Add(EC.CreateArmor(1, (Equipment.Slot)Random.Range(0,6)));


        }
        else if (startingSelectionCount == 1)
        {
            //gloves, legs, boots, random
            selectionText.text = "Selection (4/4)";
            equipments.Add(EC.CreateArmor(1, Equipment.Slot.Gloves));
            equipments.Add(EC.CreateArmor(1, Equipment.Slot.Legs));
            equipments.Add(EC.CreateArmor(1, Equipment.Slot.Boots));
            equipments.Add(EC.CreateArmor(1, (Equipment.Slot)Random.Range(0,6)));
        }
        
        //SkipButton.gameObject.SetActive(true);
        // get 4 random ints 0-c.equip.count
        
        
        
        foreach (var e in equipments)
        {
            SelectionItem item = Instantiate(selectionItemPrefab, this.transform);
            item.InitializeSelectionItem(e);
        }

        startingSelectionCount -= 1;

        if (startingSelectionCount == 0)
        {
            startingSelections = false;
        }

    }
}
