using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickup : Pickup
{
    public override void PickupAction()
    {

        StartCoroutine(SpeedBoost());

        base.PickupAction();
    }


    public IEnumerator SpeedBoost()
    {
        CharacterMovement player = GameObject.FindWithTag("Player").GetComponent<CharacterMovement>();
        player.speed += 5;
        yield return new WaitForSeconds(5);
        player.speed -= 5;
    }
}
