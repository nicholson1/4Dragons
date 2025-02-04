using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseLoader : MonoBehaviour
{
    Database _database;

    public void LoadTabs()
    {
        _database = Resources.Load<Database>("Database");

        if (_database == null)
        {
            Debug.LogError("Could not find Database in Resources. \n Aborting Load action.");
            return;
        }

        _database.LoadTabs();
    }
    
}
