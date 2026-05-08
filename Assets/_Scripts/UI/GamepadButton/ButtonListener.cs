using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DFG.UIHandling
{
    public class ButtonListener : MonoBehaviour, IButtonListener
    {
        public virtual event Action OnGamepadButtonSelected;
        public virtual event Action OnGamepadButtonDeselected;

        public virtual void OnCancelPerformed()
        {
            throw new NotImplementedException();
        }

        public virtual void OnButtonDeselected(Selectable selectable)
        {
            throw new NotImplementedException();
        }

        public virtual void OnButtonPressed(Selectable selectable, InputSource source)
        {
            throw new NotImplementedException();
        }

        public virtual void OnButtonSelected(Selectable selectable)
        {
            throw new NotImplementedException();
        }

    }
}

