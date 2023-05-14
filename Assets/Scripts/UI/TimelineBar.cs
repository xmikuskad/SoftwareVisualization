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
            SingletonManager.Instance.dataManager.DateChangeEvent += OnDateChanged;
            SingletonManager.Instance.dataManager.DateRenderChangedEvent += OnDateRenderChanged;
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
            SingletonManager.Instance.dataManager.DateChangeEvent -= OnDateChanged;
            SingletonManager.Instance.dataManager.DateRenderChangedEvent += OnDateRenderChanged;
        }

        public void OnDateRenderChanged(Pair<long, Pair<DateTime, DateTime>> pair)
        {
            SetHidden(true);
        }

        private void OnDateChanged(long projectId, DateTime date)
        {
            if (this.projectId != projectId)
            {
                this.image.material = originalMaterial;
                return;
            }
        
            if (this.date == date)
            {
                SetHighlighted(true);
            }
            else
            {
                SetHidden(true);
            }
        }
        
        private void OnMappingChanged(Dictionary<long,ColorMapping> colorMappings,Dictionary<long,ShapeMapping> shapeMappings)
        {
            this.highlightMaterial.color = colorMappings[ColorMapping.HIGHLIGHTED.id].color;
            this.hiddenMaterial.color = colorMappings[ColorMapping.HIDDEN.id].color;
        }

        private void OnResetEvent(ResetEventReason reason)
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
        
        private void OnVerticeSelected(List<Pair<VerticeData,VerticeWrapper>> list)
        {
            foreach (var val in list)
            {
                if ((val.Left != null && val.Left.HasDateWithoutHours(this.date)) || (val.Left == null && val.Right.ContainsDate(this.date)))
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
                // Vector3 mousePos = Input.mousePosition;
                // mousePos.z = -1;
                // tooltipObject.transform.position = mousePos;
                // tooltipObject.transform.position = this.transform.position+this.G;
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