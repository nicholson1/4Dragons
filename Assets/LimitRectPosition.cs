using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitRectPosition : MonoBehaviour
{
    [field: SerializeField] public RectTransform RectTransform {get; private set;}
    [field: SerializeField] public Vector2 MinPosition { get; private set; } = new Vector2(-520f, -400f);
    [field: SerializeField] public Vector2 MaxPosition { get; private set; } = new Vector2(700f, 0f);

    private void Awake()
    {
        if (RectTransform == null)
            RectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        Vector2 position = RectTransform.anchoredPosition;
        bool setPosition = false;

        if (position.x < MinPosition.x) setPosition = true;
        if (position.x > MaxPosition.x) setPosition = true;
        if (position.y < MinPosition.y) setPosition = true;
        if (position.y > MaxPosition.y) setPosition = true;

        if (!setPosition)
            return;

        position.x = Mathf.Clamp(position.x, MinPosition.x, MaxPosition.x);
        position.y = Mathf.Clamp(position.y, MinPosition.y, MaxPosition.y);
        RectTransform.anchoredPosition = position;
    }
}
