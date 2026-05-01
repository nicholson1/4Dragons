using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotHighlighter : MonoBehaviour
{
    [SerializeField] private float peakValue;
    [SerializeField] private float stableValue;
    [SerializeField] private float blinkSpeed;
    [SerializeField] private float fadeSpeed = 0.7f;
    private float blinkDuration = 1f;

    private Image image;

    private Coroutine blinkRoutine = null;

    public void ToggleHighlighter(bool toOn)
    {
        if (!toOn)
        {
            StartHide();
            return;
        }

        StartBlink();
        //pingpong alpha between peak and stable for a few moments, then lerp to stable
    }

    private void StartBlink()
    {
        if(blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void StartHide()
    {
        if(blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if(image.color.a > 0)
            StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        Color color = image.color;

        while (color.a > 0f)
        {
            float value = color.a;
            color.a = Mathf.MoveTowards(value, 0f, fadeSpeed * Time.deltaTime);
            image.color = color;
            yield return null;
        }

        color.a = 0f;
        image.color = color;

    }

    private IEnumerator BlinkRoutine()
    {
        float elapsed = 0f;
        Color color = image.color;

        while (color.a <= peakValue)
        {
            float value = color.a;

            color.a = Mathf.MoveTowards(value, peakValue, blinkSpeed * Time.deltaTime);
            image.color = color;            
            yield return null;
        }

        color.a = peakValue;
        image.color = color;

        while(color.a > stableValue)
        {
            float value = color.a;
            color.a = Mathf.MoveTowards(value, stableValue, blinkSpeed * Time.deltaTime);
            image.color = color;
            yield return null;
        }

        color.a = stableValue;
        image.color = color;
    }

    float GetValue(float elapsed)
    {
        // Higher speed = faster animation
        float scaledElapsed = elapsed * blinkSpeed;

        float q = blinkDuration / 4;

        if (scaledElapsed < q)
        {
            float t = scaledElapsed / q;
            return Mathf.Lerp(0f, peakValue, t);
        }

        if (scaledElapsed < q * 3f)
        {
            // 2nd + 3rd quarter: pingpong between 100 and 200
            float phaseElapsed = scaledElapsed - q;
            float phaseDuration = q * 2f;

            float normalized = phaseElapsed / phaseDuration;

            float bounceCount = 2f;
            float t = Mathf.PingPong(normalized * bounceCount, 1f);

            return Mathf.Lerp(stableValue, peakValue, t);
        }

        if (scaledElapsed < blinkDuration)
        {
            // 4th quarter: move from last pingpong value to 100
            float phaseElapsed = scaledElapsed - q * 3f;
            float t = phaseElapsed / q;

            float bounceCount = 2f;

            // value at the exact end of middle phase
            float startPingPongT = Mathf.PingPong(1f * bounceCount, 1f);
            float startValue = Mathf.Lerp(stableValue, peakValue, startPingPongT);

            return Mathf.Lerp(startValue, stableValue, t);
        }

        return stableValue;
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }
}
