using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObserved : Pickup, Collectible
{
    [SerializeField] private NotificationType Type;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Notify(this, Type); 
        }
              
    }

}
