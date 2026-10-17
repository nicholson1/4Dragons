using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIRotationHandler : MonoBehaviour
{
    private Canvas canvas;
    private Camera referenceCamera;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        referenceCamera = canvas.worldCamera;
    }

    private void LateUpdate()
    {
        if (!referenceCamera)
            referenceCamera = Camera.main;

        if(isActiveAndEnabled)
            transform.rotation = referenceCamera.transform.rotation;
    }

}
