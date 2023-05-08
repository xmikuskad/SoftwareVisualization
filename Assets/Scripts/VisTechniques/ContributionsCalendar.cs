using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Renderers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContributionsCalendar : MonoBehaviour
{

    public DataHolder dataHolder;
    public GameObject contributionsCalendar;

    public GameObject tileElementHolder;

    public GameObject defaultTileElement;

    public TMP_Text defaultTileElementTooltip;

    public TMP_Dropdown yearDropdown;

    public TimelineRenderer timelineRenderer;

    // Start is called before the first frame update

    private void Start()
    {
        yearDropdown.onValueChanged.AddListener(onYearChange);
        yearDropdown.value = 1;
        SingletonManager.Instance.preferencesManager.MappingChangedEvent += OnMappingChanged;
    }

    public void onYearChange(int index)
    {
        string selectedOption = yearDropdown.options[index].text;
        fillContributionsCalendar(dataHolder, int.Parse(yearDropdown.options[index].text));
    }

    public void showContributionsCalendar()
    {
        contributionsCalendar.gameObject.SetActive(!contributionsCalendar.gameObject.activeSelf);
    }

    // Update is called once per frame
    public void fillContributionsCalendar(DataHolder dataHolder, int year)
    {
        removeCurrentTiles();

        this.dataHolder = dataHolder;

        // year = dataHolder.startDate.Year; // Specify the year here

        DateTime startDate = new DateTime(year, 1, 1); // Start date of the year
        DateTime endDate = startDate.AddYears(1).AddDays(-1); // End date of the year

        int index = 0;

        Dictionary<DateTime, long> originalCounts = dataHolder.verticesByDate
            .ToDictionary(kvp => kvp.Key, kvp => (long)kvp.Value.Count);
        if (originalCounts.Count == 0) return;
        Dictionary<DateTime, long> noTimeCounts = new Dictionary<DateTime, long>();

        // Find max/min bar value
        long minValue = long.MaxValue;
        long maxValue = 0;
        foreach (var value in originalCounts.Values)
        {
            if (value > maxValue)
                maxValue = value;
            if (value < minValue)
                minValue = value;
        }

        List<DateTime> keyList = new List<DateTime>(originalCounts.Keys);
        foreach (DateTime t in keyList) noTimeCounts[t.Date] = originalCounts[t];

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            string dateTooltip = date.ToString("dd/MM/yyyy");
            if (noTimeCounts.ContainsKey(date.Date)) dateTooltip += " - " + noTimeCounts[date].ToString() + " contributions";
            else dateTooltip += " - 0 contributions";
            CultureInfo ci = CultureInfo.CurrentCulture;
            System.Globalization.Calendar cal = ci.Calendar;
            Vector3 pos = defaultTileElement.transform.position;
            pos.y = pos.y - (int)date.DayOfWeek * 30.0f;
            defaultTileElementTooltip.text = date.ToString("dd/MM/yyyy");
            pos.x = pos.x + 30.0f * ((int)cal.GetWeekOfYear(date, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek) - 1);
            GameObject newDateElement = Instantiate(defaultTileElement, pos, Quaternion.identity, tileElementHolder.transform);
            newDateElement.GetComponentInChildren<Image>().color =
                GradientUtility.CreateGradient((noTimeCounts.ContainsKey(date.Date) ? ((noTimeCounts[date] * 1.0f - minValue * 1.0f) / (maxValue * 1.0f - minValue * 1.0f)) * 1.0f : 0.0f),
                    SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.TILEMAPHIGHLIGHT).color);
            DateTime tmp = date;
            newDateElement.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                timelineRenderer.OnBtnClick(tmp,dataHolder.projectId);
                defaultTileElementTooltip.text = dateTooltip;
            });
            newDateElement.gameObject.SetActive(true);
            if ((int)date.DayOfWeek == 6) index++;
        }

    }
    private void removeCurrentTiles()
    {
        foreach (Transform child in tileElementHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void OnMappingChanged(Dictionary<long, ColorMapping> colorMappings)
    {
        if (dataHolder != null) fillContributionsCalendar(dataHolder, dataHolder.startDate.Year);
    }
}
