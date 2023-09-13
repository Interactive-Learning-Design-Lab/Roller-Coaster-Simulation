using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Draggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    RectTransform rectTransform;
    Canvas canvas;
    public Flag flag = null;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = transform.parent.GetComponent<Canvas>();
    }

    public void OnDrag(PointerEventData _EventData)
    {
        rectTransform.anchoredPosition += _EventData.delta / canvas.scaleFactor;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (flag != null) {
            flag.panelOffset = Camera.main.WorldToScreenPoint(flag.transform.position + Vector3.up * 0.425f) - transform.position;
        }
    }
}
