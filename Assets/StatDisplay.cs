using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI
    ;

public class StatDisplay : MonoBehaviour
{
    [SerializeField]private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    public Equipment.Stats stat;
    public int value;


    private void Start()
    {
        icon = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        // if (stat!= null && value != null)
        // {
        //     UpdateValues(stat, value);
        // }
    }

    public void UpdateValues(Equipment.Stats s, int v)
    {
        (string, Sprite, Color) info = StatDisplayManager._instance.GetValues(s);
        icon.sprite = info.Item2;
        icon.color = info.Item3;
        text.text = info.Item1 + ": " + v;
        text.color = info.Item3;
    }
}
