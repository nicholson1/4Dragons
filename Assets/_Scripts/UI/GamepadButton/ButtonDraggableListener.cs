using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zak.UISystem;

namespace DFG.UIHandling
{
    public class ButtonDraggableListener : ButtonListener, IButtonDraggableListener, IDragListener, IDropListener
    {
        protected bool wasDropSuccessCache = false;
        protected GameObject destinationCache = null;
        [SerializeField] protected bool shouldWaitForConfirmation = false;


        public virtual void FinalizeDragDrop(bool result, GameObject origin, GameObject destination)
        {
            throw new NotImplementedException("Consider regular button listener if you're not implementing OnDropSucess!");
        }

        #region Interface Implementations
        public bool WasDraggingValid { get; private set; } = false;

        public void SetDraggingValid(bool isValid)
        {
            WasDraggingValid = isValid;
        }


        public virtual bool CanBeginDrag() 
        {
            throw new NotImplementedException("Define drag eligibility or else the drag won't be valid!");
        }

        public void CleanupDragDropCache()
        {
            wasDropSuccessCache = false;
            destinationCache = null;
        }

        public virtual void BeginDrag(Button button, InputSource source)
        {
            throw new NotImplementedException("Consider regular button listener if you're not implementing OnDropSucess!");
        }

        public virtual void EndDrag(GameObject origin, GameObject destination, bool dropSuccess)
        {
            string originName = origin != null ? origin.name : "origin NULL";
            string destinationName = destination != null ? destination.name : "destionation NULL"; 
            Debug.LogError($"at end drag before Finalize: wasDropSuccess? {dropSuccess}. from origin: {originName} to destination: {destinationName}");
            FinalizeDragDrop(dropSuccess, origin, destination);                          
        }

        public virtual void OnDrag(Vector2 dragPosition)
        {
            throw new NotImplementedException();
        }
         

        //IDropHandler part of IDroppableListener
        public void OnDrop(PointerEventData eventData)
        {
            wasDropSuccessCache = false;
            destinationCache = null;
            Debug.LogError($"OnDrop at {this.gameObject.name}");
            var dragEventDataObj = eventData.pointerDrag;
            Debug.LogError($"OnDrop: obj from eventData.pointerDrag: {dragEventDataObj.name}");
            IButtonDraggableListener originDragListener = dragEventDataObj.GetComponentInParent<IButtonDraggableListener>();
            string originDragListenerName = originDragListener != null ? (originDragListener as Component).gameObject.name : "NONE";
            Debug.LogError($"OnDrop: originDragListenerName = {originDragListenerName}");

            if(originDragListener != null && originDragListener.WasDraggingValid)
            {
                GameObject originDragListenerObj = (originDragListener as Component).gameObject;

                Debug.LogError($"OnDrop - this: {this.gameObject} || objfromEventData: {originDragListenerObj.name}");
                ProcessDrop(eventData.position, originDragListenerObj);
            }

            Debug.LogError($"OnDrop - couldnt find the originDragListener");
            var selectable = GetComponentInChildren<Selectable>();
            if (selectable != null)
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        protected virtual bool GetDropResult(GameObject origin)
        {
            throw new NotImplementedException("Consider regular button listener if you're not implementing OnDropSucess!");
        }

        public bool WasDropSuccessOnDestination() => wasDropSuccessCache;
        #endregion

        public void ProcessDrop(Vector2 dropPosition, GameObject origin)
        {
            destinationCache = null;

            if (origin == null) return;

            //if (origin == this.gameObject) return;

            //do the implementation to determine if the drop is success on this IDroppableListener
            wasDropSuccessCache = GetDropResult(origin);
            SetDestinationAtOrigin(origin);
        }

        private void SetDestinationAtOrigin(GameObject origin)
        {
            ButtonDraggableListener originDroppableListener = origin.GetComponent<ButtonDraggableListener>();
            if(originDroppableListener == null)
            {
                return;
            }

            originDroppableListener.destinationCache = wasDropSuccessCache ? this.gameObject : origin;
        }

        public bool CanBeginDrag(out IDraggablePayload payload)
        {
            throw new NotImplementedException();
        }

        public void OnDragCompleted(DragResult result)
        {
            throw new NotImplementedException();
        }

        public bool CanAcceptDrop(IDraggablePayload payload)
        {
            throw new NotImplementedException();
        }

        public void OnDropReceived(IDraggablePayload payload)
        {
            throw new NotImplementedException();
        }

        public void OnHandleInterruption()
        {
            throw new NotImplementedException();
        }
    }
}

