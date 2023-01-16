using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    public static event Action<Character, Character> TriggerCombat;
    public static event Action EndCombat;

    private CharacterController cc;
    private void Start()
    {
        cc = GetComponentInParent<CharacterController>();
    }

    public void EndTheCombat()
    {
        EndCombat();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {

            if (cc.isGrounded)
            {
                //initiate combat;

                TriggerCombat(this.GetComponentInParent<Character>(), other.GetComponentInParent<Character>());
            }
        }

    }
}
