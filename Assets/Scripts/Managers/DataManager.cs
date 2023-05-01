using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
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

    // Events
    public event Action ResetEvent;
    public event Action<Pair<long,List<DateTime>>> DatesSelectedEvent;
    public event Action<Pair<long,List<DateTime>>> DatesRangeSelectedEvent;
    public event Action<Pair<long,List<VerticeWrapper>>> VerticesSelectedEvent;
    
    public event Action<long> VerticesCompareEvent;
    public event Action<long> VerticesCompareEndEvent;
    
    // Other
    public List<DateTime> selectedDates = new();
    public List<VerticeWrapper> selectedVertices = new();
    public long selectedProjectId = -1;
    
    [Header("References")]
    private DataRenderer dataRenderer;
    private TimelineRenderer timelineRenderer;
    
    private void Start()
    {
        dataRenderer = FindObjectOfType<DataRenderer>();
        timelineRenderer = FindObjectOfType<TimelineRenderer>();
        ResetEvent += OnResetEvent;
    }

    public void LoadData(DataHolder holder)
    {
        projectIdCounter++;
        // Template name
        projectNames.Add(projectIdCounter,"SomeName"+projectIdCounter);
        holder.projectId = projectIdCounter;
        holder.LoadData();
        unchangedDataHolders.Add(projectIdCounter,holder);
        dataRenderer.AddData(holder, false, true);
    }

    public void SetFilter(FilterHolder h)
    {
        this.filterHolder = h;
        ApplyFilter();
    }
    
    public void ApplyFilter()
    {
        // filteredDataHolders.Clear();
        // filteredDataHolders = unchangedDataHolders
        //     .Select(h => new DataHolder(h.Value, filterHolder))
        //     .ToDictionary(i => i.projectId);
        //
        // dataRenderer.ResetData();
        // dataRenderer.AddData(filteredDataHolders.Values.ToList(), true);
    }

    public void ProcessVerticeClick(long projectId, VerticeWrapper verticeWrapper)
    {
        if (this.selectedProjectId != projectId)
        {
            this.ResetEvent?.Invoke();
        }

        this.selectedProjectId = projectId;

        bool ctrlPressed = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        // Adding single items
        if (ctrlPressed)
        {
            // Clicking already selected, removing them
            if (this.selectedVertices.Contains(verticeWrapper))
            {
                this.selectedVertices.Remove(verticeWrapper);
                if (this.selectedVertices.Count == 0)
                    ResetEvent?.Invoke();
                else
                    VerticesSelectedEvent?.Invoke(
                        new Pair<long, List<VerticeWrapper>>(this.selectedProjectId, this.selectedVertices));
            }
            // Clicking new, adding and changing filter
            else
            {
                this.selectedVertices.Add(verticeWrapper);
                VerticesSelectedEvent?.Invoke(
                    new Pair<long, List<VerticeWrapper>>(this.selectedProjectId, this.selectedVertices));
            }
        }
        else
        {
            this.selectedVertices = new List<VerticeWrapper>() { verticeWrapper };
            VerticesSelectedEvent?.Invoke(
                new Pair<long, List<VerticeWrapper>>(this.selectedProjectId, this.selectedVertices));

        }
    }

    public void OnResetEvent()
    {
        this.selectedDates.Clear();
        this.selectedVertices.Clear();
        this.selectedProjectId = -1;
    }

    public void ProcessDateClick(long projectId, DateTime date)
    {
        if (this.selectedProjectId != projectId)
        {
            this.ResetEvent?.Invoke();
        }

        this.selectedProjectId = projectId;

        bool ctrlPressed = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        bool shiftPressed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // Trying to select date Range
        if (ctrlPressed && shiftPressed)
        {
            if (this.selectedDates.Count == 0 || this.selectedDates.Contains(date))
            {
                ProcessDateClickAdd(date);
                return;
            }

            List<DateTime> tmp = new List<DateTime>() { this.selectedDates.Min(), date };
            this.ResetEvent?.Invoke();
            this.selectedDates.Clear();
            // this.selectedDates.AddRange(unchangedDataHolders[projectId].verticesByDate.Keys.Where(x=>x>=tmp[0] && x<=tmp[1]));

            DatesRangeSelectedEvent?.Invoke(new Pair<long, List<DateTime>>(projectId,tmp.OrderBy(x=>x).ToList()));
        }
        else if (!ctrlPressed && !shiftPressed)
        {
            this.selectedDates = new List<DateTime>() { date };
            DatesSelectedEvent?.Invoke(new Pair<long, List<DateTime>>(this.selectedProjectId, this.selectedDates));

        }
        // Adding single items
        else if (ctrlPressed && !shiftPressed)
        {
            ProcessDateClickAdd(date);
        }
    }

    private void ProcessDateClickAdd(DateTime date)
    {
        // Clicking already selected, removing them
        if (this.selectedDates.Contains(date))
        {
            this.selectedDates.Remove(date);
            if (this.selectedDates.Count == 0)
                ResetEvent?.Invoke();
            else
                DatesSelectedEvent?.Invoke(new Pair<long, List<DateTime>>(this.selectedProjectId, this.selectedDates));
        }
        // Clicking new, adding and changing filter
        else
        {
            this.selectedDates.Add(date);
            DatesSelectedEvent?.Invoke(new Pair<long, List<DateTime>>(this.selectedProjectId, this.selectedDates));
        }
        
    }

    public void InvokeResetEvent()
    {
        ResetEvent?.Invoke();
    }

    public void InvokeCompareEvent(long projectId)
    {
        VerticesCompareEvent?.Invoke(projectId);
    }
    
    public void InvokeCompareEndEvent(long projectId)
    {
        VerticesCompareEndEvent?.Invoke(projectId);
    }
}
