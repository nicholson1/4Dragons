using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
    [SerializeField] private MeshRenderer mesh;

    [SerializeField] private Material fadeMat;
    [SerializeField] private Material solidMat;


    public void FadeOut()
    {
        StartCoroutine(FadeOut(2));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeIn(2));

    }
    
    private IEnumerator FadeOut(float fadeDuration)
    {
        //wDebug.LogWarning("time to fade");
        Color initialColor = mesh.material.color;
        mesh.material = fadeMat;
        mesh.material.color = initialColor;

        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            mesh.material.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }
    }
    
    private IEnumerator FadeIn(float fadeDuration)
    {
        //Debug.LogWarning("time to fade");
        Color initialColor = mesh.material.color;

        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            mesh.material.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }

        mesh.material = solidMat;
    }
    
}
