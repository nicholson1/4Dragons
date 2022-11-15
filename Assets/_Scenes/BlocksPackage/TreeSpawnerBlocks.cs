using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeSpawnerBlocks : MonoBehaviour
{

    
    
    public int width;
    private float[,] valueMap;
    public int totalSize = 100;
    
    public int octaves;
    public float frequency = 1;


    private float blockWidth;
    
    public GameObject TreePrefab;
    private List<GameObject> HiddenTrees = new List<GameObject>();
    private List<GameObject> ActiveTrees = new List<GameObject>();

    public GameObject TreeHolder;
    
    private void HideTree(GameObject tree)
    {
        ActiveTrees.Remove(tree);
        HiddenTrees.Add(tree);
        tree.SetActive(false);
  
    }

    private void ActivateTree(GameObject tree)
    {
        HiddenTrees.Remove(tree);
        ActiveTrees.Add(tree);
        tree.SetActive(true);

    }

    //this is the different part
    private void PlaceRandomTree()
    {
        //get random pos inside max rad, but not in inner rad;
       

         Vector3 pos = transform.position + new Vector3(Random.Range(-totalSize / 2, totalSize / 2), 0,
             Random.Range(-totalSize / 2, totalSize / 2));
        pos.y = 0;
        GameObject t;
        if (HiddenTrees.Count > 0)
        {
            t = HiddenTrees[0];
            ActivateTree(t);
            t.transform.position = pos;
        }
        else
        {
            t = Instantiate(TreePrefab, pos, TreePrefab.transform.rotation, TreeHolder.transform);
            ActiveTrees.Add(t);
            //t.transform.SetParent(this.transform);
        }
        
        t.GetComponent<TreeManger>().RandomizeTree();

        
    }
    private void PlaceSpecificTree(int i, int j, float treeScale)
    {
        //get random pos inside max rad, but not in inner rad;
        Vector3 pos = transform.position + new Vector3(i * blockWidth - blockWidth * width / 2 + blockWidth / 2, 0,
            j * blockWidth - blockWidth * width / 2 + blockWidth / 2);


        //Vector3 pos = transform.position + new Vector3(Random.Range(-totalSize / 2, totalSize / 2), 0,
        //     Random.Range(-totalSize / 2, totalSize / 2));
        pos.y = 0;
        GameObject t;
        if (HiddenTrees.Count > 0)
        {
            t = HiddenTrees[0];
            ActivateTree(t);
            t.transform.position = pos;
        }
        else
        {
            t = Instantiate(TreePrefab, pos, TreePrefab.transform.rotation, TreeHolder.transform);
            ActiveTrees.Add(t);
            //t.transform.SetParent(this.transform);
        }
        
        t.GetComponent<TreeManger>().RandomizeTree(treeScale);

        
    }
    
    private void HideAllTrees()
    {
        
        foreach (Transform t in TreeHolder.transform)
        {
            HideTree(t.gameObject);
        }

    }

    public void SpawnTrees()
    {
        //todo add peramiter for Forest heavy med light etc
        HideAllTrees();
        PlaceRandomTrees(Random.Range(50, 300));
        
        
        
        //PlaceTrees(Random.Range(9,200));

        //HideAllTrees();
        //GenerateTreeValues();
        //PlaceTrees();
        
        
    }

    private void PlaceRandomTrees(int numberOfTrees)
    {
        while (numberOfTrees > 0)
        {
            PlaceRandomTree();
            numberOfTrees -= 1;
        }
    }
    
    private void PlaceTrees()
    {
        // while (numberOfTrees > 0)
        // {
        //     PlaceRandomTree();
        //     numberOfTrees -= 1;
        // }
        //Trees = new GameObject[width, width];
        blockWidth = (float) totalSize / width;
        
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                if (valueMap[i, j] > 0)
                {
                    PlaceSpecificTree(i, j, valueMap[i,j]);
                }
            }

        }
        
    }
    
    public void GenerateTreeValues()
    {
        valueMap = new float[width,width];

        
        frequency += Random.Range(-1, 1);
        if (frequency < 1)
        {
            frequency = 1 + Random.Range(.01f, .5f);
        }
        octaves += Random.Range(-3, 3);
        if (octaves < 1)
        {
            octaves = 7 + Random.Range(1, 20);
        }
        
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < width; ++j)
            {

                
                float current_frequency = frequency;
                float amplitude = 1;
                float height = 0;
                
               
                for (int z = 0; z < octaves; ++z) // octaves control how many times the perlin noise heigth generation is performed
                {
                    // frequency controlls how many times perlin noise is repeated onto the terrain.
                    // frequency = 1 means entire terrain uses all values of perlin noise
                    // frequence = 2 means that the same perlin noise is repeated 4 times (2x2) on the terrain

                    // amplitude determines much much of the perlin noise is added to the final heigth of the vertex.
                    // aplitude =1 means that the whole value of perlin noise is added
                    // amplitute = 1/2 means that half of the perlin noise is applied for each vertex during the current octave


                    height = height + Mathf.PerlinNoise( i * current_frequency, j * current_frequency) * amplitude; // during every occave add a bit more height

                    amplitude /= 2; // add less
                    current_frequency *= Mathf.Sin(current_frequency); // and more detailed
                    
                    
                    
                    
                }
                //Perlin noise method end
                valueMap[i, j] = height;
                
                if (valueMap[i, j] < .75f)
                {
                    valueMap[i, j] = 0;
                }

                //updating map height and moving 

            }
        }
        
        
        string p = "";
        for (int i = 0; i < width; i++)
        {
           
            for (int j = 0; j < width; j++)
            {
                p += valueMap[i, j] + "    ";
                //Trees[i, j].transform.localScale = new Vector3(blockWidth, valueMap[i, j] * blockWidth, blockWidth);

            }

            p += "\n";
        }

        Debug.Log(p);
    }

    

    
}
