using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGlow : MonoBehaviour
{
    public Image targetImage; // The Image component to scale and fade
    [SerializeField]private Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f); // Desired scale
    [SerializeField]private float fadeInTime = 1.0f; // Time to fade in
    [SerializeField]private float fadeOutTime = 1.0f; // Time to fade out

    private Vector3 initialScale;
    private Color initialColor;

    private Coroutine co = null;
    private void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        initialScale = targetImage.transform.localScale;
        initialColor = targetImage.color;
        
        gameObject.SetActive(false);
    }

    public void TriggerEffect(Color c)
    {
        if (targetImage != null)
        {
            gameObject.SetActive(true);
            if(co != null)
                StopCoroutine(co);
            co = StartCoroutine(ScaleAndFadeCoroutine(c, targetScale, fadeInTime, fadeOutTime));
        }
    }
    private IEnumerator ScaleAndFadeCoroutine(Color targetColor, Vector3 targetScale, float fadeInTime, float fadeOutTime)
    {
        // Scale up and fade in
        float elapsedTime = 0f;
        Color transparentColor = new Color(0, 0, 0, 0f);

        // Fade-in effect
        targetImage.color = transparentColor;

        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime / fadeInTime;
            targetImage.color = Color.Lerp(transparentColor, targetColor, t *2);
            targetImage.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetImage.color = targetColor;
        targetImage.transform.localScale = targetScale;

        // Wait before fading out if needed (optional)
        yield return new WaitForSeconds(0.5f);

        // Fade-out effect
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;
            targetImage.color = Color.Lerp(initialColor, transparentColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetImage.color = transparentColor;
        targetImage.transform.localScale = initialScale;
    }
}
