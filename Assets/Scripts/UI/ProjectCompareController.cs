using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using UnityEngine;
using TMPro;
using UIWidgets;
using UnityEngine.UI;

namespace UI
{
    public class ProjectCompareController : MonoBehaviour
    {
        private Dictionary<long, DataHolder> dataHolders = new();

        private long firstProjectId = 1;
        private long secondProjectId = 2;

        // private List<GameObject> tiles = new();

        [Header("Properties")] public int maxWidthCount = 10;
        public int maxHeightCount = 10;
        public int distanceBetween = 2;
        
        [Header("References")] public GameObject window;
        public Button projectCompareButton;
        public TMP_Dropdown firstProjectDropdown;
        public TMP_Dropdown secondProjectDropdown;
        public GameObject tilePrefab;
        public GameObject spawnArea;

        public TMP_Text dayTxt;
        public TMP_Text firstDateTxt;
        public TMP_Text secondDateTxt;
        
        
        public GameObject previousPageBtn;
        public GameObject nextPageBtn;

        private long currentPage = 0;
        private long maxPage = 1;
        private long maxChanges = 0;
        private long maxDays = 0;
        
        private void Start()
        {
            SingletonManager.Instance.dataManager.SelectedProjectChanged += OnSelectedProjectChanged;
        }

        private void OnSelectedProjectChanged(DataHolder dataHolder)
        {
            if (dataHolders.ContainsKey(dataHolder.projectId))
            {
                return;
            }
            dataHolders.Add(dataHolder.projectId,dataHolder);

            if (dataHolders.Count >= 2)
            {
                projectCompareButton.interactable = true;
            }
            
            firstProjectDropdown.options.Clear();
            firstProjectDropdown.options = dataHolders.Values.OrderBy(x => x.projectId)
                .Select(x => new TMP_Dropdown.OptionData(x.projectName)).ToList();
            
            secondProjectDropdown.options.Clear();
            secondProjectDropdown.options = dataHolders.Values.OrderBy(x => x.projectId)
                .Select(x => new TMP_Dropdown.OptionData(x.projectName)).ToList();
        }

        public void OpenDialog()
        {
            firstProjectDropdown.onValueChanged.AddListener(OnFirstProjectChange);
            firstProjectDropdown.value = 0;
            
            secondProjectDropdown.onValueChanged.AddListener(OnSecondProjectChange);
            secondProjectDropdown.value = 1;
            
            window.gameObject.SetActive(true);
        }

        public void CloseDialog()
        {
            window.gameObject.SetActive(false);
        }
        
        public void OnFirstProjectChange(int index)
        {
            firstProjectId = index + 1;
            RerenderGraph();
        }
        
        public void OnSecondProjectChange(int index)
        {
            secondProjectId = index + 1;
            RerenderGraph();
        }

        public void NextPage()
        {
            if(currentPage+1 > maxPage)
                return;
            currentPage += 1;
            RenderTiles(currentPage);
        }
        
        public void PreviousPage()
        {
            if(currentPage == 0)
                return;
            currentPage -= 1;
            RenderTiles(currentPage);
        }
        
        private void ResetGraph()
        {
            // foreach (var tile in tiles)
            // {
            //     Destroy(gameObject);
            // }
            
            foreach (Transform child in spawnArea.transform)
            {
                Destroy(child.gameObject);
            }

            dayTxt.gameObject.SetActive(false);
            firstDateTxt.gameObject.SetActive(false);
            secondDateTxt.gameObject.SetActive(false);
        }

        private void RerenderGraph()
        {
            Debug.Log("First project: " + firstProjectId + " | Second project: " + secondProjectId);

            ResetGraph();
            
            
            TimeSpan diff1 = dataHolders[firstProjectId].maxDate -dataHolders[firstProjectId].minDate ;
            int days1 = (int)diff1.TotalDays;
            
            TimeSpan diff2 = dataHolders[secondProjectId].maxDate -dataHolders[secondProjectId].minDate ;
            int days2 = (int)diff2.TotalDays;

            maxDays = days1 > days2 ? days1 : days2;
            maxPage = Mathf.CeilToInt((maxDays * 1.0f) / (maxHeightCount * 1.0f * maxWidthCount))-1;

            int maxChanges1 = dataHolders[firstProjectId].verticesByDate.Select(x => x.Value.Count).Max();
            int maxChanges2 = dataHolders[firstProjectId].verticesByDate.Select(x => x.Value.Count).Max();
            maxChanges = maxChanges1 > maxChanges2 ? maxChanges1 : maxChanges2;

            if (maxPage == 0)
            {
                nextPageBtn.gameObject.SetActive(false);
                previousPageBtn.gameObject.SetActive(false);
            }
            else
            {
                nextPageBtn.gameObject.SetActive(true);
                previousPageBtn.gameObject.SetActive(true);
            }

            RenderTiles(0);
        }

        private void RenderTiles(long page)
        {
            Vector3 basePos = spawnArea.transform.position;

            int widthTracker = 0;
            int heightTracker = 0;
            for (long i = page * maxHeightCount * maxWidthCount; i <= maxDays; i++)
            {
                InstantiateTitlePrefab(basePos + new Vector3(widthTracker, -heightTracker, 0)*distanceBetween, i);

                widthTracker++;
                if (widthTracker > maxWidthCount)
                {
                    widthTracker = 0;
                    heightTracker++;
                }

                if (heightTracker > maxHeightCount)
                {
                    break;
                }
            }
        }

        public void InstantiateTitlePrefab(Vector3 pos, long currentDay)
        {
            long tmp = currentDay;
            GameObject go = Instantiate(tilePrefab, pos, Quaternion.identity, spawnArea.transform);
            DateTime date = dataHolders[firstProjectId].startDate.AddDays(currentDay);
            long changeCount = GetChangeCount(firstProjectId, date);
            go.GetComponentInChildren<Image>().color =
                GradientUtility.CreateGradient(changeCount * 1.0f / maxChanges, Color.green);
            go.GetComponent<Button>().onClick.AddListener(() => OnTileClick(tmp));

            
            DateTime date2 = dataHolders[secondProjectId].startDate.AddDays(currentDay);
            long changeCount2= GetChangeCount(secondProjectId, date2);
            GameObject go2 = Instantiate(tilePrefab, pos + new Vector3(7.5f, 0f, 0f), Quaternion.identity,
                spawnArea.transform);
            go2.GetComponentInChildren<Image>().color =
                GradientUtility.CreateGradient(changeCount2 * 1.0f / maxChanges, Color.red);
            ;
            go2.transform.localScale = new Vector3(0.5f, 1f, 1f);
            go2.GetComponent<Button>().onClick.AddListener(() => OnTileClick(tmp));
            // tiles.Add(go2);

            // tiles.Add(go);
        }

        private void OnTileClick(long day)
        {
            DateTime first = dataHolders[firstProjectId].startDate.AddDays(day);
            firstDateTxt.text = first.ToString("dd/MM/yyyy") + " (" + GetChangeCount(firstProjectId,first) + ")";
            DateTime second = dataHolders[secondProjectId].startDate.AddDays(day);
            secondDateTxt.text = second.ToString("dd/MM/yyyy") + " (" + GetChangeCount(secondProjectId,second) + ")";
            dayTxt.text = "Day " + (day+1);
            
            firstDateTxt.gameObject.SetActive(true);
            secondDateTxt.gameObject.SetActive(true);
            dayTxt.gameObject.SetActive(true);
        }

        public long GetChangeCount(long projectId, DateTime date)
        {
            if (!dataHolders[projectId].verticesByDate.ContainsKey(date))
                return 0;

            return dataHolders[projectId].verticesByDate[date].Count;
        }
    }
}