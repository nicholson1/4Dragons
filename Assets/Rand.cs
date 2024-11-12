using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rand : MonoBehaviour
{
    public static Rand _i;

    public TMP_InputField seedSetter;

    private int _seed;

    public System.Random Random;
    private void Awake()
    {
        if (_i != null && _i != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _i = this;
        }
        //SetSeed();
    }

    public void SetSeedForRun()
    {
        int seed = -1;
        if (seedSetter != null && seedSetter.text != String.Empty)
        {
            // turn the string into an int
            bool isInteger = int.TryParse(seedSetter.text, out int result);
            if (isInteger)
                seed = result;
            else
                seed = GetIntFromString(seedSetter.text);
        }
        
        SetSeed(seed);
    }

    private void SetSeed(int seed =-1)
    {
        if (seed != -1)
            _seed = seed;
        else
        {
            _seed = UnityEngine.Random.Range(0, 9999999);
        }
        Debug.Log($"Seed: {_seed}");

        Random = new System.Random(_seed);
    }

    private int GetIntFromString(string s)
    {
        int sum = 1;
        foreach (char c in s)
        {
            int unicodeValue = c;  // Getting Unicode value of the character
            sum += unicodeValue;
        }

        sum *= s.Length;

        return sum ;
    }
    
    
    


}
