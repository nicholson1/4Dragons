using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePickup : Pickup
{
    public override void PickupAction()
    {

        StartCoroutine(Fire());

        base.PickupAction();
    }



    public IEnumerator Fire()
    {
        CharacterMovement player = GameObject.FindWithTag("Player").GetComponent<CharacterMovement>();
        player.Fire.SetActive(true);
        player.feared = true;
        yield return new WaitForSeconds(5);
        player.Fire.SetActive(false);
        player.feared = false;

    }
}
