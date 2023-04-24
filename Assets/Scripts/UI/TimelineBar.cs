using System;
using System.Collections.Generic;
using Renderers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class TimelineBar: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        public TMP_Text tooltipObject;
        public DateTime date;
        public Material originalMaterial;
        public Material highlightMaterial;
        public Material hiddenMaterial;
        public Image image;

        public TimelineRenderer timelineRenderer;

        private void Awake()
        {
            this.image = GetComponent<Image>();
        }

        private void Start()
        {
            SingletonManager.Instance.preferencesManager.MappingChangedEvent += OnMappingChanged;
            this.highlightMaterial.color = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.HIGHLIGHTED).color;
            this.hiddenMaterial.color = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.HIDDEN).color;
        }

        private void OnMappingChanged(Dictionary<long,ColorMapping> colorMappings)
        {
            // TODO
            this.highlightMaterial.color = colorMappings[ColorMapping.HIGHLIGHTED.id].color;
            this.hiddenMaterial.color = colorMappings[ColorMapping.HIDDEN.id].color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipObject!= null)
            {
                tooltipObject.gameObject.SetActive(true);
                Vector3 mousePos = Input.mousePosition;
                tooltipObject.transform.position = mousePos;
                // tooltipObject.transform.position = this.transform.position;
                tooltipObject.text = date.ToString("dd/MM/yyyy");
            }
        }
 
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipObject!= null)
            {
                tooltipObject.gameObject.SetActive(false);
            }
        }

        public void SetUp(DateTime date, TMP_Text tooltipObject, Material originalMaterial, Material highlightMaterial, Material hiddenMaterial, TimelineRenderer timelineRenderer)
        {
            this.date = date;
            this.tooltipObject = tooltipObject;
            this.originalMaterial = originalMaterial;
            this.highlightMaterial = new Material(highlightMaterial);
            this.hiddenMaterial = new Material(hiddenMaterial);
            this.timelineRenderer = timelineRenderer;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            this.timelineRenderer.SelectDate(this.date);
        }
        
        public void OnPointerDown(PointerEventData eventData)   // This has to exists otherwise OnPointerUp is not called!
        {
        }

        public void SetHighlighted(bool isHighlighted)
        {
            this.image.material = isHighlighted ? highlightMaterial : originalMaterial;
        }
        
        public void SetHidden(bool isHidden)
        {
            this.image.material = isHidden ? hiddenMaterial : originalMaterial;
        }
    }
}