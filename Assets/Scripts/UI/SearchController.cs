using System;
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
        private long projectId = -1;
        private FilterHolder f = new();
        
        [Header("References")] public GameObject dialog;
        [FormerlySerializedAs("colorListItem")] public GameObject listItem;
        [FormerlySerializedAs("colorListItemHolder")] public GameObject listItemHolder;

        public TMP_InputField inputField;
        public DataRenderer dataRenderer;

        private void Start()
        {
            SingletonManager.Instance.dataManager.DataFilterEvent += OnDataFilter;
            SingletonManager.Instance.dataManager.SelectedProjectChanged += OnSelectedProjectChanged;
        }

        private void OnDataFilter(FilterHolder f)
        {
            this.f = f;
        }

        private void OnSelectedProjectChanged(DataHolder dataHolder)
        {
            this.projectId = dataHolder.projectId;
        }

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
            SingletonManager.Instance.dataManager.InvokeSpecificVerticeSelected(projectId,v);
            CloseDialog();
        }

        public void OnTypedString(string typed)
        {
            ResetObjects();
            if (typed == null || typed.Length < 2)
            {
                return;
            }

            foreach (var val in dataRenderer.loadedProjects[projectId].verticeWrappers.Values
                         .Where(x => !dataRenderer.filterHolder.disabledVertices.Contains(x.verticeData.verticeType) &&
                                     x.verticeData.verticeType != VerticeType.Change &&
                                     x.verticeData.verticeType != VerticeType.Commit &&
                                     !f.disabledVertices.Contains(x.verticeData.verticeType)))
            {
                if (!val.verticeData.name.ToLower().Contains(typed.ToLower()) &&
                    !val.verticeData.title.ToLower().Contains(typed.ToLower()) &&
                    !val.verticeData.id.ToString().Contains(typed.ToLower()))
                {
                    continue;
                }

                GameObject go = Instantiate(listItem, listItemHolder.transform);
                go.GetComponentInChildren<TMP_Text>().text = "<b>[" + val.verticeData.verticeType + " " +
                                                             val.verticeData.id + "]</b> " + val.verticeData.name;
                VerticeWrapper tmp = val;
                go.GetComponentInChildren<Button>().onClick.AddListener(() => Search(tmp));
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