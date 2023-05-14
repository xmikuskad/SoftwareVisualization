using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class MappingController : MonoBehaviour
    {
        private Dictionary<long, ColorMapping> tempColorMappings = new ();
        private Dictionary<long, ShapeMapping> tempShapeMappings = new ();
        private Dictionary<long, Image> colorMappingImgs = new();

        private long selectedMapping = -1;

        [Header("References")] public GameObject dialog;
        public GameObject colorListItem;
        public GameObject colorListItemHolder;
        public ColorPicker colorPicker;
        
        public GameObject shapeListItem;
        public GameObject shapeListItemHolder;
        
        public GameObject shapeChoiceListItem;
        public GameObject shapeChoiceListItemHolder;

        private List<GameObject> shapeChoices = new();

        public void OpenDialog()
        {
            ResetObjects();
            tempColorMappings = SingletonManager.Instance.preferencesManager.GetColorMappings();
            tempShapeMappings = SingletonManager.Instance.preferencesManager.GetShapeMappings();

            foreach (var colorMapping in tempColorMappings.Values)
            {
                ColorMapping tmp = colorMapping;
                GameObject go = Instantiate(colorListItem, colorListItemHolder.transform);
                Image img = go.GetComponentInChildren<Image>();
                img.color = colorMapping.color;
                colorMappingImgs[colorMapping.id] = img;
                go.GetComponentInChildren<TMP_Text>().text = colorMapping.name;
                go.GetComponentInChildren<Button>().onClick.AddListener(() =>SelectMapping(tmp));
            }
            
            foreach (var shapeMapping in tempShapeMappings.Values)
            {
                ShapeMapping tmp = shapeMapping;
                GameObject go = Instantiate(shapeListItem, shapeListItemHolder.transform);
                go.GetComponentInChildren<TMP_Text>().text = shapeMapping.name;
                go.GetComponentInChildren<Button>().onClick.AddListener(() =>SelectMapping(tmp));
            }
            
            foreach (VerticeShape s in (VerticeShape[]) Enum.GetValues(typeof(VerticeShape)))
            {
                VerticeShape _s = s;
                GameObject go = Instantiate(shapeChoiceListItem, shapeChoiceListItemHolder.transform);
                shapeChoices.Add(go);
                go.GetComponentInChildren<TMP_Text>().text = s.ToString();
                go.GetComponentInChildren<Image>().enabled = false;
                go.GetComponentInChildren<Button>().onClick.AddListener(() =>SelectMapping(s));
            }
            
            // This actually needs to be duplicated!!
            dialog.SetActive(true);
            dialog.SetActive(true);
        }

        public void SaveDialog()
        {
            SingletonManager.Instance.preferencesManager.SetMappings(tempColorMappings.Values.ToList(), tempShapeMappings.Values.ToList());
            colorPicker.gameObject.SetActive(false);
            shapeChoiceListItemHolder.gameObject.SetActive(false);
            dialog.SetActive(false);
        }
        
        // Called from ColorPicker
        public void ColorChanged(Color32 color)
        {
            if (selectedMapping < 0) return;
            tempColorMappings[selectedMapping].color = color;
            colorMappingImgs[selectedMapping].color = color;
        }

        public void SelectMapping(ColorMapping colorMapping)
        {
            this.selectedMapping = colorMapping.id;
            colorPicker.Color = colorMapping.color;
            colorPicker.gameObject.SetActive(true);
        }
        
        public void SelectMapping(ShapeMapping shapeMapping)
        {
            this.selectedMapping = shapeMapping.id;
            shapeChoiceListItemHolder.gameObject.SetActive(true);
            
            foreach (int i in Enum.GetValues(typeof(VerticeShape)))
            {
                ShapeMapping _s = shapeMapping;
                if (i == (int)_s.shape)
                {
                    shapeChoices[i].GetComponentInChildren<Image>().enabled = true;
                }
                else
                {
                    shapeChoices[i].GetComponentInChildren<Image>().enabled = false;
                }
            }
        }

        public void SelectMapping(VerticeShape s)
        {
            this.tempShapeMappings[selectedMapping].shape = s;
            
            foreach (int i in Enum.GetValues(typeof(VerticeShape)))
            {
                ShapeMapping _s = tempShapeMappings[selectedMapping];
                if (i == (int)_s.shape)
                {
                    shapeChoices[i].GetComponentInChildren<Image>().enabled = true;
                }
                else
                {
                    shapeChoices[i].GetComponentInChildren<Image>().enabled = false;
                }
            }
        }

        private void ResetObjects()
        {
            this.selectedMapping = -1;
            foreach (Transform child in colorListItemHolder.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in shapeListItemHolder.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in shapeChoiceListItemHolder.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (GameObject child in shapeChoices)
            {
                Destroy(child);
            }
            this.colorMappingImgs.Clear();
            this.tempColorMappings.Clear();
            this.tempShapeMappings.Clear();
            this.shapeChoices.Clear();
        }

        public void ResetMappingId()
        {
            this.selectedMapping = -1;
        }
    }
}