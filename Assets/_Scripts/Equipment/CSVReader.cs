using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http.Headers;

public class CSVReader : MonoBehaviour
{
    public TextAsset EquipmentNamings;
    private List<string[]> EquipmentNamingTable;
    public void Awake()
    {
        EquipmentNamingTable = ReadCSV(EquipmentNamings);
    }

    public List<string[]> GetEquipmentNamingTable()
    {
        return EquipmentNamingTable;
    }

    private List<string[]> ReadCSV(TextAsset textAsset)
    {
        string[] data = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        List<string[]> NamingTable = new List<string[]>();
        string debug = "";
        
        foreach (var line in data)
        {
            NamingTable.Add(line.Split(","));
        }

        foreach (var line in NamingTable)
        {
            foreach (var cell in line)
            {
                debug += cell + " ";
            }

            debug += "\n";
        }
        
        //Debug.Log(debug);

        return NamingTable;

    }
}
