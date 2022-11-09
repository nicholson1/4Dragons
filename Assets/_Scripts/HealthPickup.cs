using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
    public override void PickupAction()
    {

        Health();

        base.PickupAction();
    }


    private void Health()
    {
        GameObject.FindObjectOfType<GameManager>().lives += 1;
    }
}
