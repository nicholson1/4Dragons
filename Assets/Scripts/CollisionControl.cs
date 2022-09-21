using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionControl : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
         
            other.GetComponent<Pickup>().PickupAction();

        }

        if (other.CompareTag("Enemy"))
        {
            Enemy E = other.GetComponent<Enemy>();
            if (E._frozen)
            {
                E.Eaten();
            }
            else
            {
                E.Damage();
            }
        }
    }
}
