using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class BlocksGenerator : MonoBehaviour
{
    public float ActivationDistance;
    public GameObject Player;

    public int totalSize = 100;
    
    public int width;
    private float[,] valueMap;
    
    public int octaves;
    public float frequency = 1;
    public GameObject block;

    private GameObject[,] blocks;

    private float blockWidth;
    [SerializeField] private TreeSpawnerBlocks TreeSpawner;

    private GameObject TreeHolder;

    private bool PlayerInRange = false;

    

    void Start()
    {
        TreeSpawner = GetComponent<TreeSpawnerBlocks>();
        TreeHolder = TreeSpawner.TreeHolder;
        PlaceBlocks();
        GenerateWholeBlock();
    }

    private void ActivateTheObjects(bool active)
    {
        TreeHolder.SetActive(active);
    }
    
    private void Update()
    {
        if (!PlayerInRange)
        {
            if (Vector3.Distance(Player.transform.position, transform.position) < ActivationDistance)
            {
                PlayerInRange = true;
                ActivateTheObjects(PlayerInRange);
            }
        }
        else
        {
            if (Vector3.Distance(Player.transform.position, transform.position) > ActivationDistance)
            {
                PlayerInRange = false;
                ActivateTheObjects(PlayerInRange);

            }
        }
        
        

        
        
       
        
    }

    public void GenerateWholeBlock()
    {
        //todo  determien type of block here
        
        
        
        GenerateBlockValues();
        TreeSpawner.SpawnTrees();
        //Debug.Log("spawn tree");

    }

    private void PlaceBlocks()
    {
        blocks = new GameObject[width, width];
        blockWidth = (float) totalSize / width;

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                GameObject b =Instantiate(block, this.transform);
                b.transform.localScale = new Vector3(blockWidth, blockWidth, blockWidth);
                b.transform.localPosition = new Vector3(i * blockWidth - blockWidth * width / 2 + blockWidth / 2, 0,
                    j * blockWidth - blockWidth * width / 2 + blockWidth / 2);
                blocks[i, j] = b;
            }

        }
    }

    private void GenerateBlockValues()
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

                    // see http://www.yaldex.com/open-gl/ch15lev1sec1.html for more details

                    height = height + Mathf.PerlinNoise( i * current_frequency, j * current_frequency) * amplitude; // during every occave add a bit more height

                    amplitude /= 2; // add less
                    current_frequency *= Mathf.Sin(current_frequency); // and more detailed
                    
                    
                }
                //Perlin noise method end
                valueMap[i, j] =(float) Math.Round (height * 8f, MidpointRounding.ToEven) / 8f;

                //updating map height and moving 

            }
        }
        
        AdjustValueMap();
        
        string p = "";
        for (int i = 0; i < width; i++)
        {
           
            for (int j = 0; j < width; j++)
            {
                p += valueMap[i, j] + "    ";
                blocks[i, j].transform.localScale = new Vector3(blockWidth, valueMap[i, j] * blockWidth, blockWidth);

            }

            p += "\n";
        }

        //Debug.Log(p);
    }

    private void AdjustValueMap()
    {
        for (int i = 0; i < width; i++)
        {

            for (int j = 0; j < width; j++)
            {
                if (!CheckAdjacent(i,j))
                {
                    valueMap[i, j] += .125f;
                    //Debug.Log("I adjusted a little");
                }
            }
        }
    }

    private bool CheckAdjacent(int i, int j)
    {
        

        if (i == 0)
        {
            if (j == 0)
            {
                // check right and down
                return Check(false, true, true, false, i, j);

            }else if (j == width -1)
            {
                // check left and down
                return Check(true, false, true, false, i, j);


            }
            else
            {
                //check left, right and down
                return Check(true, true, true, false, i, j);

            }
        }else if (i == width -1)
        {
            if (j == 0)
            {
                // check right and up
                return Check( false, true, false, true, i, j);


            }else if (j == width -1)
            {
                // check left and up
                return Check(true, false, false, true, i, j);

            }
            else
            {
                //check left, right and up
                return Check( true, true, false, true, i, j);

            }
        }
        else
        {
            if (j == 0)
            {
                // check right and up and down
                return Check( false, true, true, true, i, j);

            }else if (j == width -1)
            {
                // check left and up and down
                return Check( true, false, true, true, i, j);

            }
            else
            {
                //check left, right and up and down
                return Check( true, true, true, true, i, j);
            }
        }

        

    }

    public float AllowedDifferece = .25f;

    private bool Check(bool left, bool right, bool down, bool up, int i, int j)
    {
        //Debug.Log(i + "  " + j);
        

        if (down)
        {
            if (Mathf.Abs(valueMap[i, j] - valueMap[i + 1, j]) <= AllowedDifferece)
            {
                return true;
            }
        }
        if (up)
        {
            if (Mathf.Abs(valueMap[i, j] - valueMap[i - 1, j]) <= AllowedDifferece)
            {
                return true;
            }
        }
        if (right)
        {

            if (Mathf.Abs(valueMap[i, j] - valueMap[i, j + 1]) <= AllowedDifferece)
            {
                return true;
            }
        }
        if (left)
        {
            if (Mathf.Abs(valueMap[i, j] - valueMap[i, j-1]) <= AllowedDifferece)
            {
                return true;
            }
        }

        return false;
    }
}
