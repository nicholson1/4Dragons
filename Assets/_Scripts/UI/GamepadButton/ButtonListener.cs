using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DFG.UIHandling
{
    public abstract class ButtonListener : MonoBehaviour, IButtonListener
    {
        public virtual event Action OnGamepadButtonSelected;
        public virtual event Action OnGamepadButtonDeselected;

        //public virtual void OnCancelPerformed()
        //{
        //    throw new NotImplementedException();
        //}

        public abstract void OnButtonDeselected(Selectable selectable);

        public abstract void OnButtonPressed(Selectable selectable, InputSource source);

        public abstract void OnButtonSelected(Selectable selectable);


    }
}

