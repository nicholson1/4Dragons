using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenTreasureStart : UIScreen
{
    [SerializeField] private TreasureChest treasureChest;


    protected override void Start()
    {
        base.Start();

        var button = defaultSelectable as Button;
        //button.onClick.AddListener(Deactivate);
    }
}
