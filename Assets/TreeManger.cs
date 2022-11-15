using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeManger : MonoBehaviour
{
    [SerializeField] private GameObject Tree;
    [SerializeField] private LayerMask Ground;
    public bool movedTree = false;
    
    

    public void RandomizeTree(float scale = 0)
    {
        //only rotate the 
        Tree.transform.eulerAngles = new Vector3(Random.Range(-10, 10), Random.Range(0, 180), Random.Range(-10, 10));
        if (scale == 0)
        {
            scale = Random.Range(.75f, 4);

        }
        
        // scale whole manager
        transform.localScale = new Vector3(scale, scale, scale);
        movedTree = false;
    }

    

    private void Update()
    {
        if (movedTree == false)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            //Debug.DrawRay(transform.position +Vector3.down, transform.TransformDirection(Vector3.up));
            if (Physics.Raycast(transform.position +Vector3.down, transform.TransformDirection(Vector3.up), out hit, 2, Ground))
            {
                //Debug.Log(hit.collider.name);
                transform.position += new Vector3(0, hit.transform.parent.localScale.y, 0);
                movedTree = true;


            }

            
            

        }
        
    }
}
