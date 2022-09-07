using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupControl : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                enemy.GetComponent<Enemy>().Freeze();
            }

        }
    }
}
