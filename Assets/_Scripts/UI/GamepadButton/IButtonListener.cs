using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DFG.UIHandling
{
    public interface IButtonListener
    {
        public void OnCancelPerformed();
        public void OnButtonPressed(Selectable selectable, InputSource source);
        public void OnButtonSelected(Selectable selectable);
        public void OnButtonDeselected(Selectable selectable);

    }

    public interface IButtonDraggableListener : IButtonListener
    {
        public bool CanBeginDrag();

        /// <summary>
        /// Start of a drag
        /// </summary>
        /// <param name="button"></param>
        /// <param name="source"></param>
        public void BeginDrag(Button button, InputSource source);

        /// <summary>
        /// Handle drag/drop ending on the dropper side, based on the wasDropSuccess
        /// </summary>
        /// <param name="origin">Owner of the component that implements IButtonDraggableListener</param>
        /// <param name="destination">Owner of the component that implement IDraggableListener if the drop was success</param>
        /// <param name="wasDropSuccess"></param>
        public void EndDrag(GameObject origin, GameObject destination, bool wasDropSuccess);

        /// <summary>
        /// If whatever is involved in this drag need to update according to dragPosition
        /// </summary>
        /// <param name="dragPosition"></param>
        public void OnDrag(Vector2 dragPosition);


    }

    public interface IDroppableListener : IDropHandler
    {
        public void ProcessDrop(Vector2 dropPosition, GameObject origin);

        /// <summary>
        /// returns a field on the implementation. the dropper needs this to validate its EndDrag()
        /// </summary>
        public bool WasDropSuccessOnDestination();

    }
}

