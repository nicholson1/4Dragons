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


    [SerializeField] public Image Background;


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
        //SkipButton.gameObject.SetActive(true);
        // get 4 random ints 0-c.equip.count
        List<List<Equipment>> EquipmentSelection = new List<List<Equipment>>();
        List<Equipment> equipments = new List<Equipment>();
        // force a spell or weapon that has not been selected
        Equipment forcedWep = c._equipment[Random.Range(c._equipment.Count - 4, c._equipment.Count)];

        equipments.Add(forcedWep);
        
        // fill the rest with 3 random
        while (equipments.Count < 4)
        {
            Equipment temp = c._equipment[Random.Range(0, c._equipment.Count)];

            if (temp.canBeLoot == false)
            {
                continue;
            }
            
            if (!equipments.Contains(temp))
            {
                equipments.Add(temp);
            }
        }
        EquipmentSelection.Add(equipments);

        if (RelicManager._instance.CheckRelic(RelicType.DragonRelic6))
        {

            List<Equipment> possible = new List<Equipment>();
            foreach (var e in c._equipment)
            {
                if (!equipments.Contains(e) && e.canBeLoot)
                {
                    possible.Add(e);
                }
            }
            if(possible.Count > 4)
            {
                while (possible.Count > 4)
                {
                    possible.RemoveAt(Random.Range(0, possible.Count));
                }
            }
            
            EquipmentSelection.Add(possible);
            
        }

        List<List<Equipment>> RelicSelections = new List<List<Equipment>>();
        if (c.isElite)
        {
            List<Equipment> relics = new List<Equipment>();
            relics.Add(RelicManager._instance.GetCommonRelic());
            relics.Add(RelicManager._instance.GetCommonRelic());
            relics.Add(RelicManager._instance.GetCommonRelic());
            RelicSelections.Add(relics);

            if (RelicManager._instance.CheckRelic(RelicType.DragonRelic5))
            {
                relics = new List<Equipment>();
                relics.Add(RelicManager._instance.GetCommonRelic());
                relics.Add(RelicManager._instance.GetCommonRelic());
                relics.Add(RelicManager._instance.GetCommonRelic());
                RelicSelections.Add(relics);
            }
        }
        if (c.isDragon)
        {
            List<Equipment> relics = new List<Equipment>();
            relics.Add(RelicManager._instance.GetDragonRelic());
            relics.Add(RelicManager._instance.GetDragonRelic());
            relics.Add(RelicManager._instance.GetDragonRelic());
            RelicSelections.Add(relics);
        }

        if (RelicSelections.Count == 0)
        {
            RelicSelections = null;
        }
        if (EquipmentSelection.Count == 0)
        {
            EquipmentSelection = null;
        }

        List<int> GoldSelections = new List<int>();

        if (RelicManager._instance.CheckRelic(RelicType.Relic34))
        {
            c._gold += Mathf.RoundToInt(c._gold * .25f);
        }
        
        GoldSelections.Add(c._gold);
        
        if (RelicManager._instance.CheckRelic(RelicType.Relic26))
        {
            GoldSelections.Add(Mathf.RoundToInt(CombatController._instance.Player._gold * .05f));
        }

        LootButtonManager._instance.SetLootButtons(EquipmentSelection, GoldSelections, RelicSelections);
        UIController._instance.ToggleLootUI(1);
        UIController._instance.ToggleInventoryUI(1);
        
        // foreach (var i in equipments)
        // {
        //     SelectionItem item = Instantiate(selectionItemPrefab, this.transform);
        //     item.InitializeSelectionItem(c._equipment[i]);
        // }
        //
        // StartCoroutine(FadeImage(.75f));
    }

    public void SelectionsFromList(List<Equipment> equipments)
    {
        SkipButton.gameObject.SetActive(true);
        foreach (var i in equipments)
        {
            SelectionItem item = Instantiate(selectionItemPrefab, this.transform);
            item.InitializeSelectionItem(i);
        }
        StartCoroutine(FadeImage(1,.75f));

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

        if (RelicManager._instance.CheckRelic(RelicType.DragonRelic8))
        {
            selectionsLeft = 1;
        }
        else
        {
            selectionsLeft = 2;
        }
        SkipButton.gameObject.SetActive(false);


        //UIController._instance.ToggleLootUI();
        selectionScreen.SetActive(false);
        //CombatController._instance.NextCombatButton.gameObject.SetActive(true);
        StartCoroutine(FadeImage(.5f,0f));
    }

    public void CreateChestReward(bool forceRelic = false)
    {
        List<List<Equipment>> relics = new List<List<Equipment>>();
        List<List<Equipment>> equipments = new List<List<Equipment>>();
        List<int> golds = new List<int>();

        
        (ChestType, ChestType) selectionType = SelectChestType();

        if (forceRelic)
        {
            selectionType.Item1 = ChestType.Relic;
            selectionType.Item2 = ChestType.None;
        }

        int level = CombatController._instance.Player._level;

        List<Equipment> selection = new List<Equipment>();

        switch (selectionType.Item1)
        {
            case ChestType.Relic:
                relics.Add(new List<Equipment> {RelicManager._instance.GetCommonRelic()});
                break;
            case ChestType.Gold:
                golds.Add(Random.Range(-10, 10) + 100 * CombatController._instance.TrialCounter);
                break;
            case ChestType.Equipment:
                selection = new List<Equipment>();
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                equipments.Add(selection);
                break;
            case ChestType.Weapon:
                selection = new List<Equipment>();
                selection.Add(EC.CreateRandomWeapon(level, false));
                selection.Add(EC.CreateRandomWeapon(level, false));
                selection.Add(EC.CreateRandomWeapon(level, false));
                selection.Add(EC.CreateRandomWeapon(level, false));
                equipments.Add(selection);
                break;
            case ChestType.Scroll:
                selection = new List<Equipment>();
                selection.Add(EC.CreateRandomSpellScroll(level));
                selection.Add(EC.CreateRandomSpellScroll(level));
                selection.Add(EC.CreateRandomSpellScroll(level));
                selection.Add(EC.CreateRandomSpellScroll(level));
                equipments.Add(selection);
                break;
            case ChestType.Potion:
                selection = new List<Equipment>();
                selection.Add(EC.CreateRandomPotion(level));
                selection.Add(EC.CreateRandomPotion(level));
                selection.Add(EC.CreateRandomPotion(level));
                selection.Add(EC.CreateRandomPotion(level));
                equipments.Add(selection);
                break;
            
        }
        switch (selectionType.Item2)
        {
            case ChestType.Gold:
                golds.Add(Random.Range(-10, 10) + 100 * CombatController._instance.TrialCounter);
                break;
            case ChestType.Equipment:
                selection = new List<Equipment>();
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                selection.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
                equipments.Add(selection);
                break;
            case ChestType.Weapon:
                selection = new List<Equipment>();
                selection.Add(EC.CreateRandomWeapon(level, false));
                selection.Add(EC.CreateRandomWeapon(level, false));
                selection.Add(EC.CreateRandomWeapon(level, false));
                selection.Add(EC.CreateRandomWeapon(level, false));
                equipments.Add(selection);
                break;
            case ChestType.Scroll:
                selection = new List<Equipment>();
                selection.Add(EC.CreateRandomSpellScroll(level));
                selection.Add(EC.CreateRandomSpellScroll(level));
                selection.Add(EC.CreateRandomSpellScroll(level));
                selection.Add(EC.CreateRandomSpellScroll(level));
                equipments.Add(selection);
                break;
            case ChestType.Potion:
                selection = new List<Equipment>();
                selection.Add(EC.CreateRandomPotion(level));
                selection.Add(EC.CreateRandomPotion(level));
                selection.Add(EC.CreateRandomPotion(level));
                selection.Add(EC.CreateRandomPotion(level));
                equipments.Add(selection);
                break;
        }
        golds.Add(Random.Range(-5, 10) + 50 * CombatController._instance.TrialCounter);


        if (relics.Count == 0)
            relics = null;

        LootButtonManager._instance.SetLootButtons(equipments, golds, relics);
    }

    public void CreateEquipmentListsStart()
    {
        List<List<Equipment>> equipments = new List<List<Equipment>>();

        List<Equipment> selection1 = new List<Equipment>();
        List<Equipment> selection2 = new List<Equipment>();

        List<Equipment> selection3 = new List<Equipment>();
        List<Equipment> selection4 = new List<Equipment>();

        int level = CombatController._instance.Player._level;
        selection1.Add(EC.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield2));
        // present 4 spells
        int spellCount = 1;
        while (spellCount < 4)
        {
            Equipment eq = EC.CreateRandomWeapon(level, false);

            if (spellCount == 3)
            {
                if (!HasDamageSpell(selection1))
                {
                    eq = EC.CreateWeapon(level, 0, Equipment.Slot.OneHander,
                        (Weapon.SpellTypes)GetRandomDamagePhysicalSpellInt());
                }
            }

            Weapon w = (Weapon)eq;

            bool hasSpell = false;
            foreach (var equipment in selection1)
            {
                Weapon wep = (Weapon)equipment;
                if (wep.spellType1 == w.spellType1)
                {
                    hasSpell = true;
                }
            }

            if (hasSpell == false)
            {
                selection1.Add(eq);
                spellCount += 1;
            }
        }

        equipments.Add(selection1);
            ///////////////////////////////////////////////////////////////////////////////////

            spellCount = 0;
            selectionText.text = "Selection (2/4)";
            while (spellCount < 4)
            {
                Equipment eq = EC.CreateRandomSpellScroll(level);

                if (spellCount == 3)
                {
                    if (!HasDamageSpell(selection2))
                    {
                        eq = EC.CreateSpellScroll(level, 0, (Weapon.SpellTypes)GetRandomDamageSpellInt());
                    }
                }

                Weapon w = (Weapon)eq;

                bool hasSpell = false;
                foreach (var equipment in selection2)
                {
                    Weapon wep = (Weapon)equipment;
                    if (wep.spellType1 == w.spellType1)
                    {
                        hasSpell = true;
                    }
                }

                if (hasSpell == false)
                {
                    selection2.Add(eq);
                    spellCount += 1;
                }
            }

            equipments.Add(selection2);
            ///////////////////////////////////////////////////////////////////////////////////
            selection3.Add(EC.CreateArmor(level, Equipment.Slot.Head));
            selection3.Add(EC.CreateArmor(level, Equipment.Slot.Shoulders));
            selection3.Add(EC.CreateArmor(level, Equipment.Slot.Chest));
            selection3.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
            equipments.Add(selection3);
            
            ///////////////////////////////////////////////////////////////////////////////////
            selection4.Add(EC.CreateArmor(level, Equipment.Slot.Gloves));
            selection4.Add(EC.CreateArmor(level, Equipment.Slot.Legs));
            selection4.Add(EC.CreateArmor(level, Equipment.Slot.Boots));
            selection4.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0, 6)));
            equipments.Add(selection4);
            
            List<Equipment> selection5 = new List<Equipment>();
            selection5.Add(EC.CreateRandomPotion(level));
            selection5.Add(EC.CreateRandomPotion(level));
            selection5.Add(EC.CreateRandomPotion(level));
            selection5.Add(EC.CreateRandomPotion(level));
            equipments.Add(selection5);
            

            // foreach (var VARIABLE in equipments)
            // {
            //     foreach (var v in VARIABLE)
            //     {
            //         Debug.Log(v.name);
            //     }
            // }
            List<Equipment> relics = new List<Equipment>();
            relics.Add(RelicManager._instance.GetCommonRelic());
            relics.Add(RelicManager._instance.GetCommonRelic());
            relics.Add(RelicManager._instance.GetCommonRelic());
            
            LootButtonManager._instance.SetLootButtons(equipments, new List<int>(){25}, new List<List<Equipment>>(){relics});
    }

    
    public void RandomSelectionBegging()
    {
        //get level from character
        int level = CombatController._instance.Player._level;
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
            equipments.Add(EC.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield2));
            // present 4 spells
            int spellCount = 1;
            while (spellCount < 4)
            {
                Equipment eq = EC.CreateRandomWeapon(level, false);

                if (spellCount == 3)
                {
                    if (!HasDamageSpell(equipments))
                    {
                        eq = EC.CreateWeapon(level, 0, Equipment.Slot.OneHander,(Weapon.SpellTypes)GetRandomDamagePhysicalSpellInt());
                    }
                }
                
                Weapon w = (Weapon)eq;

                bool hasSpell = false;
                foreach (var equipment in equipments)
                {
                    Weapon wep = (Weapon)equipment;
                    if (wep.spellType1 == w.spellType1)
                    {
                        hasSpell = true;
                    }
                }

                if (hasSpell == false)
                {
                    equipments.Add(eq);
                    spellCount += 1;
                    //Debug.Log(spellCount);
                }

            }

        }

        else if (startingSelectionCount == 3)
        {
            // present 4 spells
            int spellCount = 0;
            selectionText.text = "Selection (2/4)";
            while (spellCount < 4)
            {
                Equipment eq = EC.CreateRandomSpellScroll(level);

                if (spellCount == 3)
                {
                    if (!HasDamageSpell(equipments))
                    {
                        eq = EC.CreateSpellScroll(level, 0, (Weapon.SpellTypes)GetRandomDamageSpellInt());
                    }
                }
                Weapon w = (Weapon)eq;

                bool hasSpell = false;
                foreach (var equipment in equipments)
                {
                    Weapon wep = (Weapon)equipment;
                    if (wep.spellType1 == w.spellType1)
                    {
                        hasSpell = true;
                    }
                }

                if (hasSpell == false)
                {
                    equipments.Add(eq);
                    spellCount += 1;
                }

            }
            //equipments.Add(EC.CreateRandomSpellScroll(level));
            //equipments.Add(EC.CreateRandomSpellScroll(level));

            
        }
        else if (startingSelectionCount == 2)
        {
            //head, shoulder, chest, random
            selectionText.text = "Selection (3/4)";
            equipments.Add(EC.CreateArmor(level, Equipment.Slot.Head));
            equipments.Add(EC.CreateArmor(level, Equipment.Slot.Shoulders));
            equipments.Add(EC.CreateArmor(level, Equipment.Slot.Chest));
            equipments.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0,6)));


        }
        else if (startingSelectionCount == 1)
        {
            //gloves, legs, boots, random
            selectionText.text = "Selection (4/4)";
            equipments.Add(EC.CreateArmor(level, Equipment.Slot.Gloves));
            equipments.Add(EC.CreateArmor(level, Equipment.Slot.Legs));
            equipments.Add(EC.CreateArmor(level, Equipment.Slot.Boots));
            equipments.Add(EC.CreateArmor(level, (Equipment.Slot)Random.Range(0,6)));
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
    
    IEnumerator FadeImage(float fadeDuration, float targetAlpha)
    {
            
        Background.gameObject.SetActive(true);
        
        // Set the initial alpha value 
        float startingAlpha = Background.color.a;

        Color startColor = Background.color;
        Color endColor = startColor;
        endColor.a = targetAlpha;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Calculate the current alpha value based on the elapsed time
            float alpha = Mathf.Lerp(startingAlpha, targetAlpha, elapsedTime / fadeDuration);

            // Set the alpha value of the image
            Background.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the alpha value is exactly 1 at the end
        Background.color = endColor;
        if (targetAlpha == 0)
        {
            Background.gameObject.SetActive(false);
        }
    }

    bool HasDamageSpell(List<Equipment> equipments)
    {
        bool hasDamage = false;
        foreach (var eq in equipments)
        {
            Weapon wep = (Weapon)eq;
            // get the spell
            int spellIndex = (int)wep.spellType1;
            List<List<object>> scaling = DataReader._instance.GetWeaponScalingTable();
            IList abilities = (IList)scaling[(int)spellIndex][4];

            if (abilities.Contains(0) || abilities.Contains(1))
            {
                hasDamage = true;
            }
        }

        return hasDamage;
    }

    int GetRandomDamageSpellInt()
    {
        int[] damageSpells = new[] { 17, 18, 21, 22, 23, 27, 28, 30, 31, 37 };
        return damageSpells[Random.Range(0, damageSpells.Length)];
    }
    int GetRandomDamagePhysicalSpellInt()
    {
        int[] damageSpells = new[] { 0,1,2,3,6,7,8,9,10,11,12,13,14 };
        return damageSpells[Random.Range(0, damageSpells.Length)];
    }

    private (ChestType, ChestType) SelectChestType()
    {
        
        int roll1 = Random.Range(0, 6);
        
        //return ((ChestType)roll1, ChestType.Potion);

        if (roll1 == 0)
        {
            return (ChestType.Relic, ChestType.None);
        }
        int roll2 = Random.Range(1, 6);
        return ((ChestType)roll1, (ChestType)roll2);

    }

    enum ChestType
    {
        Relic,
        Gold,
        Scroll,
        Weapon, 
        Equipment,
        Potion,
        None,
    }
}
