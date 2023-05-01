using System;
using System.Collections.Generic;
using Data;
using Helpers;
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
        public long projectId;
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
            SingletonManager.Instance.dataManager.ResetEvent += OnResetEvent;
            SingletonManager.Instance.dataManager.DatesSelectedEvent += OnDatesSelected;
            SingletonManager.Instance.dataManager.DatesRangeSelectedEvent += OnDatesRangeSelectedSelected;
            SingletonManager.Instance.dataManager.VerticesSelectedEvent += OnVerticeSelected;
            this.highlightMaterial.color = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.HIGHLIGHTED).color;
            this.hiddenMaterial.color = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.HIDDEN).color;
        }

        private void OnDestroy()
        {
            SingletonManager.Instance.preferencesManager.MappingChangedEvent -= OnMappingChanged;
            SingletonManager.Instance.dataManager.ResetEvent -= OnResetEvent;
            SingletonManager.Instance.dataManager.DatesSelectedEvent -= OnDatesSelected;
            SingletonManager.Instance.dataManager.DatesRangeSelectedEvent -= OnDatesRangeSelectedSelected;
            SingletonManager.Instance.dataManager.VerticesSelectedEvent -= OnVerticeSelected;
        }

        private void OnMappingChanged(Dictionary<long,ColorMapping> colorMappings)
        {
            // TODO
            this.highlightMaterial.color = colorMappings[ColorMapping.HIGHLIGHTED.id].color;
            this.hiddenMaterial.color = colorMappings[ColorMapping.HIDDEN.id].color;
        }

        private void OnResetEvent()
        {
            this.image.material = originalMaterial;
        }

        private void OnDatesSelected(Pair<long, List<DateTime>> pair)
        {
            if (pair.Left != projectId)
            {
                this.image.material = originalMaterial;
                return;
            }

            if (pair.Right.Contains(date))
            {
                SetHighlighted(true);
            }
            else
            {
                SetHidden(true);
            }
        }

        private void OnDatesRangeSelectedSelected(Pair<long, List<DateTime>> pair)
        {
            if (pair.Left != projectId)
            {
                this.image.material = originalMaterial;
                return;
            }

            if (date >= pair.Right[0] && date <= pair.Right[1])
            {
                SetHighlighted(true);
            }
            else
            {
                SetHidden(true);
            }
        }
        
        private void OnVerticeSelected(Pair<long, List<VerticeWrapper>> pair)
        {
            if (pair.Left != projectId)
            {
                SetHighlighted(false);
                return;
            }

            foreach (var verticeWrapper in pair.Right)
            {
                if (verticeWrapper.ContainsDate(this.date))
                {
                    SetHighlighted(true);
                    return;
                }
            }
            
            SetHidden(true);
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

        public void SetUp(DateTime date, TMP_Text tooltipObject, Material originalMaterial, Material highlightMaterial, Material hiddenMaterial, TimelineRenderer timelineRenderer, long projectId)
        {
            this.date = date;
            this.tooltipObject = tooltipObject;
            this.originalMaterial = originalMaterial;
            this.highlightMaterial = new Material(highlightMaterial);
            this.hiddenMaterial = new Material(hiddenMaterial);
            this.timelineRenderer = timelineRenderer;
            this.projectId = projectId;
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