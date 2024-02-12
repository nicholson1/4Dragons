using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionPingPong : MonoBehaviour
{
    public float minIntensity = 0.5f; // Minimum intensity of emission
    public float maxIntensity = 2.0f; // Maximum intensity of emission
    public float speed = 1.0f; // Speed of the ping pong effect
    public Color EmissionColor;
    
    private Material pingPongMaterial;
    private float currentIntensity;
    private bool increasingIntensity = true;

    private void Start()
    {
        InitializeMaterial();
    }

    public void InitializeMaterial()
    {
        pingPongMaterial = GetComponent<Renderer>().material;
        currentIntensity = minIntensity;
        EmissionColor = pingPongMaterial.GetColor("_EmissionColor");
    }

    void Update()
    {
        // Adjust intensity based on time
        if (increasingIntensity)
        {
            currentIntensity += Time.deltaTime * speed;
            if (currentIntensity >= maxIntensity)
                increasingIntensity = false;
        }
        else
        {
            currentIntensity -= Time.deltaTime * speed;
            if (currentIntensity <= minIntensity)
                increasingIntensity = true;
        }

        // Apply intensity to emission color
        Color newEmissionColor = EmissionColor * currentIntensity;
        pingPongMaterial.SetColor("_EmissionColor", newEmissionColor);

        // Refresh material properties
        pingPongMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        DynamicGI.SetEmissive(GetComponent<Renderer>(), newEmissionColor);
    }
}
