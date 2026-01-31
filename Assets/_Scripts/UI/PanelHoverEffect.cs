using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelHoverEffect : MonoBehaviour
{
    public float hoveredScale = 1.1f;
    private float scalingTime = 0.2f;

    private Vector3 initialScale = Vector3.one;

    private SelectionItem selectionItemPanel;
    private bool isScaled = false;

    public void ScaleUp()
    {
        ScaleUIElement(initialScale * hoveredScale, scalingTime);
    }

    public void ScaleDown()
    {
        ScaleUIElement(initialScale, scalingTime);
    }

    private void ScaleUIElement(Vector3 targetScale, float duration)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, targetScale, duration).setEaseInOutQuad();
    }

    private void Start()
    {
        selectionItemPanel = GetComponent<SelectionItem>();
        initialScale = transform.localScale;

    }
}
