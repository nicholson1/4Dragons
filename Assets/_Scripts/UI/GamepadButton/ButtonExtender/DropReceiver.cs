using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zak.UISystem
{
    public class DropReceiver : MonoBehaviour, IDropHandler
    {
        [SerializeField] private MonoBehaviour dropListenerSource;
        private IDropListener dropListener;

        public void OnDrop(PointerEventData eventData)
        {
            if (dropListener == null) return;
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent(out DragDriver dragDriver)) return;

            IDraggablePayload payload = dragDriver.GetPayload();
            if (payload == null) return;

            if (!dropListener.CanAcceptDrop(payload))
            {
                dragDriver.SetDropResult(false, dropListener);
                return;
            }

            dragDriver.SetDropResult(true, dropListener);
        }

        private void Awake()
        {
            ResolveListener();
        }
        private void ResolveListener()
        {
            if (dropListenerSource != null)
            {
                dropListener = dropListenerSource as IDropListener;

                if (dropListener == null)
                    Debug.LogWarning($"{dropListenerSource.name} does not implement {nameof(IDropListener)}", dropListenerSource);
            }

            dropListener ??= GetComponentInParent<IDropListener>();
        }
    }
}

