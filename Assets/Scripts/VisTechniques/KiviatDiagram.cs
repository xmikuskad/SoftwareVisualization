using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using E2C;
using Data;

public class KiviatDiagram : MonoBehaviour
{
    public GameObject defaultPersonToggleElement;

    public GameObject personToggleElementHolder;

    public GameObject kiwiatDiagramArea;

    public GameObject kiviatDiagramMain;

    private Dictionary<long, VerticeData> allPersons = new Dictionary<long, VerticeData>();

    public E2Chart e2chart;

    private E2ChartData e2ChartData;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void initiateKiviat(DataHolder dataHolder)
    {
        e2chart.chartData.series.Clear();
        Dictionary<long, List<float>> metrics = computeMetrics(dataHolder);
        foreach (KeyValuePair<long, List<float>> personIdToMetrics in metrics)
        {
            E2ChartData.Series newSeries = new E2ChartData.Series();
            newSeries.name = dataHolder.verticeWrappers[personIdToMetrics.Key].verticeData.name;
            newSeries.show = true;
            newSeries.dataY = personIdToMetrics.Value;
            e2chart.chartData.series.Add(newSeries);
            Debug.Log("ADDING " + dataHolder.verticeWrappers[personIdToMetrics.Key].verticeData.name);
        }

        e2chart.UpdateChart();
    }

    public Dictionary<long, List<float>> computeMetrics(DataHolder dataHolder)
    {
        foreach (VerticeData vertice in dataHolder.verticeData.Values) if (vertice.verticeType == VerticeType.Person) allPersons[vertice.id] = vertice;

        Dictionary<long, List<float>> metrics = new Dictionary<long, List<float>>();
        foreach (KeyValuePair<long, VerticeData> person in allPersons)
        {
            // changes commits repos files wikis
            List<float> metricz = new List<float>();
            Dictionary<VerticeType, List<VerticeData>> personRelatedVerticesDict = dataHolder.verticeWrappers[person.Value.id].GetRelatedVerticesDict();

            if (personRelatedVerticesDict.ContainsKey(VerticeType.Change)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.Change].Count));
            else metricz.Add(0.0f);
            if (personRelatedVerticesDict.ContainsKey(VerticeType.Commit)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.Commit].Count));
            else metricz.Add(0.0f);
            if (personRelatedVerticesDict.ContainsKey(VerticeType.RepoFile)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.RepoFile].Count));
            else metricz.Add(0.0f);
            if (personRelatedVerticesDict.ContainsKey(VerticeType.File)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.File].Count));
            else metricz.Add(0.0f);
            if (personRelatedVerticesDict.ContainsKey(VerticeType.Wiki)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.Wiki].Count));
            else metricz.Add(0.0f);


            Debug.Log("---- for person " + person.Value.name + " ----");
            foreach (var x in metricz) Debug.Log("currently in list " + x.ToString());
            Debug.Log("-------");

            float sum = 0;
            foreach (float v in metricz) sum += v;

            if (sum > 0)
                metrics[person.Value.id] = metricz;
            else { Debug.Log("ignoring " + person.Value.name + "cuz its all 0"); }
            // metrics[person.Value.id] = new List<float>() { 1, 2, 3, 4, 5 };
        }
        return metrics;
    }

}
