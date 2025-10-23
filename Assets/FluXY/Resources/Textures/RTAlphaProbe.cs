using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTAlphaProbe : MonoBehaviour
{
    public RenderTexture rt;

    float timer = 3;
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Check();
            timer = 3;
        }
    }

    void Check()
    {
        if (!rt) { Debug.LogWarning("No RT assigned"); return; }

        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        Color corner = tex.GetPixel(0, 0);
        Color center = tex.GetPixel(rt.width / 2, rt.height / 2);

        Debug.Log($"Corner RGBA {corner}  (should be A≈0)");
        Debug.Log($"Center RGBA {center}  (should be A≈1 if model is centered)");

        RenderTexture.active = prev;
        Destroy(tex);
    }
}   
