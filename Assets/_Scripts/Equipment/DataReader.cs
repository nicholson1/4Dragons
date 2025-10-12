using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

public class DataReader : MonoBehaviour
{
    public static DataReader _instance;

    public TextAsset EquipmentNamings;
    public TextAsset WeaponScaling;
    
    public TextAsset DailyChallenges;


    private List<string[]> EquipmentNamingTable;
    
    //Weapon Scaling table is "Ability", Base damage, Level Scaling, type scaling, .... could include more data...
    private List<List<object>> WeaponScalingTable;
    
    private List<List<object>> DailyChallengesTable;
    

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
        EquipmentNamingTable = ReadCSV(EquipmentNamings);
        WeaponScalingTable = ReadTSVWeaponTable(WeaponScaling);
        DailyChallengesTable = ReadTSVDailyChallenges(DailyChallenges);

    }
    

    public List<string[]> GetEquipmentNamingTable()
    {
        return EquipmentNamingTable;
    }
    
    public List<List<object>>  GetWeaponScalingTable()
    {
        return WeaponScalingTable;
    }
    public List<List<object>>  GetDailyChallengesTable()
    {
        return DailyChallengesTable;
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
    private List<List<object>> ReadTSVWeaponTable(TextAsset textAsset)
    {
        string[] data = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        List<object[]> dataTable = new List<object[]>();
        List<List<object>> WeaponTable = new List<List<object>>();
        
        
        
        foreach (var line in data)
        {
            //Debug.Log(line);
            dataTable.Add(line.Split("\t"));
        }

        //List<object> temp = new List<object>();
        for (int i = 0; i < dataTable.Count; i++)
        {
            List<object> temp = new List<object>();
            //temp.Clear();
            // add the name
            temp.Add(dataTable[i][0]);
            //add the data
            string[] scalingData = dataTable[i][1].ToString().Split(",");
            List<object> scaling = new List<object>();
            foreach (var num in scalingData)
            {
                if (num.Contains("."))
                {
                    scaling.Add(float.Parse(num));
                }
                else
                {
                    scaling.Add(int.Parse(num));
                }
            }
            
            temp.Add(scaling);
            
            //energy cost
            temp.Add(dataTable[i][2]);
            
            //description
            temp.Add(dataTable[i][3]);
            
            //abilitytype
            string[] abilitytype = dataTable[i][4].ToString().Split(",");
            List<int> abilities = new List<int>();
            foreach (string num in abilitytype)
            {
                abilities.Add(int.Parse(num));
            }
            temp.Add(abilities);


            // string debug = "";
            // foreach (var o in temp)
            // {
            //     debug += o.GetType().ToString() + ", ";
            // }
            // Debug.Log(debug);
            
            WeaponTable.Add(temp);
            
            //Debug.Log(temp[0] + " " + temp[1]);

        }
        
        // string debug = "";
        // foreach (var sdata in WeaponTable)
        // {
        //     foreach (var i in sdata)
        //     {
        //         debug += i + ", ";
        //     }
        //
        //     debug += "\n";
        // }
        
        // Debug.Log("******************************");
        // Debug.Log(WeaponTable[1].GetType());
        // foreach (var VARIABLE in WeaponTable[1][1])
        // {
        //     Debug.Log(VARIABLE);
        // }

        return WeaponTable;

    }
    private List<List<object>> ReadTSVDailyChallenges(TextAsset textAsset)
    {
        string[] data = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        List<object[]> dataTable = new List<object[]>();
        List<List<object>> dailyChallenges = new List<List<object>>();
        
        
        
        foreach (var line in data)
        {
            dataTable.Add(line.Split("\t"));
        }

        for (int i = 0; i < dataTable.Count; i++)
        {
            List<object> temp = new List<object>();
            
            // add the ID
            temp.Add(int.Parse(dataTable[i][0].ToString()));
            // add the name
            temp.Add(dataTable[i][1]);
            
            //description
            temp.Add(dataTable[i][2]);
            
            //add the data
            string[] modData = dataTable[i][3].ToString().Split(",");
            List<int> mods = new List<int>();
            foreach (var num in modData)
            {
                mods.Add(int.Parse(num));
                
            }
            temp.Add(mods);
            
            //add the data
            string[] decriptorData = dataTable[i][4].ToString().Split(",");
            List<int> decriptor = new List<int>();
            foreach (var num in decriptorData)
            {
                decriptor.Add(int.Parse(num));
                
            }
            temp.Add(decriptor);

            dailyChallenges.Add(temp);
            
        }

        return dailyChallenges;

    }
}
