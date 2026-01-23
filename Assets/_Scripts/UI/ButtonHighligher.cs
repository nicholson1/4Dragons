using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighligher : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image highlighter = null;
    private Selectable selectable = null;
    private float fadeDuration = 0.3f;

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(FadeColor(false));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(FadeColor(true));
    }

    private IEnumerator FadeColor(bool toInvisible)
    {
        var startColor = toInvisible ? Color.white : Color.clear;
        var endColor = toInvisible ? Color.clear : Color.white;
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            highlighter.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        highlighter.color = endColor;
    }

    private void Start()
    {
        selectable = GetComponentInParent<Selectable>();
        highlighter.color = Color.clear;        
    }


}
