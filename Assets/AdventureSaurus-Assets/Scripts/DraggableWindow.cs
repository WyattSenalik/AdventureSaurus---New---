using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler
{
    // The window that will be moved
    [SerializeField] private RectTransform _windowToDrag = null;

    /// <summary>
    /// Called when a drag happens (ex. mouse moves after being pressed down) over the
    /// image this script is attached to
    /// </summary>
    /// <param name="eventData">PointerEventData</param>
    public void OnDrag(PointerEventData eventData)
    {
        // Drag the window
        try
        {
            _windowToDrag.position += new Vector3(eventData.delta.x, eventData.delta.y);
        }
        catch
        {
            Debug.Log("Error dragging window");
        }
    }
}
