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
        CombatController.CombatNotifications += Notification;
    }

    private void OnDestroy()
    {
        EquipmentManager.InventoryNotifications -= Notification;
        CombatEntity.Notification -= Notification;
        CombatController.CombatNotifications -= Notification;
        Character.Notification -= Notification;




    }

    private void Notification(Errors error)
    {
        ErrorMessage e = Instantiate(ErrorMessagePrefab, this.transform);
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
                e.InitializeError("Death!", Color.black);
                break;

                

        }
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
    }
}
