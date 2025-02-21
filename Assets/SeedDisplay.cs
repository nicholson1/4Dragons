using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeedDisplay : MonoBehaviour
{
    public static SeedDisplay _instance;

    private TextMeshProUGUI display;
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

        display = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        display.text = "";
    }

    public void UpdateSeed(int seed)
    {
        display.text = "[" + seed + "]";
    }
}
