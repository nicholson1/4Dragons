using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f; // Scale factor when hovered
    private float shakeAmount = 1f; // Amount to shake when hovered (in degrees)
    private float shakeTime = .1f; // Speed of the shake effect

    private Vector3 initialScale = new Vector3(1,1,1); // Initial scale of the UI element
    private Quaternion initialRotation = Quaternion.identity; // Initial scale of the UI element

    [SerializeField]public bool shakeUI = true;
    
    private bool setOnce = false;
    
    void Start()
    {
        

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!setOnce)
        {
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
            setOnce = true;
        }
        // Scale up and start shaking when mouse enters the UI element
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, initialScale * hoverScale, 0.2f).setEaseInOutQuad();
        if(shakeUI)
            ShakeUIElement();
        //LeanTween.rotateZ(gameObject, shakeAmount, shakeSpeed).setLoopPingPong().setEaseInOutQuad();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale down and stop shaking when mouse leaves the UI element
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, initialScale, 0.2f).setEaseInOutQuad();
        if(shakeUI)
            gameObject.transform.localRotation = initialRotation;
        //LeanTween.cancel(gameObject, "rotateZ");
        //transform.localRotation = Quaternion.identity;
    }
    
    void ShakeUIElement()
    {
        // Shake the UI element by rotating it back and forth
        //LeanTween.cancel(gameObject, false); // Cancel any ongoing tween

        // Shake left
        LeanTween.rotateLocal(gameObject, new Vector3(0f, 0f, shakeAmount), shakeTime / 2)
            .setEaseInOutQuad();

        // Shake right
        LeanTween.rotateLocal(gameObject, new Vector3(0f, 0f, -shakeAmount), shakeTime)
            .setEaseInOutQuad()
            .setDelay(shakeTime / 2)
            .setLoopPingPong(1);
    }

    public void FlashScale()
    {
        StartCoroutine(BlinkScale());
    }
    private IEnumerator BlinkScale()
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, initialScale * hoverScale, 0.1f).setEaseInOutQuad();
        yield return new WaitForSeconds(.5f);
        LeanTween.scale(gameObject, initialScale, 0.1f).setEaseInOutQuad();

    }
}