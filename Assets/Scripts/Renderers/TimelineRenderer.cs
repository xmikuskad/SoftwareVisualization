using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Renderers
{
    public class TimelineRenderer : MonoBehaviour
    {
        private List<MonthGroup> groupByMonth;
        private Dictionary<DateTime, TimelineBar> barObjects = new();

        [Header("References")] public RectTransform graphContainer;
        public RectTransform textPrefab;
        public RectTransform barPrefab;
        public TMP_Text tooltipPrefab;

        public Material oddMaterial;
        public Material evenMaterial;
        public Material highlightMaterial;
        public void LoadTimeline(DataHolder dataHolder)
        {
            ResetTimeline();
            Dictionary<DateTime, long> counts = dataHolder.eventCountByDate;
            if (counts.Count == 0)
            {
                return;
            }

            // Find max/min bar value
            long minValue = long.MaxValue;
            long maxValue = 0;
            foreach (var value in counts.Values)
            {
                if (value > maxValue)
                    maxValue = value;
                if (value < minValue)
                    minValue = value;
            }

            // Prepare max width/height
            long containerHeight = Convert.ToInt64(graphContainer.sizeDelta.y * 0.9f);
            long containerWidth = Convert.ToInt64(graphContainer.sizeDelta.x * 0.9f);
            long barWidth = Convert.ToInt64(containerWidth / (counts.Count + 1f) * 0.9f);
            long index = 0;
            long lastMonth = -1;
            Material activeMaterial = oddMaterial;
            foreach (var date in counts.Keys.OrderBy(x => x))
            {
                if (date.Month != lastMonth)
                {
                    activeMaterial = activeMaterial == oddMaterial ? evenMaterial : oddMaterial;
                    lastMonth = date.Month;
                }
                float xPosition = barWidth + index * barWidth;
                float yPosition = (counts[date] / (maxValue * 1f)) * containerHeight;
                TimelineBar timelineBar = CreateBar(new Vector2(xPosition, yPosition), barWidth * .9f, date, activeMaterial);
                this.barObjects[date] = timelineBar;
                // RectTransform labelX = Instantiate(textPrefab);
                // labelX.SetParent(graphContainer, false);
                // labelX.sizeDelta = new Vector2(barWidth*0.9f, 50f);
                // labelX.gameObject.SetActive(true);
                // labelX.anchoredPosition = new Vector2(xPosition, -30f);
                // labelX.anchorMin = new Vector2(0, 0);
                // labelX.anchorMax = new Vector2(0, 0);
                // labelX.pivot = new Vector2(.5f, 0f);
                // labelX.GetComponent<TMP_Text>().text =date.ToString("dd");
                // // labelX.GetComponent<TMP_Text>().text =date.ToString("dd/MM/yy");

                index++;
            }


            groupByMonth = counts.Keys
                .GroupBy(dt => new { dt.Month, dt.Year })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month) // optional
                .Select(g => new MonthGroup
                {
                    month = g.Key.Month,
                    year = g.Key.Year,
                    count = g.Count()
                }).ToList();

            float offset = barWidth / 2f;
            activeMaterial = evenMaterial;
            foreach (var group in groupByMonth)
            {
                RectTransform labelX = Instantiate(textPrefab);
                labelX.SetParent(graphContainer, false);
                labelX.sizeDelta = new Vector2(barWidth * group.count, 30f);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(offset + (barWidth * group.count / 2f), -30f);
                labelX.anchorMin = new Vector2(0, 0);
                labelX.anchorMax = new Vector2(0, 0);
                labelX.pivot = new Vector2(.5f, 0f);
                labelX.GetComponent<TMP_Text>().text = GetTooltip(group);
                labelX.GetComponent<TMP_Text>().color = activeMaterial.color;
                offset += barWidth * group.count;
                activeMaterial = activeMaterial == oddMaterial ? evenMaterial : oddMaterial;
            }
        }

        private TimelineBar CreateBar(Vector2 graphPosition, float barWidth, DateTime dateTime, Material material)
        {
            // GameObject gameObject = new GameObject("bar", typeof(Image));
            // gameObject.transform.SetParent(graphContainer, false);
            RectTransform rectTransform = Instantiate(barPrefab, graphContainer, false);

            // RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
            rectTransform.sizeDelta = new Vector2(barWidth, graphPosition.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(.5f, 0f);

            TimelineBar timelineBar = rectTransform.GetComponent<TimelineBar>();
            timelineBar.SetUp(dateTime, tooltipPrefab, material, highlightMaterial, this);
            rectTransform.GetComponent<Image>().material = material;
            return timelineBar;
        }

        public void ResetTimeline()
        {
            foreach (Transform child in graphContainer.transform)
            {
                Destroy(child.gameObject);
            }
            barObjects.Clear();
        }

        private string GetTooltip(MonthGroup group)
        {
            DateTime dt = new DateTime(group.year, group.month, 1); // create DateTime object for the first day of the month
            return dt.ToString("MMM") + " " + group.year; // combine month abbreviation and year
        }

        public void UnhighlightElements()
        {
            foreach (var bar in this.barObjects.Values)
            {
                bar.SetHighlighted(false);
            }
        }

        public void ShowHideTimeline()
        {
            graphContainer.gameObject.SetActive(!graphContainer.gameObject.activeInHierarchy);
        }

        public void SelectDate(DateTime date)
        {
            UnhighlightElements();
            SingletonManager.Instance.dataManager.HightlightDate(1L, date);
            // TODO call data renderer to filter
        }

        public void HighlightDate(DateTime date)
        {
            barObjects[date].SetHighlighted(true);
        }


        public void HighlightDates(List<DateTime> dates)
        {
            foreach (var dateTime in dates)
            {
                barObjects[dateTime].SetHighlighted(true);
            }
        }
    }

    class MonthGroup
    {
        public int month { get; set; }
        public int year { get; set; }
        public int count { get; set; }
    }
}