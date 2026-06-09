using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.UI;

namespace Zak.UISystem
{
    public class DragDriver : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, ICancelHandler
    {
        private InputHandler inputHandler = null;

        [SerializeField] MonoBehaviour dragListenerSource;

        IDragListener dragListener;

        private bool isDragging = false;

        IDraggablePayload dragPayload;
        private bool wasDropSuccess = false;
        private IDropListener dropDestination;

        public IDraggablePayload GetPayload() => dragPayload;

        public void SetDropResult(bool wasSuccess, IDropListener destination)
        {
            var destinationObj = destination as InventorySlot;
            wasDropSuccess = wasSuccess;
            dropDestination = destination;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragPayload = null;
            isDragging = dragListener.CanBeginDrag(out dragPayload);
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (!isDragging) return;

            var dragResult = new DragResult(false, null, null);
            dragListener.OnDragCompleted(dragResult);
            CleanupDrag();
            isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || dragPayload == null) return;

            (dragPayload.sourceObject.transform as RectTransform).position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging || dragPayload == null)
            {
                return;
            }

            var dragResult = new DragResult(wasDropSuccess, dragPayload, dropDestination);
            dragListener.OnDragCompleted(dragResult);

            CleanupDrag();
            isDragging = false;
        }

        private void HandleMouseInterruptingGamepad()
        {
            dragListener.OnHandleInterruption();
        }

        private void CleanupDrag()
        {
            isDragging = false;
            dragPayload = null;
            dropDestination = null;
            wasDropSuccess = false;
        }


        protected void Awake()
        {
            ResolveListener();
            inputHandler = EventSystem.current.GetComponent<InputHandler>();

            inputHandler.OnInputTypeChange += HandleInputTypeChange;
        }

        protected void OnDestroy()
        {
            inputHandler.OnInputTypeChange -= HandleInputTypeChange;
        }

        private void HandleInputTypeChange(InputSource newSource)
        {
            if (newSource == InputSource.MouseKeyboard)
            {
                HandleMouseInterruptingGamepad();
            }
            else if(newSource == InputSource.Gamepad)
                OnCancel(null);
        }

        private void ResolveListener()
        {
            if (dragListenerSource != null)
            {
                dragListener = dragListenerSource as IDragListener;

                if (dragListener == null)
                    Debug.LogWarning($"{dragListenerSource.name} does not implement {nameof(IDragListener)}", dragListenerSource);
            }

            dragListener ??= GetComponentInParent<IDragListener>();
        }
    }


}

