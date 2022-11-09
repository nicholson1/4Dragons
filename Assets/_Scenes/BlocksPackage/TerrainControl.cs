using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainControl : MonoBehaviour
{
    public BlocksGenerator[] terrains;

    public GameObject Player;
    public BlocksGenerator CurrentTerrain;

    public float terrainWidth;
    
    public float timeBetweenChecks = 0;


    

    private void FindClosestTerrain()
    {
        float dist = terrainWidth * 100;
        BlocksGenerator CurrentTemp = null;
        foreach (var t in terrains)
        {
            float d = Vector3.Distance(t.transform.position, Player.transform.position);
            if (d < dist)
            {
                CurrentTemp = t;
                dist = d;
                //Debug.Log(t.name + " : " + d);

            }
        }


        if (CurrentTemp != CurrentTerrain)
        {
            CurrentTerrain = CurrentTemp;
            foreach (var t in terrains)
            {
                float d = Vector3.Distance(Player.transform.position, t.transform.position);
                //Debug.Log(t.name + " : " + d);

            }
            PlayerChangedTerrain();

        }

    }

    private void Update()
    {

        //Debug.Log(" CURRENT : " +  Vector3.Distance(Player.transform.position, CurrentTerrain.transform.position));
        FindClosestTerrain();


    }

    private List<BlocksGenerator> GetThreeFurthest()
    {
        List<BlocksGenerator> furthest = new List<BlocksGenerator>();

        while (furthest.Count < 3)
        {
            BlocksGenerator add = null;
            float dist = 0;
            foreach (var t in terrains)
            {
                if (!furthest.Contains(t))
                {
                    float d = Vector3.Distance(Player.transform.position, t.transform.position);
                    if (d > dist)
                    {
                        dist = d;
                        add = t;
                    }
                }
            }
            furthest.Add(add);
        }

        return furthest;

    }

    private (float,bool) GetTranslateDirection()
    {
        //horizontal = true;
        // vertical = false;
        
        Vector3 translate = Player.transform.position - CurrentTerrain.transform.position;

        
        if (Mathf.Abs(translate.x) > Mathf.Abs(translate.z))
        {
            // left right
            if (translate.x > 0)
            {
                // shift negative 3 * terrain width
                return (-3 * terrainWidth, true);
            }
            else
            {
                return (3 * terrainWidth, true);

            }
        }
        else
        {
            //up down
            if (translate.z > 0)
            {
                return (-3 * terrainWidth, false) ;

            }
            else
            {
                return (3 * terrainWidth, false);

            }
        }
        
        
        
        
    }

    void PlayerChangedTerrain()
    {
        //Debug.Log("Hi we ran");
        List<BlocksGenerator> furthest = GetThreeFurthest();
        (float, bool) info = GetTranslateDirection();
        foreach (var t in furthest)
        {
            if (info.Item2 == true)
            {
                t.transform.position += new Vector3(info.Item1, 0, 0);
            }
            else
            {
                t.transform.position += new Vector3(0, 0, info.Item1);

            }
            t.GenerateBlockValues();
        }
        
        //get furthest
        //find translate direction
        // do it to all
        
        
    }
}
