using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class ErrorMessageManager : MonoBehaviour
{
    [SerializeField] private ErrorMessage ErrorMessagePrefab;

    private void Start()
    {
        EquipmentManager.InventoryNotifications += Notification;
        CombatEntity.Notification += Notification;
        Character.Notification += Notification;
        Character.NotificationGold += Notification;

        CombatController.CombatNotifications += Notification;

        DragItem.CombatMove += Notification;
        InventorySlot.CombatMove += Notification;
        InventorySlot.SellItem += Notification;
        InventorySlot.NotEnoughGold += Notification;

    }

    private void OnDestroy()
    {
        EquipmentManager.InventoryNotifications -= Notification;
        CombatEntity.Notification -= Notification;
        CombatController.CombatNotifications -= Notification;
        Character.Notification -= Notification;
        DragItem.CombatMove -= Notification;
        InventorySlot.CombatMove -= Notification;
        InventorySlot.SellItem -= Notification;

        Character.NotificationGold -= Notification;
        InventorySlot.NotEnoughGold -= Notification;







    }

    private void Notification(Errors error, int i)
    {
        if (i == 0)
        {
            return;
        }
        ErrorMessage e = GetMessage();
        switch (error)
        {
            case Errors.GetGold:
                e.InitializeError("+" + i + " Gold", Color.yellow);
                break;
            case Errors.LoseGold:                
                e.InitializeError( i + " Gold", Color.yellow);
                break;

        }

    }


    private void Notification(Errors error)
    {
        ErrorMessage e = GetMessage();
        switch (error)
        {
            case Errors.InventoryFull:
                e.InitializeError("Inventory is full", Color.white);
                break;
            case Errors.CriticalHit:
                e.InitializeError("Critical Hit!", Color.red);
                break;
            case Errors.CriticalHeal:
                e.InitializeError("Critical Heal!", Color.green);
                break;
            case Errors.YourTurn:
                e.InitializeError("Your Turn!", Color.white);
                break;
            case Errors.NewFoe:
                e.InitializeError("A New Foe Has Appeared!", Color.yellow);
                break;
            case Errors.Victory:
                e.InitializeError("Level Up!", Color.white);
                e.InitializeError("Victory!", Color.blue);
                break;
            case Errors.YouHaveDied:
                e.InitializeError("", Color.black);
                break;
            case Errors.CombatMove:
                e.InitializeError("You Don't Have Enough Energy", Color.white);
                break;
            case Errors.NotEnoughGold:
                e.InitializeError("You Don't Have Enough Gold", Color.yellow);
                break;
            case Errors.Tie:
                e.InitializeError("Its a Draw!", Color.blue);
                break;

        }
    }

    private ErrorMessage GetMessage()
    {
        ErrorMessage e;
        if (UIPooler._instance.NotificationMessages.Count != 0)
        {
            e = UIPooler._instance.NotificationMessages[0].GetComponent<ErrorMessage>();
            UIPooler._instance.NotificationMessages.RemoveAt(0);
            e.transform.SetParent(this.transform);
            e.gameObject.SetActive(true);

        }
        else
        {
            e = Instantiate(ErrorMessagePrefab, this.transform);
        }

        return e;
    }

    public enum Errors
    {
        InventoryFull,
        NewFoe,
        NotEnoughGold,
        NotEnoughEnergy,
        YouHaveDied,
        Victory,
        YourTurn,
        CriticalHit,
        CriticalHeal,
        CombatMove,
        GetGold,
        LoseGold,
        Tie,
    }
}
