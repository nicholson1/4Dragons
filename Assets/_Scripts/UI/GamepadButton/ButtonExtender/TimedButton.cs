using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimedButton : Selectable
{
    public UnityEvent onClick;

    [SerializeField] private Image fillBar;

    private InputHandler inputHandler;

    private Coroutine timerRoutine = null;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        StartTimerRoutine();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        StopTimerRoutine();
    }

    private void ClickButton()
    {
        StopTimerRoutine();
        onClick?.Invoke();
    }

    private void StartTimerRoutine()
    {
        if(timerRoutine != null)
        {
            StopTimerRoutine();
        }

        fillBar.fillAmount = 0f;
        timerRoutine = StartCoroutine(TimerRoutine());
    }

    private void StopTimerRoutine()
    {
        if(timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        fillBar.fillAmount = 0f;

    }

    private IEnumerator TimerRoutine()
    {
        while (fillBar.fillAmount < 1f)
        {
            fillBar.fillAmount += Time.deltaTime;
            yield return null;
        }

        ClickButton();
    }

    private void HandleYesPressed()
    {
        StartTimerRoutine();
    }

    private void HandleYesCanceled()
    {
        StopTimerRoutine();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if(EventSystem.current != null)
        {
            inputHandler ??= EventSystem.current.GetComponentInChildren<InputHandler>();
        }

        if(inputHandler != null)
        {
            inputHandler.OnYes.AddListener(HandleYesPressed);
            inputHandler.OnYesCanceled.AddListener(HandleYesCanceled);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if(inputHandler != null)
        {
            inputHandler.OnYes.RemoveListener(HandleYesPressed);
            inputHandler.OnYesCanceled.RemoveListener(HandleYesCanceled);
        }
    }
}
