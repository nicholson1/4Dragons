using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickupObserved : Pickup, Collectible
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            Notify(this, NotificationType.HealthPickup);       
    }

    public override void PickupAction()
    {
        base.PickupAction();
    }

}
