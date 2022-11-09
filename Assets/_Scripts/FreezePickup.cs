using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezePickup : Pickup
{
    public override void PickupAction()
    {

        Freeze();

        base.PickupAction();
    }

    private void Freeze()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Enemy>().Freeze();
        }
    }

}
