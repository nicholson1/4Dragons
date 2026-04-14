using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIInventorySubPanel : MonoBehaviour
{
    public InventorySubPanel subPanelType = InventorySubPanel.Undefined;

    public abstract Selectable GetFirstInteractableSelectable();

    public abstract void SetupLeftNavigationToMainPanel(List<Selectable> selectables);
}

public enum InventorySubPanel
{
    Loot,
    Shop,
    Blacksmith,
    Selection,
    Undefined
}
