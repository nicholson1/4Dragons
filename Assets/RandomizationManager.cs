using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizationManager : MonoBehaviour
{
    public static RandomizationManager _instance;

    private int _seed;

    public System.Random myRandom;
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
        SetSeed();
    }

    private void SetSeed(int seed =-1)
    {
        if (seed != -1)
            _seed = seed;
        else
        {
            _seed = Random.Range(0, 9999999);
        }
        Debug.Log($"Seed: {_seed}");

        myRandom = new System.Random(_seed);
        //myRandom.
    }


}
