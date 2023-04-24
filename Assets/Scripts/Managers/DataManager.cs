using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Renderers;
using UnityEngine;

public class DataManager: MonoBehaviour
{
    // Cached values
    private long projectIdCounter = 0;
    private Dictionary<long, String> projectNames = new();
    private Dictionary<long, DataHolder> unchangedDataHolders = new();
    private Dictionary<long, DataHolder> filteredDataHolders = new();

    // Filters
    private FilterHolder filterHolder;
    
    // Other
    public long highlightedProjectId = -1;
    public DateTime? highlightedDate = null;
    public long highlightedVerticeId = -1; // Unity has problem comparing GameObject so we need this too
    [CanBeNull] public VerticeData highlightedVerticeData = null;
    
    [Header("References")]
    private DataRenderer dataRenderer;
    private TimelineRenderer timelineRenderer;
    
    private void Start()
    {
        dataRenderer = FindObjectOfType<DataRenderer>();
        timelineRenderer = FindObjectOfType<TimelineRenderer>();
    }

    public void LoadData(DataHolder holder)
    {
        projectIdCounter++;
        // Template name
        projectNames.Add(projectIdCounter,"SomeName"+projectIdCounter);
        holder.projectId = projectIdCounter;
        holder.LoadData();
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

    public void HightlightDate(long projectId, DateTime date)
    {
        this.highlightedProjectId = projectId;
        if (highlightedVerticeId >= 0)
        {
            this.dataRenderer.UnhighlightElements(projectId);
            highlightedVerticeData = null;
            highlightedVerticeId = -1;
        }
        
        highlightedDate = date;
        this.timelineRenderer.HighlightDate(date);
        this.dataRenderer.HighlightVerticeByDate(projectId,date);
    }
    
    public void HightlightVertice(long projectId, VerticeData verticeData)
    {
        this.highlightedProjectId = projectId;
        this.timelineRenderer.UnhighlightElements();
        highlightedDate = null;
        
        highlightedVerticeData = verticeData;
        highlightedVerticeId = verticeData.id;
        this.dataRenderer.HighlightVertice(projectId,verticeData.id);
        List<DateTime> data;
        if(this.unchangedDataHolders[projectId].rawDatesForVertice.TryGetValue(verticeData.id, out data)) {
            this.timelineRenderer.HighlightDates(this.unchangedDataHolders[projectId].rawDatesForVertice[verticeData.id]);
        }
    }

    public void Unhighlight()
    {
        this.highlightedProjectId = -1;
        this.highlightedVerticeId = -1;
        this.highlightedVerticeData = null;
        this.highlightedDate = null;
        this.timelineRenderer.UnhighlightElements();
        this.dataRenderer.UnhighlightElements(1L);
    }

    public bool IsActiveHighlight()
    {
        return highlightedDate.HasValue || highlightedVerticeId < 0 ;
    }

}
