using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    public static CameraShake _instance;
    public AnimationCurve Curve;
    public bool ScreenShakeEnabled = true;
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

        ScreenShakeEnabled = (PlayerPrefsManager.getScreenShake() == 1);
    }

    public void ToggleScreenShake()
    {
        ScreenShakeEnabled = !ScreenShakeEnabled;
        if(ScreenShakeEnabled)
            PlayerPrefsManager.SetScreenShake(1);
        else
            PlayerPrefsManager.SetScreenShake(0);

    }

    public void GetHit(int hitCost)
    {
        HitCost = hitCost;
        if (!ScreenShakeEnabled)
        {
            return;
        }
        StartCoroutine(Shaking());
        
    }

    private float duration = 1f;
    public float HitCost = 4;

    public bool start = false;

    IEnumerator Shaking()
    {
        duration = .25f * HitCost;
        Vector3 startPos = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = Curve.Evaluate(elapsedTime / duration);
            transform.position = startPos + Random.insideUnitSphere * strength * (HitCost/4);
            yield return null;
        }

        transform.position = startPos;
    }

    private void Update()
    {
        if (start)
        {
            start = false;
            StartCoroutine(Shaking());

        }
    }
}
