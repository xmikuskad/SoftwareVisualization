using System.Collections.Generic;
using System.Linq;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class MappingController : MonoBehaviour
    {
        private Dictionary<long, ColorMapping> tempColorMappings = new ();
        private Dictionary<long, Image> colorMappingImgs = new();

        private long selectedMapping = -1;

        [Header("References")] public GameObject dialog;
        public GameObject colorListItem;
        public GameObject colorListItemHolder;
        public ColorPicker colorPicker;

        public void OpenDialog()
        {
            ResetObjects();
            tempColorMappings = SingletonManager.Instance.preferencesManager.GetMappings();

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
            
            // This actually needs to be duplicated!!
            dialog.SetActive(true);
            dialog.SetActive(true);
        }

        public void SaveDialog()
        {
            SingletonManager.Instance.preferencesManager.SetColorMappings(tempColorMappings.Values.ToList());
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
        }

        private void ResetObjects()
        {
            this.selectedMapping = -1;
            foreach (Transform child in colorListItemHolder.transform)
            {
                Destroy(child.gameObject);
            }
            this.colorMappingImgs.Clear();
            this.tempColorMappings.Clear();
        }
    }
}