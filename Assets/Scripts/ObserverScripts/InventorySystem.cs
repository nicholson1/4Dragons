using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour, Observer
{
 
    public void OnNotify(Collectible obj, NotificationType notificationType)
    {
        if (notificationType == NotificationType.HealthPickup)
        {
            Debug.Log("Congrats! + 1 health");
        }
    }

    void Start()
    {
        foreach (SubjectBeingObserved subject in FindObjectsOfType<SubjectBeingObserved>())
        {
            subject.AddObserver(this);
        }
    }

}

