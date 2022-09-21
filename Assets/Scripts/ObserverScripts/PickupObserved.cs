using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObserved : SubjectBeingObserved, Collectible
{
    private void OnTriggerEnter(Collider other)
    {
        Notify(this, NotificationType.HealthPickup);       
    }

}
