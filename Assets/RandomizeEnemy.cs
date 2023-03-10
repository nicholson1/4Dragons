using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizeEnemy : MonoBehaviour
{
    [SerializeField] private MeshRenderer Body;
    [SerializeField] private MeshRenderer face;


    private void Start()
    {
        Body.material.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        face.material.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));

    }
}
