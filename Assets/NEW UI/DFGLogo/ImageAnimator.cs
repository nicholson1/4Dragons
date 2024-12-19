using System;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using PlayFab.Internal;
using Unity.VisualScripting;
using UnityEngine.UI;

public class ImageAnimatior : MonoBehaviour {

    public Sprite[] sprites;
    public int spritePerFrame = 6;
    public bool loop = true;
    public bool destroyOnEnd = false;

    private int index = 0;
    private Image image;
    private int frame = 0;

    [SerializeField] private CanvasGroup Logo;
    [SerializeField] private CanvasGroup parent;

    void Awake() {
        image = GetComponent<Image> ();
    }

    private void Start()
    {
        if (WorkInProgress._instance.hasDisplayed)
        {
            parent.gameObject.SetActive(false);
            return;
        }

        parent.alpha = 1;
        StartCoroutine(FadeCanvasGroup(Logo, 1, 2,0));
        StartCoroutine(ChangeSpritesSequentially(3));
    }

    void FadeOut()
    {
        StartCoroutine(FadeCanvasGroup(parent, 0, 1,2));
    }
    
    
    private IEnumerator ChangeSpritesSequentially(float totalDuration)
    {
        float timePerSprite = totalDuration / sprites.Length; // Calculate time per sprite

        for (int i = 0; i < sprites.Length; i++)
        {
            image.sprite = sprites[i]; // Change the sprite
            yield return new WaitForSeconds(timePerSprite); // Wait for the specified time
        }
    }
    

    // void Update () {
    //     if (!loop && index == sprites.Length) return;
    //     frame ++;
    //     if (frame < spritePerFrame) return;
    //     image.sprite = sprites [index];
    //     frame = 0;
    //     index ++;
    //     if (index >= sprites.Length) {
    //         if (loop) index = 0;
    //         if (destroyOnEnd) Destroy (gameObject);
    //     }
    // }
    
    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration, float delay)
    {
        // Check if the CanvasGroup is valid
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null.");
            yield break;
        }

        yield return new WaitForSeconds(delay);

        //canvasGroup.alpha = 0;
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
        if(targetAlpha >= 1)
            FadeOut();
        if(targetAlpha == 0)
            canvasGroup.gameObject.SetActive(false);
        

    }
}