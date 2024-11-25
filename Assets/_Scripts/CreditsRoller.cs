using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsRoller : MonoBehaviour
{
    public RectTransform creditsText; // The RectTransform of the credits text
    public float scrollSpeed = 50f;   // Speed at which the credits roll
    public float resetDelay = 5f;     // Time before credits loop or stop
    [SerializeField] private CanvasGroup _canvasGroup; 

    private Vector3 startPosition;
    public bool scroll = false;

    void Start()
    {
        // Save the initial position of the credits
        startPosition = creditsText.localPosition;
    }

    public void StartScroll()
    {
        creditsText.localPosition = startPosition;
        StartCoroutine(FadeIn(_canvasGroup, 2));
        scroll = true;
    }

    void Update()
    {
        if(!scroll)
            return;
        // Move the credits upward
        creditsText.localPosition += Vector3.up * scrollSpeed * Time.deltaTime;

        // Check if the credits have scrolled off the top
        if (creditsText.localPosition.y > Screen.height + creditsText.rect.height)
        {
            // Delay and reset the position
            Invoke(nameof(ResetPosition), resetDelay);
        }
    }
    
    public IEnumerator FadeIn(CanvasGroup targetCanvasGroup, float duration)
    {
        targetCanvasGroup.gameObject.SetActive(true);
        float elapsedTime = 0f;

        // Ensure the CanvasGroup is visible and starts from 0 alpha
        targetCanvasGroup.alpha = 0;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            targetCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null; // Wait until the next frame
        }

        // Make sure alpha is set to 1 after fading
        targetCanvasGroup.alpha = 1;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;
        
    }

    void ResetPosition()
    {
        creditsText.localPosition = startPosition;
    }
}
