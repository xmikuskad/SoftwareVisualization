using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;

public class ContributionsCalendar : MonoBehaviour
{

    public DataHolder dataHolder;
    public GameObject contributionsCalendar;

    public GameObject tileElementHolder;

    public GameObject defaultTileElement;

    public TMP_Text defaultTileElementTooltip;

    // Start is called before the first frame update

    private void Start()
    {
        SingletonManager.Instance.preferencesManager.MappingChangedEvent += OnMappingChanged;
    }
    public void showContributionsCalendar()
    {
        contributionsCalendar.gameObject.SetActive(!contributionsCalendar.gameObject.activeSelf);
    }

    // Update is called once per frame
    public void fillContributionsCalendar(DataHolder dataHolder)
    {
        this.dataHolder = dataHolder;

        int year = 2020; // Specify the year here

        DateTime startDate = new DateTime(year, 1, 1); // Start date of the year
        DateTime endDate = startDate.AddYears(1).AddDays(-1); // End date of the year

        int index = 0;

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            string dateTooltip = date.ToString("dd/MM");
            CultureInfo ci = CultureInfo.CurrentCulture;
            System.Globalization.Calendar cal = ci.Calendar;
            Vector3 pos = defaultTileElement.transform.position;
            pos.y = pos.y - (int)date.DayOfWeek * 30.0f;
            defaultTileElementTooltip.text = date.ToString("dd/MM");
            pos.x = pos.x + 30.0f * ((int)cal.GetWeekOfYear(date, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek) - 1);
            GameObject newDateElement = Instantiate(defaultTileElement, pos, Quaternion.identity, tileElementHolder.transform);
            newDateElement.GetComponentInChildren<Image>().color =
                GradientUtility.CreateGradient(UnityEngine.Random.Range(0.0f, 1.0f),
                    SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.TILEMAPHIGHLIGHT).color);
            newDateElement.GetComponentInChildren<Button>().onClick.AddListener(() => defaultTileElementTooltip.text = dateTooltip);
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
        if (dataHolder != null) fillContributionsCalendar(dataHolder);
    }
}
