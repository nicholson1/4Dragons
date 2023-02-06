using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] Transform targetCamera;

    private void Start()
    {
        CombatTrigger.TriggerCombat += UpdateCombatUI;

    }

    private void OnDestroy()
    {
        CombatTrigger.TriggerCombat -= UpdateCombatUI;

    }

    private void UpdateCombatUI(Character arg1, Character arg2)
    {
        transform.LookAt(targetCamera);
    }

    
}
