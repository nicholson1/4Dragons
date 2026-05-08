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

        public bool IsDraggingValid()
        {
            return isDraggingValid;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDraggingValid = false;

            if(draggableListener.CanBeginDrag())
            {
                isDraggingValid = true;
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
                
            dropReceiver = null;

            if (WasDropSuccess(eventData, out dropReceiver))
            {
                if(dropReceiver != null)
                {
                    GameObject dropReceiverGO = (dropReceiver as Component).gameObject;
                    Debug.LogError($"{dropReceiverGO.name}: wasDropSuccessOnDestination()? {dropReceiver.WasDropSuccessOnDestination()}");
                    draggableListener.EndDrag(this.gameObject, dropReceiverGO, dropReceiver.WasDropSuccessOnDestination());
                    return;
                }

                Debug.LogError($"dropReceiver NULL!");
            }

            Debug.LogError($"OnEndDrag no droppable listener, therefore, OnCancelPerformed");
            draggableListener.OnCancelPerformed();
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

            if (receiverCandidate.Contains(this.gameObject))
                receiverCandidate.Remove(this.gameObject);


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

        protected override void Start()
        {
            base.Start();
            draggableListener = buttonListener as IButtonDraggableListener;
        }

    }
}

