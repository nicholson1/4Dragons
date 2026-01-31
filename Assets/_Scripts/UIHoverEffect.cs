using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public float hoverScale = 1.1f; // Scale factor when hovered
    private float shakeAmount = 1f; // Amount to shake when hovered (in degrees)
    private float shakeTime = .1f; // Speed of the shake effect

    private Vector3 initialScale = new Vector3(1,1,1); // Initial scale of the UI element
    private Quaternion initialRotation = Quaternion.identity; // Initial scale of the UI element

    public bool shakeUI = false;
    
    private bool setOnce = false;

    private bool isPointerEvent = false; //a guard to prevent pointer event triggering on gamepad select
    protected InputHandler inputHandler = null;

    //protected UIScreen buttonOwnerUIScreen = null;

    private Selectable selectable = null;

    private bool shouldTween = true;
    private bool shouldShake = true;


    public void SetTweening(bool canTween)
    {
        shouldTween = canTween;
    }

    public void SetShake(bool canShake)
    {
        if (shouldShake != canShake)
            shouldShake = canShake;

        if(!shouldShake)
            gameObject.transform.localRotation = initialRotation;
    }

    public void ResetScale()
    {
        initialScale = transform.localScale;
        initialRotation = transform.localRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {        
        isPointerEvent = true;
                

        if(!setOnce)
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

    public void OnSelect(BaseEventData eventData)
    {
        if (!selectable.interactable) return;

        if (!setOnce)
        {
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
            setOnce = true;
        }
        // Scale up and start shaking when mouse enters the UI element
        if(shouldTween)
        {
            ScaleUIElement(initialScale * hoverScale, 0.2f);
        }

        if (shakeUI)
            ShakeUIElement();
        //LeanTween.rotateZ(gameObject, shakeAmount, shakeSpeed).setLoopPingPong().setEaseInOutQuad();


        UIController._instance.PlayUIHover();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isPointerEvent)
        {
            isPointerEvent = false;
            return;
        }

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
        if(this.isActiveAndEnabled)
            StartCoroutine(BlinkScale(time));
    }

    private IEnumerator BlinkScale(float time = .5f)
    {
        if(!setOnce)
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

    //protected void HandleClickThroughYes()
    //{
    //    var button = this.button as Button;
    //    if(button == null)
    //    {
    //        Debug.LogError($"Error: Cannot cast selectable as button! /l" +
    //            $"Probably you need to override HandleYesThroughInput()");
    //        return;
    //    }

    //    if (EventSystem.current.currentSelectedGameObject == this.gameObject)
    //        button.onClick.Invoke();
    //}

    //protected void HandleClickThroughNo()
    //{
    //    ClickThroughInput();
    //}

    //protected void ClickThroughInput()
    //{
    //    var button = this.button;
    //    if (button == null)
    //    {
    //        Debug.LogError($"Error: Cannot cast selectable as button! /l" +
    //            $"Probably you need to override ClickThroughInput()");
    //        return;
    //    }
        
    //    if (!button.interactable) return;

    //    button.onClick.Invoke();        
    //}


    //protected virtual void ToggleButtonInteractability(UIScreen screen)
    //{

    //}


    //protected void BindGamepadToButton()
    //{
    //    if (button == null)
    //        return;

    //    if(clickableWithYes)
    //        inputHandler.OnYes.AddListener(HandleClickThroughYes);

    //    if (clickableWithNo)
    //        inputHandler.OnNo.AddListener(ClickThroughInput);

    //    switch(extraButtonToUse)
    //    {
    //        case ExtraButton.Extra1:
    //            inputHandler.OnMenuExtra1.AddListener(ClickThroughInput);
    //            break;
    //        case ExtraButton.Extra2:
    //            inputHandler.OnMenuExtra2.AddListener(ClickThroughInput);
    //            break;
    //        case ExtraButton.Start:
    //            inputHandler.OnStart.AddListener(ClickThroughInput);
    //            break;
    //        case ExtraButton.Select:
    //            inputHandler.OnSelect.AddListener(ClickThroughInput);
    //            break;

    //    }
    //}

    //protected void UnbindGamepadFromButton()
    //{
    //    if (button == null)
    //        return;

    //    if(clickableWithYes)
    //        inputHandler.OnYes.RemoveListener(HandleClickThroughYes);

    //    if (clickableWithNo)
    //        inputHandler.OnNo.RemoveListener(ClickThroughInput);


    //    switch (extraButtonToUse)
    //    {
    //        case ExtraButton.Extra1:
    //            inputHandler.OnMenuExtra1.RemoveListener(ClickThroughInput);
    //            break;
    //        case ExtraButton.Extra2:
    //            inputHandler.OnMenuExtra2.RemoveListener(ClickThroughInput);
    //            break;
    //        case ExtraButton.Start:
    //            inputHandler.OnStart.RemoveListener(ClickThroughInput);
    //            break;
    //        case ExtraButton.Select:
    //            inputHandler.OnSelect.RemoveListener(ClickThroughInput);
    //            break;


    //    }
    //}

    //private void BindInput(UIScreen screen, bool navigatable)
    //{
    //    UnbindGamepadFromButton();

    //    if(navigatable)
    //        BindGamepadToButton();            
    //}

    //private void UnbindInput(UIScreen _)
    //{
    //    UnbindGamepadFromButton();
    //    ManualDeselect();
    //}

    //public virtual void SetUIScreen(UIScreen screen)
    //{
    //    buttonOwnerUIScreen = screen;
    //    buttonOwnerUIScreen.OnNewScreenActive += BindInput;
    //    buttonOwnerUIScreen.OnScreenDeactivated += UnbindInput;
    //}

    private void HandleInputChange(InputType inputType)
    {

    }

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
                
        //if(shouldHaveClickableButton)
        //    button ??= GetComponentInChildren<Button>();

        inputHandler.OnInputTypeChange += HandleInputChange;
        shouldShake = shakeUI;
    }

    private void OnDestroy()
    {
        //if (buttonOwnerUIScreen != null)
        //{
        //    buttonOwnerUIScreen.OnNewScreenActive -= BindInput;
        //    buttonOwnerUIScreen.OnScreenDeactivated -= UnbindInput;
        //}

        inputHandler.OnInputTypeChange -= HandleInputChange;
    }
   
}