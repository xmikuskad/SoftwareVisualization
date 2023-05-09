using System;
using System.Collections.Generic;
using Helpers;
using TMPro;
using UIWidgets;
using UnityEngine;

public class SidebarBtnController : MonoBehaviour
{
    [Header("References")] public GameObject panelForProject;
    public TMP_Text projectName;
    public TMP_Text projectDate;
    public Sidebar sidebar;

    public Dictionary<long, DataHolder> projects = new ();
    public long activeProject = -1;

    private void Start()
    {
        SingletonManager.Instance.dataManager.DateChangeEvent += OnDateChangeEvent;
        SingletonManager.Instance.dataManager.DateRenderChangedEvent += OnDateRenderChanged;
    }

    public void Open()
    {
        sidebar.Open();
    }

    public void Close()
    {
        sidebar.Close();
    }

    public void OpenOrClose()
    {
        if (sidebar.IsOpen)
        {
            sidebar.Close();
        }
        else
        {
            sidebar.Open();
        }
    }

    public void AddProject(DataHolder dataHolder)
    {
        this.activeProject = dataHolder.projectId;
        this.projects[dataHolder.projectId] = dataHolder;
        SetProject(this.activeProject);
        if(!sidebar.IsOpen)
            sidebar.Open();
    }

    public void MoveProjectNext()
    {
        SetProject(this.activeProject+1);
    }

    public void MoveProjectBack()
    {
        SetProject(this.activeProject-1);
    }

    public void SetProject(long id)
    {
        if(!projects.ContainsKey(id))
            return;

        this.projectName.text = projects[id].projectName;
        this.projectDate.text = projects[id].minDate.ToString("dd/MM/yyyy") + " - "+projects[id].maxDate.ToString("dd/MM/yyyy");
    }

    private void OnDateRenderChanged(Pair<long, Pair<DateTime, DateTime>> pair)
    {
        if (this.activeProject != pair.Left)
            return;
        this.projectDate.text = pair.Right.Left.ToString("dd/MM/yyyy") + " - " + pair.Right.Right.ToString("dd/MM/yyyy");
    }

    private void OnDateChangeEvent(long projectId, DateTime date)
    {
        if (this.activeProject != projectId)
            return;
        this.projectDate.text = date.ToString("dd/MM/yyyy");
    }
}
