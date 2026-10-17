using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zak.UISystem
{
    public interface IDragListener
    {
        /// <summary>
        /// Requirement for a valid drag
        /// </summary>
        /// <returns></returns>
        bool CanBeginDrag(out IDraggablePayload payload);
        void OnDragCompleted(DragResult result);
        void OnHandleInterruption();
    }

    public interface IDropListener
    {
        bool CanAcceptDrop(IDraggablePayload payload);

    
    }

    public interface IDraggablePayload
    {
        GameObject sourceObject { get; }
    }

    public interface IDragDropTransferController
    {
        DragResult TryTransfer(IDraggablePayload payload, IDropListener dropListener);
    }

    public readonly struct DragResult
    {
        public bool Success { get; }
        public IDraggablePayload Payload { get; }
        public IDropListener DropDestination { get; }        

        public DragResult(bool success, IDraggablePayload payload, IDropListener destination)
        {
            Success = success;
            Payload = payload;
            DropDestination = destination;
        }
    }
}

