using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItemHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f; // Scale factor when hovered
    private float shakeAmount = 1f; // Amount to shake when hovered (in degrees)
    private float shakeTime = .1f; // Speed of the shake effect

    private Vector3 initialScale = new Vector3(1, 1, 1); // Initial scale of the UI element
    private Quaternion initialRotation = Quaternion.identity; // Initial scale of the UI element

    public bool shakeUI = false;

    private bool setOnce = false;

    private bool isPointerEvent = false; //a guard to prevent pointer event triggering on gamepad select
    protected InputHandler inputHandler = null;

    //protected UIScreen buttonOwnerUIScreen = null;

    private Selectable selectable = null;

    private bool shouldTween = true;
    private bool shouldShake = true;

    public void GamepadSelect()
    {
        //if (!setOnce)
        //{

            initialScale = transform.localScale;
            initialRotation = transform.localRotation;

        //    setOnce = true;
        //}

        // Scale up and start shaking when mouse enters the UI element
        if (shouldTween)
        {
            ScaleUIElement(initialScale * hoverScale, 0.2f);
        }

        if (shakeUI)
            ShakeUIElement();
        //LeanTween.rotateZ(gameObject, shakeAmount, shakeSpeed).setLoopPingPong().setEaseInOutQuad();        

        UIController._instance.PlayUIHover();
    }

    public void GamepadDeselect()
    {

        ScaleUIElement(initialScale, 0.2f);

        gameObject.transform.localRotation = initialRotation;

        
    }

    public void SetTweening(bool canTween)
    {
        shouldTween = canTween;
    }

    public void SetShake(bool canShake)
    {
        if (shouldShake != canShake)
            shouldShake = canShake;

        if (!shouldShake)
            gameObject.transform.localRotation = initialRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerEvent = true;


        if (!setOnce)
        {
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
            setOnce = true;
        }

        // Scale up and start shaking when mouse enters the UI element
        if (shouldTween)
        {
            ScaleUIElement(initialScale * hoverScale, 0.2f);
        }

        if (shakeUI)
            ShakeUIElement();
        //LeanTween.rotateZ(gameObject, shakeAmount, shakeSpeed).setLoopPingPong().setEaseInOutQuad();        

        UIController._instance.PlayUIHover();

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerEvent = true;

        ScaleUIElement(initialScale, 0.2f);

        gameObject.transform.localRotation = initialRotation;
    }

    private void ScaleUIElement(Vector3 targetScale, float duration)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, targetScale, duration).setEaseInOutQuad();
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

    public void FlashScale(float time = .5f)
    {
        if (this.isActiveAndEnabled)
            StartCoroutine(BlinkScale(time));
    }

    private IEnumerator BlinkScale(float time = .5f)
    {
        if (!setOnce)
        {
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
            setOnce = true;
        }
        //LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, initialScale * hoverScale, 0.1f).setEaseInOutQuad();
        yield return new WaitForSeconds(time);
        LeanTween.scale(gameObject, initialScale, 0.1f).setEaseInOutQuad();

    }

    private void HandleInputChange(InputType inputType)
    {

    }

    // Start is called before the first frame update
    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();

        inputHandler.OnInputTypeChange += HandleInputChange;
        shouldShake = shakeUI;
        initialScale = Vector3.zero;
    }

    private void OnDestroy()
    {
        inputHandler.OnInputTypeChange -= HandleInputChange;
    }

}
