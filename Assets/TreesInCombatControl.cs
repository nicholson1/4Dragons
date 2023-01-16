using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreesInCombatControl : MonoBehaviour
{
    private List<GameObject> trees = new List<GameObject>();
    private void OnEnable()
    {
        trees = new List<GameObject>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            GameObject parent = other.transform.parent.gameObject;
            if (!trees.Contains(parent))
            {
                trees.Add(parent);
            }
            
            //other.GetComponent<FadeObject>().FadeOut();
            foreach (var tree in trees)
            {
                FadeObject[] fadeObjects = tree.GetComponentsInChildren<FadeObject>();
                foreach (var i in fadeObjects)
                {
                    i.FadeOut();
                }
            }
            
        }
    }

    

    private void OnDisable()
    {
        foreach (var tree in trees)
        {
            FadeObject[] fadeObjects = tree.GetComponentsInChildren<FadeObject>();
            foreach (var i in fadeObjects)
            {
                i.FadeIn();
            }
        }
        
    }
}
