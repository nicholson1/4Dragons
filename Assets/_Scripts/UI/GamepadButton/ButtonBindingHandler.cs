using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InputIcons;
using System;

public class ButtonBindingHandler : MonoBehaviour
{
    private bool isPointerEvent = false; //a guard to prevent pointer event triggering on gamepad select
    protected UIScreen buttonOwnerUIScreen = null;

    protected Button button = null;
    protected InputHandler inputHandler = null;

    [SerializeField] protected ExtraButton extraButtonToUse = ExtraButton.None;
    [SerializeField] protected bool clickableWithYes = true;

    [SerializeField] protected bool shouldHaveClickableButton = true;

    private Image glyphImage = null;

    public virtual void ButtonClickCallback()
    {
        Debug.LogError($"Error: No function bind");
    }

    protected virtual void HandleClickThroughYes()
    {
        if (button == null)
        {
            Debug.LogError($"Error: Cannot cast selectable as button! /l" +
                $"Probably you need to override HandleYesThroughInput()");
            return;
        }

        if (!button.interactable) return;

        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
            button.onClick.Invoke();
    }

    protected virtual void ClickThroughInput()
    {
        if (button == null)
        {
            Debug.LogError($"Error: Cannot cast selectable as button! /l" +
                $"Probably you need to override ClickThroughInput()");
            return;
        }

        if (!button.interactable) return;

        button.onClick.Invoke();
    }


    protected virtual void ToggleButtonInteractability(UIScreen screen)
    {
        throw new NotImplementedException();
    }


    protected void BindGamepadToButton()
    {
        if (button == null)
            return;

        if (clickableWithYes)
            inputHandler.OnYes.AddListener(HandleClickThroughYes);

        switch (extraButtonToUse)
        {
            case ExtraButton.Extra1:
                inputHandler.OnMenuExtra1.AddListener(ClickThroughInput);
                break;
            case ExtraButton.Extra2:
                inputHandler.OnMenuExtra2.AddListener(ClickThroughInput);
                break;
            case ExtraButton.Start:
                inputHandler.OnStart.AddListener(ClickThroughInput);
                break;
            case ExtraButton.Select:
                inputHandler.OnSelect.AddListener(ClickThroughInput);
                break;
            case ExtraButton.L1:
                inputHandler.OnL1.AddListener(ClickThroughInput);
                break;
            case ExtraButton.R1:
                inputHandler.OnR1.AddListener(ClickThroughInput);
                break;

        }

        if (glyphImage != null)
            glyphImage.enabled = true;
    }

    protected void UnbindGamepadFromButton()
    {
        if (button == null)
            return;

        if (clickableWithYes)
            inputHandler.OnYes.RemoveListener(HandleClickThroughYes);

        switch (extraButtonToUse)
        {
            case ExtraButton.Extra1:
                inputHandler.OnMenuExtra1.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.Extra2:
                inputHandler.OnMenuExtra2.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.Start:
                inputHandler.OnStart.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.Select:
                inputHandler.OnSelect.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.L1:
                inputHandler.OnL1.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.R1:
                inputHandler.OnR1.AddListener(ClickThroughInput);
                break;
        }

        if (glyphImage != null)
            glyphImage.enabled = false;
    }

    protected void BindInput(UIScreen screen, bool navigatable)
    {
        UnbindGamepadFromButton();

        if (navigatable)
            BindGamepadToButton();
    }

    protected void UnbindInput(UIScreen _)
    {
        UnbindGamepadFromButton();        
    }

    /// <summary>
    /// for non UIScreen panel buttons that requires runtime binding/unbinding    /// </summary>
    public void ManualBindInput(bool toBind)
    {
        UnbindGamepadFromButton();

        if(toBind)
            BindGamepadToButton();
    }

    public virtual void SetUIScreen(UIScreen screen)
    {
        buttonOwnerUIScreen = screen;
        buttonOwnerUIScreen.OnNewScreenActive += BindInput;
        buttonOwnerUIScreen.OnScreenDeactivated += UnbindInput;
    }

    private void HandleButtonSound()
    {
        UIController._instance.PlayUIClick();
    }

    private void InitializeButton()
    {
        button ??= GetComponentInChildren<Button>();

        if (button == null) return;

        button.onClick.AddListener(HandleButtonSound);
    }

    protected virtual void Awake()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        var imagePrompt = GetComponentInChildren<II_ImagePrompt>();
        if(imagePrompt)
            glyphImage = imagePrompt.GetComponent<Image>();

        if (shouldHaveClickableButton)
            InitializeButton();            
    }

    protected virtual void OnDestroy()
    {
        if (buttonOwnerUIScreen != null)
        {
            buttonOwnerUIScreen.OnNewScreenActive -= BindInput;
            buttonOwnerUIScreen.OnScreenDeactivated -= UnbindInput;
        }

    }
}
