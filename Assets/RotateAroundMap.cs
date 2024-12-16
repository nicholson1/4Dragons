using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RotateAroundMap : MonoBehaviour
{
    public Transform TargetObject;

    private float currentRotation;
    public static RotateAroundMap _instance;
    
    public bool SlowRotate;
    public float speed = 10f;

    private bool onStart = true;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        StopRandomRotate();
        if (onStart)
        {
            SlowRotate = true;
            onStart = false;
        }
    }
    
    public void ToggleRotate(bool isRotating)
    {
        SlowRotate = isRotating;
    }
    

    public void StopRandomRotate()
    {
        SlowRotate = false;
        currentRotation = Random.Range(currentRotation + 20, currentRotation + 270);
        transform.RotateAround(TargetObject.position, Vector3.up, currentRotation);
    }

    private void Update()
    {
        if (SlowRotate && TargetObject != null)
        {
            // Rotate around the target object on the Y-axis (vertical rotation)
            transform.RotateAround(TargetObject.position, Vector3.up, speed * Time.deltaTime);
        }
    }
}
