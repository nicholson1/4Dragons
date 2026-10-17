using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DFG.UIHandling
{
    public class DraggableButtonExtender : ButtonExtender, ISelectHandler, IDeselectHandler,
                                                IPointerDownHandler, IPointerUpHandler,
                                                IPointerEnterHandler, IPointerExitHandler,
                                                IBeginDragHandler, IEndDragHandler, IDragHandler

    {
        public IButtonDraggableListener DraggableListener => draggableListener;
        private IButtonDraggableListener draggableListener;
        private IDroppableListener dropReceiver;
        protected bool isDraggingValid = false;
        [SerializeField] bool shouldClickAfterDrag = false;

        //public bool IsDraggingValid()
        //{
        //    return isDraggingValid;
        //}

        public void OnBeginDrag(PointerEventData eventData)
        {

            if (draggableListener.CanBeginDrag())
            {
                isDraggingValid = true;
                draggableListener.SetDraggingValid(isDraggingValid);
                draggableListener.CleanupDragDropCache();
                BeginDrag();
            }
        }

        public void BeginDrag()
        {
            if (!isDraggingValid) return;

            draggableListener.BeginDrag(button, InputSource.MouseKeyboard);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggingValid) return;

            isDraggingValid = false;
            draggableListener.SetDraggingValid(isDraggingValid);

            dropReceiver = null;

            if (WasDropSuccess(eventData, out dropReceiver))
            {
                if(dropReceiver != null)
                {
                    GameObject dropReceiverGO = (dropReceiver as Component).gameObject;
                    draggableListener.EndDrag((draggableListener as Component).gameObject, dropReceiverGO, dropReceiver.WasDropSuccessOnDestination());
                    return;
                }
            }

            //draggableListener.OnCancelPerformed();

            if (eventData.pointerCurrentRaycast.gameObject == this.gameObject)
            {
                EvaluateClickButton();
            }            
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDraggingValid)
            {
                draggableListener.OnDrag(eventData.position);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.dragging) return;

            base.OnPointerUp(eventData);
        }
        
        public bool WasDropSuccess(IDroppableListener destinationListener)
        {
            return destinationListener.WasDropSuccessOnDestination();
        }

        private bool WasDropSuccess(PointerEventData eventData, out IDroppableListener destinationListener)
        {
            destinationListener = null;
            var receiverCandidate = eventData.hovered;

            //if (receiverCandidate.Contains(this.gameObject))
            //    receiverCandidate.Remove(this.gameObject);

            if (eventData.hovered.Count <= 0)
                return false;

            foreach (var obj in eventData.hovered)
            {
                destinationListener = obj.GetComponent<IDroppableListener>();
                if (destinationListener == null) continue;

                return destinationListener.WasDropSuccessOnDestination();
            }

            return false;
        }

        private void EvaluateClickButton()
        {
            Debug.LogError($"Evaluate click after drag");
            if (shouldClickAfterDrag)
            {
                wasPointerUpEvent = true;
                ClickButton(InputSource.MouseKeyboard);
            }
        }

        protected override void Start()
        {
            base.Start();
            draggableListener = buttonListener as IButtonDraggableListener;
        }

    }
}

