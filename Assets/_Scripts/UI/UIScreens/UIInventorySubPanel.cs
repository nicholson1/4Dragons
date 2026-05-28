using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIInventorySubPanel : MonoBehaviour
{
    public event Action<UIInventorySubPanel> OnPanelOpen;
    public event Action<UIInventorySubPanel> OnPanelClosed;

    public InventoryState subPanelType = InventoryState.Base;
    protected List<Selectable> cachedRightmostInventoryButtons = new List<Selectable>();

    public abstract Selectable GetFirstInteractableSelectable();

    public abstract void SetupLeftNavigationToMainPanel(List<Selectable> selectables);

    public abstract void SetSkipButtonInteractable(bool isInteractable);

    //public abstract void SetInventoryButtonsCache(List<Selectable> inventoryButtons);
    //public virtual void SetInventoryButtonsCache(List<Selectable> inventoryButtons)
    //{
    //    cachedInventoryButtons = inventoryButtons;
    //}

    protected void BroadcastPanelOpen()
    {
        OnPanelOpen?.Invoke(this);
    }

    protected void BroadcastPanelClose()
    {
        OnPanelClosed?.Invoke(this);
    }
}

public enum InventorySubPanel
{
    Loot,
    Shop,
    Blacksmith,
    Selection,
    Undefined
}
