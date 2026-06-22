using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DFG.UIHandling;

public class ButtonHighligher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image highlighter = null;
    [SerializeField] private Color higlightedColor = Color.white;
    private Selectable selectable = null;
    private float fadeDuration = 0.3f;


    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectable.interactable)
            StartCoroutine(FadeColor(true));
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selectable.interactable)
            StartCoroutine(FadeColor(false));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (selectable.interactable)
            StartCoroutine(FadeColor(true));
    }

    private IEnumerator FadeColor(bool toInvisible)
    {
        var startColor = toInvisible ? higlightedColor : Color.clear;
        var endColor = toInvisible ? Color.clear : higlightedColor;
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            highlighter.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        highlighter.color = endColor;
    }

    private void Awake()
    {
        selectable = GetComponentInParent<Selectable>();

        highlighter.color = Color.clear;        
    }

}
