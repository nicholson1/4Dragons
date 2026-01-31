using System;
using UnityEngine;

public abstract class UIInventorySubPanel : MonoBehaviour
{
    public InventorySubPanel subPanelType = InventorySubPanel.Undefined;
}

public enum InventorySubPanel
{
    Loot,
    Shop,
    Blacksmith,
    Selection,
    Undefined
}
