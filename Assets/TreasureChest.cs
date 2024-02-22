using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    public Transform Lid;
    private bool isOpen = false;
    public float targetRotation = -140f;
    public float rotationSpeed = 5f;
    private Quaternion initialRotation;
    private bool isRotating = false;

    public bool startingChest;
    private void OnMouseDown()
    {
        if(isOpen)
            return;
        if (!isOpen)
        {
            isRotating = true;
        }

        if (startingChest)
        {
            SelectionManager._instance.CreateEquipmentListsStart();
        }
        UIController._instance.ToggleLootUI(1);
        UIController._instance.ToggleInventoryUI(1);
    }
    
    void Start()
    {
        initialRotation = Lid.transform.localRotation;
    }

    void Update()
    {
        if (isRotating)
        {
            // Interpolate between the current rotation and the target rotation
            Lid.transform.localRotation = Quaternion.Lerp(Lid.transform.localRotation, Quaternion.Euler(targetRotation, 0, 0), rotationSpeed * Time.deltaTime);

            isOpen = true;
        }
    }
}
