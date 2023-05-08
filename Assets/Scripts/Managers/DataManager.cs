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
    private Dictionary<long, DataHolder> unchangedDataHolders = new();

    // Filters
    private FilterHolder filterHolder;

    // Events
    public event Action<ResetEventReason> ResetEvent;   // Called for clearings
    public event Action<Pair<long,List<DateTime>>> DatesSelectedEvent;  // Called on calendar clicked
    public event Action<Pair<long,List<DateTime>>> DatesRangeSelectedEvent; // Called on calendar clicked
    public event Action<Pair<long,List<Pair<VerticeData,VerticeWrapper>>>> VerticesSelectedEvent;   // Called on vertice click
    public event Action<Pair<long,Pair<DateTime,DateTime>>> DateRenderChangedEvent; 
    public event Action<FilterHolder> DataFilterEvent;  // When filter is applied

    public event Action<long, DateTime> DateChangeEvent;    // When moving forward/backwards with dates
    public event Action<long, VerticeWrapper> SpecificVerticeSelected;    // When moving forward/backwards with dates
    
    // Other
    public List<DateTime> selectedDates = new();
    public List<Pair<VerticeData,VerticeWrapper>> selectedVertices = new();
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
        holder.projectId = projectIdCounter;
        holder.LoadData();
        unchangedDataHolders.Add(projectIdCounter,holder);
        dataRenderer.AddData(holder, false, true);
    }

    public void InvokeDataFilterEvent(FilterHolder f)
    {
        DataFilterEvent?.Invoke(f);
    }

    public void ProcessVerticeClick(long projectId, Pair<VerticeData,VerticeWrapper> pair)
    {
        if (this.selectedProjectId != projectId)
        {
            this.ResetEvent?.Invoke(ResetEventReason.PROJECT_CHANGED);
        }

        this.selectedProjectId = projectId;

        bool ctrlPressed = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        // Adding single items
        if (ctrlPressed)
        {
            // Clicking already selected, removing them
            if (this.selectedVertices.Contains(pair))
            {
                this.selectedVertices.Remove(pair);
                if (this.selectedVertices.Count == 0)
                {
                    ResetEvent?.Invoke(ResetEventReason.VERTICE_UNSELECTED);
                }
                else
                    VerticesSelectedEvent?.Invoke(
                        new Pair<long, List<Pair<VerticeData,VerticeWrapper>>>(this.selectedProjectId, this.selectedVertices));
            }
            // Clicking new, adding and changing filter
            else
            {
                this.selectedVertices.Add(pair);
                VerticesSelectedEvent?.Invoke(
                    new Pair<long, List<Pair<VerticeData,VerticeWrapper>>>(this.selectedProjectId, this.selectedVertices));
            }
        }
        else
        {
            this.selectedVertices = new List<Pair<VerticeData,VerticeWrapper>>() { pair };
            VerticesSelectedEvent?.Invoke(
                new Pair<long, List<Pair<VerticeData,VerticeWrapper>>>(this.selectedProjectId, this.selectedVertices));

        }
    }

    public void OnResetEvent(ResetEventReason reason)
    {
        this.selectedDates.Clear();
        this.selectedVertices.Clear();
        this.selectedProjectId = -1;
    }

    public void ProcessDateClick(long projectId, DateTime date)
    {
        if (this.selectedProjectId != projectId)
        {
            this.ResetEvent?.Invoke(ResetEventReason.PROJECT_CHANGED);
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
            this.ResetEvent?.Invoke(ResetEventReason.CLEARING_DATES);
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
                ResetEvent?.Invoke(ResetEventReason.DATES_UNSELECTED);
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

    public void InvokeResetEvent(ResetEventReason reason)
    {
        ResetEvent?.Invoke(reason);
    }

    public void InvokeRenderDateEvent(long projectId, Pair<DateTime, DateTime> pair)
    {
        DateRenderChangedEvent?.Invoke(new Pair<long, Pair<DateTime, DateTime>>(projectId, pair));
    }

    public void InvokeVerticeSelect(List<Pair<VerticeData,VerticeWrapper>> v, long projectId)
    {
        VerticesSelectedEvent?.Invoke(new Pair<long, List<Pair<VerticeData,VerticeWrapper>>>(projectId,v));
    }

    public void InvokeDateChangedEvent(long projectId, DateTime date)
    {
        DateChangeEvent?.Invoke(projectId,date);
    }
    
    public void InvokeSpecificVerticeSelected(long projectId, VerticeWrapper verticeWrapper)
    {
        ResetEvent?.Invoke(ResetEventReason.CLEARING_DATES);
        SpecificVerticeSelected?.Invoke(projectId,verticeWrapper);
    }
}
