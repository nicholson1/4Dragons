using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Elite : MonoBehaviour
{
    [SerializeField] private Character c;
    public EliteType EliteType;
    
    [SerializeField] private GameObject[] Models;

    public void InitializeElite()
    {
        // select elite type
        EliteType = EliteManager._instance.GetEliteType(c._level);
        
        //select model prefab
        Models[(int)EliteType].SetActive(true);
        
        //set rarity

        //create equipment
        
        // set gold
        c._gold = c._level * 5 + (Random.Range(-c._level, c._level+1));
    }
    
}


