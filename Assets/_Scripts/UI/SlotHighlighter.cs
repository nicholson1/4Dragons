using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotHighlighter : MonoBehaviour
{
    [SerializeField] private float peakValue;
    [SerializeField] private float stableValue;
    [SerializeField] private float blinkSpeed;
    [SerializeField] private float fadeOutSpeed;

    private Image image;


    public void ToggleHighlighter(bool toOn)
    {
        if (!toOn)
        {
            //lerp alpha to 0
            return;
        }

        //pingpong alpha between peak and stable for a few moments, then lerp to stable
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }
}
