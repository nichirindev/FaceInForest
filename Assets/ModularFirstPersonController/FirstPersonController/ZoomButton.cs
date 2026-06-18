using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public FirstPersonController controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.ZoomIn();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        controller.ZoomOut();
    }
}
