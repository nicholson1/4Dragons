using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPingPongEffect : MonoBehaviour
{
    public Image targetImage; // The UI Image component to modify

    // Scale variables
    [SerializeField] private Vector3 minScale = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 maxScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float scaleSpeed = 1f;

    // Color variables
    [SerializeField] private Color color1 = Color.white;
    [SerializeField] private Color color2 = Color.red;
    public float colorSpeed = 1f;


    private void Start()
    {
        targetImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (targetImage == null)
            return;

        // PingPong the scale
        float scaleLerp = Mathf.PingPong(Time.time * scaleSpeed, 1);
        transform.localScale = Vector3.Lerp(minScale, maxScale, scaleLerp);

        // PingPong the color
        float colorLerp = Mathf.PingPong(Time.time * colorSpeed, 1);
        targetImage.color = Color.Lerp(color1, color2, colorLerp);
    }
}