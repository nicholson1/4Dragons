using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackFade : MonoBehaviour
{
   //singleton that fades the screen to black and back, and can be called from anywhere in the game
    public static BlackFade _instance;
    CanvasGroup canvasGroup;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        canvasGroup = GetComponentInChildren<CanvasGroup>();

    }

    public void FadeInScreen(float time)
    {
        StartCoroutine(FadeIn(time));
    }
    public void FadeOutScreen(float time)
    {
        StartCoroutine(FadeOut(time));
    }
    IEnumerator FadeIn(float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / time);
            yield return null;
        }
    }
    IEnumerator FadeOut(float time)
    {
       
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / time));
            yield return null;
        }
    }
}
