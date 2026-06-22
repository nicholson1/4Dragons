using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DFG.UIHandling;
using Zak.UISystem;
using UnityEngine.EventSystems;

public class TargettingButtonListener : ButtonListener, IDropListener
{
    public Button GamepadButton => button;

    public CombatEntity Entity => combatEntity;


    //public Button GamepadButton => button;
    public Image TargetImage => targetImage;

    private Button button;
    private Image targetImage;
    private CombatEntity combatEntity;



    private void InitiateGamepadProcessDrop(IDraggablePayload payload, IButtonListener origin)
    {
        var originSlot = origin as PotionHolder;
        originSlot.EndGamepadDragRoutine();

        var selected = EventSystem.current.currentSelectedGameObject;
        
        if(CanAcceptDrop(payload))
        {
            var dragResult = new DragResult(true, payload, this as IDropListener);
            originSlot.OnDragCompleted(dragResult);
        }
        else
        {
            var dragResult = new DragResult(false, payload, this as IDropListener);
            originSlot.OnDragCompleted(dragResult);
        }
    }

    public override void OnButtonDeselected(Selectable selectable)
    {
        
    }

    public override void OnButtonPressed(Selectable selectable, InputSource source)
    {
        if (source == InputSource.Gamepad)
        {
            if (UIController._instance.StateMonitor.TryGetItemOnGamepad(out IDraggablePayload itemOnGamepad) && itemOnGamepad is PotionDrag)
            {
                var item = itemOnGamepad as PotionDrag;
                InitiateGamepadProcessDrop(itemOnGamepad, item.currentLocation);
            }
        }
    }

    public override void OnButtonSelected(Selectable selectable)
    {
        Debug.Log($"targetting button selected");
    }

    public void InitializeButton(PotionDrag potion)
    {
        combatEntity = GetComponentInParent<HealthBar>().displayCharacter._combatEntity;

        targetImage.sprite = potion.PotionImage.sprite;
    }

    public void HandleCancelPerformed()
    {

    }

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        targetImage = GetComponent<Image>();
        targetImage.enabled = false;
    }

    public bool CanAcceptDrop(IDraggablePayload payload)
    {
        var potionDrag = payload as PotionDrag;

        if (potionDrag == null)
            return false;

        Debug.LogError($"can Accept Drop!");
        return true;
    }


}
