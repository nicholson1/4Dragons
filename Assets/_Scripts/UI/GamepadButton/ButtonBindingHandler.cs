using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InputIcons;
using System;
using DFG.UIHandling;

public class ButtonBindingHandler : SelectableBindingHandler
{
    public bool ignoreButtonSound = false;
    protected Button button = null;

    [SerializeField] protected ExtraButton extraButtonToUse = ExtraButton.None;
    [SerializeField] protected bool clickableWithYes = true;

    [SerializeField] protected bool shouldHaveClickableButton = true;

    private Image glyphImage = null;


    [ContextMenu("Debug Manual Bind")]
    public void DebugManualBind()
    {
        ManualBindInput(true);
    }

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
        {

            //FINALIZE THIS!!
            Debug.Log($"Finalize This!!");
            if (button.TryGetComponent(out ButtonExtender extender))
            {
                extender.ClickButton(InputSource.Gamepad);
            }
            else                
                button.onClick.Invoke();
        }
            
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

        //FINALIZE THIS!!
        Debug.Log($"Finalize This!!");
        if (button.TryGetComponent(out ButtonExtender extender))
        {
            extender.ClickButton(InputSource.Gamepad);
        }
        else
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
            case ExtraButton.L2:
                inputHandler.OnL2.AddListener(ClickThroughInput);
                break;
            case ExtraButton.R2:
                inputHandler.OnR2.AddListener(ClickThroughInput);
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
                inputHandler.OnR1.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.L2:
                inputHandler.OnL2.RemoveListener(ClickThroughInput);
                break;
            case ExtraButton.R2:
                inputHandler.OnR2.RemoveListener(ClickThroughInput);
                break;
        }

        if (glyphImage != null)
            glyphImage.enabled = false;
    }

    protected override void BindInput(UIScreen screen, bool navigatable)
    {

        UnbindGamepadFromButton();

        if (navigatable)
            BindGamepadToButton();
    }

    protected override void UnbindInput(UIScreen _)
    {
        UnbindGamepadFromButton();        
    }

    /// <summary>
    /// for non UIScreen panel buttons that requires runtime binding/unbinding    
    /// </summary>
    public void ManualBindInput(bool toBind)
    {
        UnbindGamepadFromButton();

        if(toBind)
            BindGamepadToButton();
    }



    private void HandleButtonSound()
    {
        if (ignoreButtonSound) return;

        UIController._instance.PlayUIClick();
    }

    private void InitializeButton()
    {
        button ??= GetComponentInChildren<Button>();

        if (button == null) return;

        button.onClick.AddListener(HandleButtonSound);
    }

    public override void SetUIScreen(UIScreen screen)
    {
        selectableOwnerUIScreen = screen;
        selectableOwnerUIScreen.OnNewScreenActive += BindInput;
        selectableOwnerUIScreen.OnScreenDeactivated += UnbindInput;
    }

    protected override void Awake()
    {
        base.Awake();

        var imagePrompt = GetComponentInChildren<II_ImagePrompt>();
        if(imagePrompt)
            glyphImage = imagePrompt.GetComponent<Image>();

        if (shouldHaveClickableButton)
            InitializeButton();
    }

    protected virtual void OnDestroy()
    {
        if (selectableOwnerUIScreen != null)
        {
            selectableOwnerUIScreen.OnNewScreenActive -= BindInput;
            selectableOwnerUIScreen.OnScreenDeactivated -= UnbindInput;
        }

    }
}
