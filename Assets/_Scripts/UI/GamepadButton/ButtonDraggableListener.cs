using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DFG.UIHandling
{
    public class ButtonDraggableListener : ButtonListener, IButtonDraggableListener, IDroppableListener
    {
        protected bool wasDropSuccess = false;
        protected GameObject destination = null;
        [SerializeField] protected bool shouldWaitForConfirmation = false;


        public virtual void FinalizeDragDrop(bool result, GameObject origin, GameObject destination)
        {
            throw new NotImplementedException("Consider regular button listener if you're not implementing OnDropSucess!");
        }

        #region Interface Implementations
        public virtual bool CanBeginDrag() 
        {
            throw new NotImplementedException("Define drag eligibility or else the drag won't be valid!");

        }

        public virtual void BeginDrag(Button button, InputSource source)
        {
            throw new NotImplementedException("Consider regular button listener if you're not implementing OnDropSucess!");
        }

        public void EndDrag(GameObject origin, GameObject destination, bool wasDropSuccess)
        {
            string originName = origin != null ? origin.name : "origin NULL";
            string destinationName = destination != null ? destination.name : "destionation NULL"; 
            Debug.LogError($"at end drag before Finalize: wasDropSuccess? {wasDropSuccess}. from origin: {originName} to destination: {destinationName}");
            FinalizeDragDrop(wasDropSuccess, origin, destination);
            //if (shouldWaitForConfirmation)
            //{

            //}
            //else
                
        }

        public virtual void OnDrag(Vector2 dragPosition)
        {
            throw new NotImplementedException();
        }
         
        //IDropHandler part of IDroppableListener
        public void OnDrop(PointerEventData eventData)
        {
            var dragOriginObj = eventData.pointerDrag;
            if(dragOriginObj.TryGetComponent(out DraggableButtonExtender buttonExtender) && buttonExtender.IsDraggingValid())
            {
                Debug.LogError($"OnDrop draggedObj: {dragOriginObj.gameObject}");
                ProcessDrop(eventData.position, dragOriginObj);
            }

            var selectable = GetComponentInChildren<Selectable>();
            if (selectable != null)
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        protected virtual bool GetDropResult(GameObject origin)
        {
            throw new NotImplementedException("Consider regular button listener if you're not implementing OnDropSucess!");
        }

        public bool WasDropSuccessOnDestination() => wasDropSuccess;
        #endregion

        public void ProcessDrop(Vector2 dropPosition, GameObject origin)
        {
            Debug.LogError($"ProcessDrop() start!!");
            destination = null;
            Debug.LogError($"Process Drop ButtonDraggableListener");
            wasDropSuccess = false;
            if (origin == null || origin == this.gameObject)
            {
                Debug.LogError($"Process Drop: origin is null or this!");
                return;
            }

            //do the implementation to determine if the drop is success on this IDroppableListener
            wasDropSuccess = GetDropResult(origin);

            Debug.LogError($"wasDropSuccess at the end of ProcessDrop? {wasDropSuccess}");
            SetDestinationAtOrigin(origin);
        }

        private void SetDestinationAtOrigin(GameObject origin)
        {
            ButtonDraggableListener originDroppableListener = origin.GetComponentInParent<ButtonDraggableListener>();
            if(originDroppableListener == null)
            {
                Debug.LogError($"Error - IDroppableListener can't be found at the origin!");
                return;
            }

            originDroppableListener.destination = wasDropSuccess ? this.gameObject : origin;
        }
    }
}

