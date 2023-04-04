using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipObject;
 
    void Start()
    {
        if (tooltipObject!= null)
        {
            tooltipObject.SetActive(false);
        }
    }
 
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject!= null)
        {
            tooltipObject.SetActive(true);
        }
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipObject!= null)
        {
            tooltipObject.SetActive(false);
        }
    }
}
