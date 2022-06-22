using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unidice.SDK.UI
{
    public class CursorNotifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private CursorType cursor;
        [SerializeField] private CursorType cursorHold;
        public readonly CursorChangedEvent onEnter = new CursorChangedEvent();
        public readonly CursorChangedEvent onExit = new CursorChangedEvent();
        public readonly CursorChangedEvent onHold = new CursorChangedEvent();
        private bool _dragging;

        private bool _inside;

        // If a screen gets enabled or similar, reset the cursor
        public void OnEnable()
        {
            // But how? Should check if we're inside; otherwise notifier objects anywhere in the scene will reset the cursor...
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _dragging = true;
            onHold.Invoke(cursorHold);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.pointerEnter || !eventData.pointerEnter.GetComponent<Selectable>()) return;
            _inside = true;
            if (!_dragging) onEnter.Invoke(cursor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.pointerEnter || !eventData.pointerEnter.GetComponent<Selectable>()) return;
            _inside = false;
            if (!_dragging) onExit.Invoke(CursorType.Normal);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
            if (_inside) OnPointerEnter(eventData);
            else OnPointerExit(eventData);
        }
    }

    public enum CursorType
    {
        Normal,
        Highlight,
        Drag,
        DragHighlight
    }

    public class CursorChangedEvent : UnityEvent<CursorType> { }
}