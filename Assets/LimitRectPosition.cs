using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitRectPosition : MonoBehaviour
{
    [field: SerializeField] public RectTransform RectTransform {get; private set;}
    [field: SerializeField] public RectTransform[] ParentOffsets {get; private set;}
    [field: SerializeField] public Vector2 MinPosition { get; private set; } = new Vector2(-520f, -400f);
    [field: SerializeField] public Vector2 MaxPosition { get; private set; } = new Vector2(700f, 0f);

    Vector2 minPosition;
    Vector2 maxPosition;
    Vector2[] parentOffsets; 

    private void Awake()
    {
        if (RectTransform == null)
            RectTransform = GetComponent<RectTransform>();

        parentOffsets = new Vector2[ParentOffsets.Length];
    }

    void Update()
    {
        SetBounds();

        Vector2 position = RectTransform.anchoredPosition;
        bool setPosition = false;

        if (position.x < minPosition.x) setPosition = true;
        if (position.x > maxPosition.x) setPosition = true;
        if (position.y < minPosition.y) setPosition = true;
        if (position.y > maxPosition.y) setPosition = true;

        if (!setPosition)
            return;

        position.x = Mathf.Clamp(position.x, minPosition.x, maxPosition.x);
        position.y = Mathf.Clamp(position.y, minPosition.y, maxPosition.y);
        RectTransform.anchoredPosition = position;
    }

    private void SetBounds()
    {
        Vector2 offset = Vector2.zero;

        if (parentOffsets == null)
            parentOffsets = new Vector2[ParentOffsets.Length];

        if (parentOffsets.Length != ParentOffsets.Length)
            parentOffsets = new Vector2[ParentOffsets.Length];

        for (int i = 0; i < ParentOffsets.Length; i++)
        {
            parentOffsets[i] += ParentOffsets[i].anchoredPosition;
            offset += parentOffsets[i];
        }       

        minPosition = MinPosition + offset;
        maxPosition = MaxPosition + offset;
    }

    internal void SetLimits(Vector2 min, Vector2 max)
    {
        MinPosition = min; 
        MaxPosition = max;
    }
}
