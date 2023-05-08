using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace UI
{
    public class SearchController : MonoBehaviour
    {
        [Header("References")] public GameObject dialog;
        [FormerlySerializedAs("colorListItem")] public GameObject listItem;
        [FormerlySerializedAs("colorListItemHolder")] public GameObject listItemHolder;

        public TMP_InputField inputField;
        public DataRenderer dataRenderer;

        public void OpenDialog()
        {
            // This actually needs to be duplicated!!
            dialog.SetActive(true);
            dialog.SetActive(true);
        }

        public void CloseDialog()
        {
            ResetObjects();
            inputField.text = "";
            dialog.SetActive(false);
            dialog.SetActive(false);
        }

        private void Search(VerticeWrapper v)
        {
            SingletonManager.Instance.dataManager.InvokeSpecificVerticeSelected(1L,v);
            CloseDialog();
        }

        public void OnTypedString(string typed)
        {
            ResetObjects();
            if (typed == null || typed.Length < 2)
            {
                return;
            }
            
            foreach (var val in dataRenderer.loadedProjects[1L].verticeWrappers.Values
                         .Where(x=>!dataRenderer.filterHolder.disabledVertices.Contains(x.verticeData.verticeType)))
            {
                if (!val.verticeData.name.ToLower().Contains(typed.ToLower()))
                {
                    continue;
                }
                
                GameObject go = Instantiate(listItem, listItemHolder.transform);
                go.GetComponentInChildren<TMP_Text>().text = val.verticeData.name;
                VerticeWrapper tmp = val;
                go.GetComponentInChildren<Button>().onClick.AddListener(() =>Search(tmp));
            }
        }

        private void ResetObjects()
        {
            foreach (Transform child in listItemHolder.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}