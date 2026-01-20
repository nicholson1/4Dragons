using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LootButtonManager : MonoBehaviour
{
    public GameObject[] EquipmentButtons;
    public GameObject[] GoldButtons;
    public GameObject[] RelicButtons;

    [SerializeField] private Sprite[] EquipmentSprites;
    [SerializeField] private Transform layoutgroup;

    public List<Button> CurrentActiveButtons => currentActiveButtons;

    public List<List<Equipment>> EquipmentLists = new List<List<Equipment>>();
    public List<List<Equipment>> RelicLists = new List<List<Equipment>>();

    public List<int> GoldList = new List<int>();

    [SerializeField] public GameObject SkipButton;

    public static LootButtonManager _instance;

    private List<Button> currentActiveButtons = new List<Button>();

    private Selectable leftSelectableAtInventoryUI = null;

    
    private void SetLootPanelButtonsLeftNavigation(Selectable selectable)
    {
        
    }

    private void SetupLootPanelNavigation()
    {
        for (int i = 0; i < currentActiveButtons.Count; i++)
        {
            var button = currentActiveButtons[i];
            var navi = button.navigation;
            navi.mode = Navigation.Mode.Explicit;

            if (i % 2 == 0)
            {
                navi.selectOnRight = i + 1 >= currentActiveButtons.Count ? null : currentActiveButtons[i + 1];
                navi.selectOnLeft = leftSelectableAtInventoryUI;
            }
            else
            {
                navi.selectOnRight = null;
                navi.selectOnLeft = i - 1 < 0 ? null : currentActiveButtons[i - 1];
            }

            navi.selectOnDown = i + 2 >= currentActiveButtons.Count ? null : currentActiveButtons[i + 2];
            navi.selectOnUp = i - 2 < 0 ? null : currentActiveButtons[i - 2];

            button.navigation = navi;
        }
       
    }

    private void PopulateCurrentActiveButtons(List<GameObject> buttonGOs)
    {
        if (buttonGOs.Count < 1)
        {
            Debug.LogError($"Error: No active loot button GameObject available!");
            return;
        }

        foreach (var buttonGO in buttonGOs)
        {
            if (buttonGO.TryGetComponent(out Button button))
                currentActiveButtons.Add(button);
        }

        if (currentActiveButtons.Count < 1)
        {
            Debug.LogError($"Error: No active loot button component available!");
        }

        SetupLootPanelNavigation();
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

    public bool HasItems()
    {
        if (EquipmentLists.Count > 0)
            return true;
        if (GoldList.Count > 0)
            return true;
        if (RelicLists.Count > 0)
            return true;
        return false;
    }

    public void SetLootButtons(List<List<Equipment>> equipments = null, List<int> Golds = null, List<List<Equipment>> relics = null)
    {
        ClearAll();
        EquipmentLists = equipments;
        GoldList = Golds;
        RelicLists = relics;
        List<GameObject> cachedButtonGameObjects = new List<GameObject>();

        if(equipments != null)
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                EquipmentButtons[i].SetActive(true);
                AdjustTextAndIcon( EquipmentButtons[i], equipments[i]);
                cachedButtonGameObjects.Add(EquipmentButtons[i]);
            }
        }
        if(relics != null)
        {
            for (int i = 0; i < relics.Count; i++)
            {
                RelicButtons[i].SetActive(true);
                cachedButtonGameObjects.Add(RelicButtons[i]);
            }
        }
        if(Golds != null)
        {
            for (int i = 0; i < Golds.Count; i++)
            {
                GoldButtons[i].SetActive(true);
                GoldButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = Golds[i] + " Gold";
                cachedButtonGameObjects.Add(GoldButtons[i]);
            }
        }

        PopulateCurrentActiveButtons(cachedButtonGameObjects);
    }

    private void AdjustTextAndIcon(GameObject Button, List<Equipment> equipments)
    {
        bool allWeap = true;
        bool allPotion = true;

        bool allScoll = true;
        foreach (var e in equipments)
        {
            if (e.slot != Equipment.Slot.Scroll)
            {
                allScoll = false;
            }
            if (e.slot != Equipment.Slot.OneHander)
            {
                allWeap = false;
            }
            if (e.slot != Equipment.Slot.Consumable)
            {
                allPotion = false;
            }
        }

        if (allWeap)
        {
            Button.GetComponentInChildren<TextMeshProUGUI>().text = "Weapon";
            Button.transform.GetChild(2).GetComponent<Image>().sprite = EquipmentSprites[2];
            return;
        }

        if (allScoll)
        {
            Button.GetComponentInChildren<TextMeshProUGUI>().text = "Scroll";
            Button.transform.GetChild(2).GetComponent<Image>().sprite = EquipmentSprites[1];
            return;
        }
        if (allPotion)
        {
            Button.GetComponentInChildren<TextMeshProUGUI>().text = "Potion";
            Button.transform.GetChild(2).GetComponent<Image>().sprite = EquipmentSprites[3];
            return;
        }
        
        Button.GetComponentInChildren<TextMeshProUGUI>().text = "Equipment";
        Button.transform.GetChild(2).GetComponent<Image>().sprite = EquipmentSprites[0];
        return;

        
    }

    public void ClearAll()
    {
        currentActiveButtons.Clear();

        foreach (var button in EquipmentButtons)
        {
            button.SetActive(false);
            button.GetComponent<Button>().interactable = true;
        }
        foreach (var button in GoldButtons)
        {
            button.SetActive(false);
            button.GetComponent<Button>().interactable = true;
        }
        foreach (var button in RelicButtons)
        {
            button.SetActive(false);
            button.GetComponent<Button>().interactable = true;
        }
    }

    public void EquipmentSelect(int i)
    {
        SelectionManager._instance.SelectionsFromList(EquipmentLists[i]);
        UIController._instance.ToggleInventoryUI(1);
        EquipmentButtons[i].GetComponent<Button>().interactable = false;

        if (EquipmentLists[i].Count == 1)
        {
            SelectionManager._instance.selectionsLeft = 1;
        }
    }
    public void RelicSelect(int i)
    {
        SelectionManager._instance.SelectionsFromList(RelicLists[i]);
        UIController._instance.ToggleInventoryUI(1);
        RelicButtons[i].GetComponent<Button>().interactable = false;
    }
    public void GoldSelect(int i)
    {
        CombatController._instance.Player.GetGold(GoldList[i]);
        GoldButtons[i].GetComponent<Button>().interactable = false;

    }

   
    // public void AddRelicButton()
    // {
    //     GameObject b = Instantiate(GoldButtonPrefab, layoutgroup);
    // }


    public void EquipmentStartButton()
    {
        SelectionManager._instance.RandomSelectionBegging();
    }
    
    
}
