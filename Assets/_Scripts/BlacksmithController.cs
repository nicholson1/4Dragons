using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlacksmithController : UIScreen
{
    public GameObject TitlePanel => titlePanel;
    public GameObject SelectionPanel => selectionPanel;

    [SerializeField] GameObject titlePanel;
    [SerializeField] GameObject selectionPanel;

    public void ClickForge()
    {
        //UIController._instance.ToggleForgeUI(1);
        //UIController._instance.ToggleInventoryUI(1);
        UIController._instance.CloseBlacksmith(BlacksmithMode.Forge);
    }
    public void ClickShop()
    {
        //ShopManager._instance.InitializeShop(6);
        //UIController._instance.ToggleShopUI();
        //UIController._instance.ToggleInventoryUI(1);

        UIController._instance.CloseBlacksmith(BlacksmithMode.Shop);
    }

    public void ClickDuel()
    {        
        UIController._instance.CloseBlacksmith(BlacksmithMode.Duel);
    }
    
    public void ClickLeave()
    {
        UIController._instance.CloseBlacksmith(BlacksmithMode.Leave);
        //UIController._instance.ToggleBlackSmithUI(0);
        //UIController._instance.ToggleInventoryUI(0);
        //UIController._instance.ToggleMapUI(1);
        //CombatController._instance.SetMapCanBeClicked(true);

        //should call the same function as ending combat to open map
    }
}

public enum BlacksmithMode
{
    Forge,
    Shop,
    Duel,
    Leave
}
