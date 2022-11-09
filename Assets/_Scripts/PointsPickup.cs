using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsPickup : Pickup
{
    public override void PickupAction()
    {

        StartCoroutine(DoublePoints());

        base.PickupAction();
    }


    public IEnumerator DoublePoints()
    {
        GameManager gm = GameObject.FindObjectOfType<GameManager>();
        gm.doublePoints = true;
        yield return new WaitForSeconds(10);
        gm.doublePoints = false;
    }
}
