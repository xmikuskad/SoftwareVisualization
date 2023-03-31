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
    private FilterHolder filterHolder;
    
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
        holder.CreateEventData();
        unchangedDataHolders.Add(projectIdCounter,holder);
        dataRenderer.AddData(holder, false);
    }

    public void SetFilter(FilterHolder h)
    {
        this.filterHolder = h;
        ApplyFilter();
    }
    
    public void ApplyFilter()
    {
        filteredDataHolders.Clear();
        filteredDataHolders = unchangedDataHolders
            .Select(h => new DataHolder(h.Value, filterHolder))
            .ToDictionary(i => i.projectId);
        
        dataRenderer.ResetData();
        dataRenderer.AddData(filteredDataHolders.Values.ToList(), true);
    }

}
