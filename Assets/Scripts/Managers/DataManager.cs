using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager: MonoBehaviour
{
    // Cached values
    private long projectIdCounter = 0;
    private Dictionary<long, String> projectNames = new();
    private Dictionary<long, DataHolder> unchangedDataHolders = new();
    private Dictionary<long, DataHolder> filteredDataHolders = new();

    private DataRenderer dataRenderer;

    // Filters
    private HashSet<EdgeType> edgeFilter;
    private HashSet<VerticeType> verticeFilter;
    
    private void Start()
    {
        dataRenderer = FindObjectOfType<DataRenderer>();
    }

    public void LoadData(DataHolder holder)
    {
        projectIdCounter++;
        // Template name
        projectNames.Add(projectIdCounter,"SomeName"+projectIdCounter);
        holder.projectId = projectIdCounter;
        unchangedDataHolders.Add(projectIdCounter,holder);
        
        Debug.Log(string.Join("\n", holder.edgeData.Values.Select(x => x.ToString()).ToArray()));
        Debug.Log(string.Join("\n", holder.verticeData.Values.Select(x => x.ToString()).ToArray()));
        Debug.Log(holder.projectId);
        dataRenderer.AddData(holder, false);
    }
    
    
    public void ApplyFilter()
    {
        filteredDataHolders.Clear();
        filteredDataHolders = unchangedDataHolders
            .Select(h => new DataHolder(h.Value, edgeFilter, verticeFilter))
            .ToDictionary(i => i.projectId);
        
        dataRenderer.ResetData();
        dataRenderer.AddData(filteredDataHolders.Values.ToList(), true);
    }

}
