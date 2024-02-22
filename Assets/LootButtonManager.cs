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
    [SerializeField] private Transform layoutgroup;

    public List<GameObject> CurrentButtons = new List<GameObject>();

    public List<List<Equipment>> EquipmentLists = new List<List<Equipment>>();
    public List<int> GoldList = new List<int>();


    public void SetLootButtons(List<List<Equipment>> equipments = null, List<int> Golds = null)
    {
        ClearAll();
        EquipmentLists = equipments;
        GoldList = Golds;
        for (int i = 0; i < equipments.Count; i++)
        {
            EquipmentButtons[i].SetActive(true);
        }
        for (int i = 0; i < Golds.Count; i++)
        {
            GoldButtons[i].SetActive(true);
            GoldButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = Golds[i] + " Gold";
        }
    }

    public void ClearAll()
    {
        foreach (var button in EquipmentButtons)
        {
            button.SetActive(false);
        }
        foreach (var button in GoldButtons)
        {
            button.SetActive(false);
        }
        foreach (var button in RelicButtons)
        {
            button.SetActive(false);
        }
    }

    public void EquipmentSelect(int i)
    {
        SelectionManager._instance.SelectionsFromList(EquipmentLists[i]);
        UIController._instance.ToggleInventoryUI(1);
        EquipmentButtons[i].SetActive(false);
    }
    public void GoldSelect(int i)
    {
        CombatController._instance.Player._gold += GoldList[i];
        CombatController._instance.Player.UpdateStats();
        GoldButtons[i].SetActive(false);
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
