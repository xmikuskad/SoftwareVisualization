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

    private FilterHolder filterHolder = new();

    private DataHolder currentDataHolder;

    // Start is called before the first frame update
    void Start()
    {
        SingletonManager.Instance.dataManager.DataFilterEvent += OnDataFilter;
    }
    private void OnDataFilter(FilterHolder f)
    {
        this.filterHolder = f;
        initiateKiviat(currentDataHolder);
    }

    public void initiateKiviat(DataHolder dataHolder)
    {
        currentDataHolder = dataHolder;
        e2chart.chartData.series.Clear();
        Dictionary<long, List<float>> metrics = computeMetrics(dataHolder);

        e2chart.chartData.categoriesX = new List<string>();
        // e2chart.chartData.categoriesX = new List<string>(){"changes", "commits", "repos", "files", "wikis"};
        // e2chart.chartData.categoriesY = new List<string>(){"changes", "commits", "repos", "files", "wikis"};
        e2chart.chartData.categoriesY = new List<string>();

        if (!filterHolder.disabledVertices.Contains(VerticeType.Change))
        {
            e2chart.chartData.categoriesX.Add("changes");  
            e2chart.chartData.categoriesY.Add("changes");  
        }
        
        if (!filterHolder.disabledVertices.Contains(VerticeType.Commit))
        {
            e2chart.chartData.categoriesX.Add("commits");  
            e2chart.chartData.categoriesY.Add("commits");  
        }
        if (!filterHolder.disabledVertices.Contains(VerticeType.RepoFile))
        {
            e2chart.chartData.categoriesX.Add("repos");  
            e2chart.chartData.categoriesY.Add("repos");  
        }
        if (!filterHolder.disabledVertices.Contains(VerticeType.File))
        {
            e2chart.chartData.categoriesX.Add("files");  
            e2chart.chartData.categoriesY.Add("files");  
        }
        if (!filterHolder.disabledVertices.Contains(VerticeType.Wiki))
        {
            e2chart.chartData.categoriesX.Add("wikis");  
            e2chart.chartData.categoriesY.Add("wikis");  
        }

        foreach (var (key, value) in metrics)
        {
            List<float> filteredMetrics = new();
            if (!filterHolder.disabledVertices.Contains(VerticeType.Change))
            {
                filteredMetrics.Add(value[0]);
            }
        
            if (!filterHolder.disabledVertices.Contains(VerticeType.Commit))
            {
                filteredMetrics.Add(value[1]); 
            }
            if (!filterHolder.disabledVertices.Contains(VerticeType.RepoFile))
            {
                filteredMetrics.Add(value[2]);
            }
            if (!filterHolder.disabledVertices.Contains(VerticeType.File))
            {
                filteredMetrics.Add(value[3]);
            }
            if (!filterHolder.disabledVertices.Contains(VerticeType.Wiki))
            {
                filteredMetrics.Add(value[4]);
            }
            
            E2ChartData.Series newSeries = new E2ChartData.Series();
            newSeries.name = dataHolder.verticeWrappers[key].verticeData.name;
            newSeries.show = true;
            newSeries.dataY = value;
            e2chart.chartData.series.Add(newSeries);
            Debug.Log("ADDING " + dataHolder.verticeWrappers[key].verticeData.name);
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
            List<float> metricz = new List<float>() { 0f, 0f, 0f, 0f, 0f };
            Dictionary<VerticeType, List<VerticeData>> personRelatedVerticesDict = dataHolder.verticeWrappers[person.Value.id].GetRelatedVerticesDict();

            if (personRelatedVerticesDict.ContainsKey(VerticeType.Change))
            {
                List<VerticeData> personRelatedChanges = personRelatedVerticesDict[VerticeType.Change];
                metricz[0] += (personRelatedChanges.Count);
                foreach (VerticeData personRelatedChange in personRelatedChanges)
                {
                    VerticeWrapper personRelatedChangeWrapper = dataHolder.verticeWrappers[personRelatedChange.id];
                    Dictionary<VerticeType, List<VerticeData>> verticesRelatedToChange = personRelatedChangeWrapper.GetRelatedVerticesDict();
                    if (verticesRelatedToChange.ContainsKey(VerticeType.Commit)) metricz[1] += (verticesRelatedToChange[VerticeType.Commit].Count);
                    else metricz[1] += (0.0f);
                    if (verticesRelatedToChange.ContainsKey(VerticeType.RepoFile)) metricz[2] += (verticesRelatedToChange[VerticeType.RepoFile].Count);
                    else metricz[2] += (0.0f);
                    if (verticesRelatedToChange.ContainsKey(VerticeType.File)) metricz[3] += (verticesRelatedToChange[VerticeType.File].Count);
                    else metricz[3] += (0.0f);
                    if (verticesRelatedToChange.ContainsKey(VerticeType.Wiki)) metricz[4] += (verticesRelatedToChange[VerticeType.Wiki].Count);
                    else metricz[4] += (0.0f);
                }
            }

            if (personRelatedVerticesDict.ContainsKey(VerticeType.Commit))
            {
                List<VerticeData> personRelatedCommits = personRelatedVerticesDict[VerticeType.Commit];
                metricz[1] += (personRelatedCommits.Count);
                foreach (VerticeData personRelatedCommit in personRelatedCommits)
                {
                    VerticeWrapper personRelatedCommitWrapper = dataHolder.verticeWrappers[personRelatedCommit.id];
                    Dictionary<VerticeType, List<VerticeData>> verticesRelatedToCommit = personRelatedCommitWrapper.GetRelatedVerticesDict();
                    if (verticesRelatedToCommit.ContainsKey(VerticeType.Commit)) metricz[1] += (verticesRelatedToCommit[VerticeType.Commit].Count);
                    else metricz[1] += (0.0f);
                    if (verticesRelatedToCommit.ContainsKey(VerticeType.RepoFile)) metricz[2] += (verticesRelatedToCommit[VerticeType.RepoFile].Count);
                    else metricz[2] += (0.0f);
                    if (verticesRelatedToCommit.ContainsKey(VerticeType.File)) metricz[3] += (verticesRelatedToCommit[VerticeType.File].Count);
                    else metricz[3] += (0.0f);
                    if (verticesRelatedToCommit.ContainsKey(VerticeType.Wiki)) metricz[4] += (verticesRelatedToCommit[VerticeType.Wiki].Count);
                    else metricz[4] += (0.0f);
                }
            }

            // if (personRelatedVerticesDict.ContainsKey(VerticeType.Change)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.Change].Count));
            // else metricz.Add(0.0f);
            // if (personRelatedVerticesDict.ContainsKey(VerticeType.Commit)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.Commit].Count));
            // else metricz.Add(0.0f);
            // if (personRelatedVerticesDict.ContainsKey(VerticeType.RepoFile)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.RepoFile].Count));
            // else metricz.Add(0.0f);
            // if (personRelatedVerticesDict.ContainsKey(VerticeType.File)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.File].Count));
            // else metricz.Add(0.0f);
            // if (personRelatedVerticesDict.ContainsKey(VerticeType.Wiki)) metricz.Add((float)Math.Sqrt(personRelatedVerticesDict[VerticeType.Wiki].Count));
            // else metricz.Add(0.0f);

            for (int i = 0; i < 5; i++)
            {
                if (metricz[i] < 0.01f) metricz[i] = 0.01f;
                else metricz[i] = (float)Math.Sqrt(metricz[i]);
                if (metricz[i] < 0.01f) metricz[i] = 0.01f;
            }

            // Debug.Log("---- for person " + person.Value.name + " ----");


            // foreach (var x in metricz) Debug.Log("currently in list " + x.ToString());

            float sum = 0;
            foreach (float v in metricz) sum += v;

            if (sum > 0.05f)
                metrics[person.Value.id] = metricz;
            // else { Debug.Log("ignoring " + person.Value.name + "cuz its all 0"); }


            // Debug.Log("-------");

            // metrics[person.Value.id] = new List<float>() { 1, 2, 3, 4, 5 };
        }
        return metrics;
    }

}
