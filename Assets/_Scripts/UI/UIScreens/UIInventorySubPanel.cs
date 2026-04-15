using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIInventorySubPanel : MonoBehaviour
{
    public event Action<UIInventorySubPanel> OnPanelOpen;
    public event Action<UIInventorySubPanel> OnPanelClosed;

    public InventoryState subPanelType = InventoryState.Base;

    public abstract Selectable GetFirstInteractableSelectable();

    public abstract void SetupLeftNavigationToMainPanel(List<Selectable> selectables);

    public abstract void SetLeaveButtonInteractable(bool isInteractable);

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
