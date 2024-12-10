using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkInProgress : MonoBehaviour
{
    public static WorkInProgress _instance;
    [SerializeField] private CanvasGroup _canvasGroup;

    private bool hasDisplayed = false;
    
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
        
        DontDestroyOnLoad(this.gameObject);
    }

    public void OpenWorkInProgress()
    {
        if(hasDisplayed)
        {
            //_canvasGroup.gameObject.SetActive(false);
            return;
        }

        _canvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeCanvasGroup(_canvasGroup, 1, 1));
        hasDisplayed = true;
    }
    
    public void CloseWorkInProgess()
    {
        _canvasGroup.blocksRaycasts = false;
        StartCoroutine(FadeCanvasGroup(_canvasGroup, 0, 1));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        // Check if the CanvasGroup is valid
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null.");
            yield break;
        }

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(true);

        // Store the initial alpha value
        float startAlpha = canvasGroup.alpha;

        // Track the time elapsed
        float elapsedTime = 0f;
        

        // Gradually change the alpha value
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        // Set the final alpha value to ensure it reaches the target
        canvasGroup.alpha = targetAlpha;
        if(targetAlpha == 0)
            canvasGroup.gameObject.SetActive(false);
        

    }
}
