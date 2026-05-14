using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DFG.UIHandling
{
    public class SelectableExtender : MonoBehaviour, ISelectHandler, IDeselectHandler,                                                
                                                IPointerEnterHandler, IPointerExitHandler
    {
        protected Selectable selectable;
        protected ISelectableListener selectableListener;
        protected bool hadPointerDownEvent = false;
        protected bool wasPointerUpEvent = false;

        protected bool wasPointerInside = false;
        private const float moveDeltaTreshold = 0.1f;

        public virtual void OnSelect(BaseEventData eventData)
        {
            selectableListener?.OnSelectableSelected(selectable);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            selectableListener?.OnSelectableDeselected(selectable);
        }


        public virtual void OnPointerExit(PointerEventData eventData)
        {
            bool aboveTreshold = eventData.delta.sqrMagnitude >= moveDeltaTreshold;

            if (!hadPointerDownEvent)
            {
                if ((!wasPointerInside || aboveTreshold) && EventSystem.current.currentSelectedGameObject == gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            wasPointerInside = false;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.dragging) return;

            wasPointerInside = true;
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        //public virtual void OnCancel(BaseEventData eventData)
        //{
        //    Debug.LogError($"Mouse cancel performed");
        //    buttonListener.OnCancelPerformed();
        //}


        protected virtual void Start()
        {
            selectable = GetComponent<Selectable>();
            selectableListener = GetComponentInParent<ISelectableListener>();
        }
    }
}

