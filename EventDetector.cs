using UnityEngine.EventSystems;
using UnityEngine;

public class EventDetector : MonoBehaviour, IPointerDownHandler, IPointerClickHandler,
    IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("Drag Begin");
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("Dragging");
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("Drag Ended");
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
        pointer_clicked(eventData);
    }

    protected void pointer_clicked(PointerEventData eventData)
    {
        print("protected click");
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Mouse Down: " + eventData.pointerCurrentRaycast.gameObject.name);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Mouse Enter" + eventData.pointerCurrentRaycast.gameObject.name);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Mouse Exit");
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Mouse Up");
    }
}