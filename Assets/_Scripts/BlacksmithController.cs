using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlacksmithController : MonoBehaviour
{
    public void ClickForge()
    {
        UIController._instance.ToggleForgeUI(1);
        UIController._instance.ToggleInventoryUI(1);
        UIController._instance.ToggleBlackSmithUI(0);
    }
    public void ClickShop()
    {
        ShopManager._instance.InitializeShop(6);
        UIController._instance.ToggleShopUI();
        UIController._instance.ToggleInventoryUI(1);
        
        UIController._instance.ToggleBlackSmithUI(0);

    }
    public void ClickDuel()
    {
    }
}
