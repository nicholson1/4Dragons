using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeManger : MonoBehaviour
{
    [SerializeField] private GameObject Tree;
    [SerializeField] private LayerMask Ground;
    private bool movedTree = false;

    public void RandomizeTree()
    {
        //only rotate the 
        Tree.transform.eulerAngles = new Vector3(Random.Range(-10, 10), Random.Range(0, 180), Random.Range(-10, 10));
        float scale = Random.Range(.75f, 4);
        
        // scale whole manager
        transform.localScale = new Vector3(scale, scale, scale);
        movedTree = false;
    }

    

    private void Update()
    {
        while (movedTree == false)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
        
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, 1, Ground))
            {
                //Debug.Log(hit.transform.parent.localScale.y);
                transform.position += new Vector3(0, hit.transform.parent.localScale.y, 0);
            }

            movedTree = true;
        }
        
    }
}
