using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictorySequence : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup victoryImage;
    [SerializeField] private CanvasGroup background;

    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private CanvasGroup darkness;
    [SerializeField] private TextMeshProUGUI darknessText;
    //[SerializeField] private CreditsRoller credits ;
    [SerializeField] private float panelMoveDuration = 0.5f;
    [SerializeField] private AnimationCurve panelMoveCurve;


    public void StartStringAnimation()
    {
        string startString = "Hello";
        string endString = "Hello, World!";
        float duration = 3f; // Duration in seconds

        StartCoroutine(AnimateString(startString, endString, duration, UpdateVictoryText));
    }

    private void UpdateVictoryText(string updatedString)
    {
        victoryText.text = updatedString;
    }

    private void UpdateDarknessText(string updatedString)
    {
        darknessText.text = updatedString;
    }

    public void StartVictorySequence()
    {
        if (continueButton.interactable)
            continueButton.interactable = false;
        StartCoroutine(Sequence());
    }
    
    public IEnumerator Sequence()
    {
        //StartCoroutine(FadeCanvasGroup(victoryImage, 1, 3));
        StartCoroutine(MovePanel(victoryImage.gameObject, PanelMoveDirection.Vertical, true));
        yield return new WaitForSeconds(2);
        Debug.LogError($"Play victory music");
        MusicManager.Instance.PlayVictoryMusic();

        StartCoroutine(FadeCanvasGroup(background, 1, 3));
        StartCoroutine(AnimateString("Victory!", "Victory!...?", 3, UpdateVictoryText));
        yield return new WaitForSeconds(3);
        //StartCoroutine(FadeCanvasGroup(victoryImage, 0, 2));
        StartCoroutine(MovePanel(victoryImage.gameObject, PanelMoveDirection.Vertical, false));
        yield return new WaitForSeconds(2);

        StartCoroutine(FadeCanvasGroup(darkness, 1, 2));
        StartCoroutine(AnimateString("", "The Shadows Still Loom in Wyrmwood....", 5, UpdateDarknessText));
        yield return new WaitForSeconds(7);

        Action<bool> OnButtonMoved = (_) => continueButton.interactable = true;
        StartCoroutine(MovePanel(continueButton.gameObject, PanelMoveDirection.Vertical, true, OnButtonMoved));
        StartCoroutine(FadeCanvasGroup(darkness, 0, 2));
        //UIController._instance.ActivateTitleScreen();

    }

    //fade in victory image
    // start fade in background
    //change victory text to ....?
    // fade out victory image
    // fade in New Text, the darkness still looms
    // fade out credits
    
    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        // Check if the CanvasGroup is valid
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null.");
            yield break;
        }
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

    private IEnumerator MovePanel(GameObject targetPanel, PanelMoveDirection moveDirection, bool toOpen, Action<bool> OnFinished = null)
    {      
        RectTransform rt = targetPanel.GetComponent<RectTransform>();

        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = GetPanelTargetPosition(moveDirection, startPos);

        float t = 0;
        while (t < 1)
        {
            t = t + Time.deltaTime / panelMoveDuration;
            float curvedT = panelMoveCurve.Evaluate(t);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, curvedT);
            yield return null;
        }

        rt.anchoredPosition = endPos;

        OnFinished?.Invoke(toOpen);
    }

    public IEnumerator AnimateString(string startString, string endString, float duration, System.Action<string> onUpdate)
    {
        // Ensure the end string is at least as long as the start string
        if (endString.Length < startString.Length)
        {
            Debug.LogError("End string must be longer or equal in length to start string.");
            yield break;
        }

        float elapsedTime = 0f;
        int totalCharacters = endString.Length;
        int startLength = startString.Length;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the number of characters to show based on time elapsed
            int charactersToShow = Mathf.Clamp(Mathf.FloorToInt((elapsedTime / duration) * totalCharacters), startLength, totalCharacters);

            // Create the current string by taking the appropriate substring of the end string
            string currentString = endString.Substring(0, charactersToShow);

            // Fill remaining characters with placeholders if needed
            if (charactersToShow < totalCharacters)
            {
                currentString += new string(' ', totalCharacters - charactersToShow); // Replace '_' with your placeholder if needed
            }

            onUpdate?.Invoke(currentString);

            yield return null;
        }

        // Ensure the final string matches the end string
        onUpdate?.Invoke(endString);
    }

    private Vector2 GetPanelTargetPosition(PanelMoveDirection direction, Vector2 startPos)
    {
        switch (direction)
        {
            case PanelMoveDirection.Horizontal:
                return new Vector2(startPos.x * -1, startPos.y);

            case PanelMoveDirection.Vertical:
                return new Vector2(startPos.x, startPos.y * -1);
            default:
                Debug.LogError($"Error: PanelMoveDirection for panel you want to move is not set!");
                return startPos;

        }
    }
}
