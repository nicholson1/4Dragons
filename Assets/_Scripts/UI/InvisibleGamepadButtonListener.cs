using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvisibleGamepadButtonListener : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Button button;
    private IGamepadButtonListener buttonListener;

    public void OnSelect(BaseEventData eventData)
    {
        buttonListener.HandleGamepadButtonSelected();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        buttonListener.HandleGamepadButtonDeselected();
    }

    private void OnClickCallback()
    {
        buttonListener.HandleGamepadButtonPressed();
    }

    private void Start()
    {
        button = GetComponent<Button>();
        buttonListener = GetComponentInParent<IGamepadButtonListener>();
        button.onClick.AddListener(OnClickCallback);
    }
}
