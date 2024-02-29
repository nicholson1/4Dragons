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
        RandomRotate();
    }

    public void RandomRotate()
    {
        currentRotation = Random.Range(currentRotation + 20, currentRotation + 270);
        transform.RotateAround(TargetObject.position, Vector3.up, currentRotation);
    }
}
