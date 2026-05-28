using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DFG.UIHandling;

public class CombatButtonExtender : ButtonExtender
{
    [SerializeField] private int actionIndex = 0;

    public void ClickButton(int index, InputSource source)
    {
        if(index == actionIndex)
        {
            ClickButton(source);
        }
    }

    public override void ClickButton(InputSource source)
    {
        if (!wasPointerUpEvent)
            button.onClick.Invoke();

        wasPointerUpEvent = false;
        buttonListener?.OnButtonPressed(button, source);
    }
}
