using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DFG.UIHandling
{
    public class ButtonExtender : MonoBehaviour, ISelectHandler, IDeselectHandler,
                                                IPointerDownHandler, IPointerUpHandler,
                                                IPointerEnterHandler, IPointerExitHandler

    {
        protected Button button;
        protected IButtonListener buttonListener;

        protected bool hadPointerDownEvent = false;
        protected bool wasPointerUpEvent = false;
        protected bool wasPointerInside = false;
        private const float moveDeltaTreshold = 0.1f;

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (hadPointerDownEvent) return;

            buttonListener.OnButtonSelected(button);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            buttonListener.OnButtonDeselected(button);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnCancel(eventData);
                return;
            }

            hadPointerDownEvent = true;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!hadPointerDownEvent)
            {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                return;
            }

            hadPointerDownEvent = false;

            if (wasPointerInside)
            {
                wasPointerUpEvent = true;
                ClickButton(InputSource.MouseKeyboard);
            }
            else
            {
                if (EventSystem.current.currentSelectedGameObject == gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

            }
        }

        public virtual void ClickButton(InputSource source)
        {
            if (!wasPointerUpEvent)
                button.onClick.Invoke();

            wasPointerUpEvent = false;
            buttonListener.OnButtonPressed(button, InputSource.Gamepad);
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
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            Debug.LogError($"Mouse cancel performed");
            buttonListener.OnCancelPerformed();
        }


        protected virtual void Start()
        {
            button = GetComponent<Button>();
            buttonListener = GetComponentInParent<IButtonListener>();
        }
    }
}


