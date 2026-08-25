using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitionController : MonoBehaviour
{
    // ienumerator, that takes a time and unparents the camera then reparrents it after the time has passed, and then resets local position and rotation
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    //singleton
    public static CameraTransitionController _instance;
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public void Start()
    {
        originalPosition = Camera.main.transform.localPosition;
        originalRotation = Camera.main.transform.localRotation;
    }
    //lets also have a ui canvas black screen fade in and out after the camera transition, so we can hide the transition from the player

    public void TransitionCamera(float time)
    {
        StartCoroutine(TransitionCameraEnum(time));
    }
    public IEnumerator TransitionCameraEnum(float time)
    {
        // unparent the camera
        Debug.Log("Transitioning camera for " + time + " seconds");
        Camera.main.transform.parent = null;
        yield return new WaitForSeconds(time-2);
        // fade in the black screen
        BlackFade._instance.FadeInScreen(1f);
        yield return new WaitForSeconds(2f);
        // reparent the camera
        Camera.main.transform.parent = transform;
        // reset local position and rotation
        Camera.main.transform.localPosition = originalPosition;
        Camera.main.transform.localRotation = originalRotation;
        BlackFade._instance.FadeOutScreen(1f);

    }
}
