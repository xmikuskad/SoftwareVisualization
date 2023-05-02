using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using TMPro;
using UnityEngine;

public class VerticeRenderer : MouseOverRenderer
{

    public bool isGhost = false;
    private long projectId;
    public VerticeWrapper verticeWrapper;
    private MeshRenderer meshRenderer;

    private long completedCount = 0;
    private long taskCount = 0;
    private bool shouldHover = false;
    private bool isLoaded = false;

    [Header("References")] private Canvas hoverCanvas;
    private GameObject hoverElement;
    private TMP_Text hoverText;

    public Material hoverMaterial;
    public Material nonHoverMaterial;
    public Material hightlightMaterial;
    public Material hiddenMaterial;

    private Vector3 offPosition = new(2000, 20000, 2000);

    private SidebarController sidebarScript;
    private DataRenderer dataRenderer;


    protected void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    protected new void Start()
    {
        base.Start();
        this.hightlightMaterial.color = SingletonManager.Instance.preferencesManager
            .GetColorMapping(ColorMapping.HIGHLIGHTED).color;
        this.hiddenMaterial.color =
            SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.HIDDEN).color;
    }

    private void OnResetEvent()
    {
        this.meshRenderer.material = nonHoverMaterial;
    }

    private void OnVerticeSelected(Pair<long, List<VerticeWrapper>> pair)
    {
        if (this.projectId != pair.Left)
        {
            this.meshRenderer.material = nonHoverMaterial;
            return;
        }

        if (this.verticeWrapper.IsConnectedWithVertices(pair.Right.Select(x => x.verticeData.id).ToHashSet()))
        {
            SetHighlighted(pair.Right.Select(x=>x.verticeData.id).ToHashSet().Contains(this.verticeWrapper.verticeData.id) ? true : false);
        }
        else
        {
            SetHidden(true);
        }
    }

    private void OnDatesSelected(Pair<long, List<DateTime>> pair)
    {
        if (this.projectId != pair.Left)
        {
            this.meshRenderer.material = nonHoverMaterial;
            return;
        }

        if (this.verticeWrapper.ContainsDate(pair.Right))
        {
            SetHighlighted(false);
        }
        else
        {
            SetHidden(true);
        }
    }

    public void OnSpawned()
    {
        SingletonManager.Instance.preferencesManager.MappingChangedEvent += OnMappingChanged;
        SingletonManager.Instance.dataManager.ResetEvent += OnResetEvent;
        SingletonManager.Instance.dataManager.VerticesSelectedEvent += OnVerticeSelected;
        SingletonManager.Instance.dataManager.DatesSelectedEvent += OnDatesSelected;
        SingletonManager.Instance.dataManager.VerticesCompareEvent += OnVerticeCompare;
        SingletonManager.Instance.dataManager.VerticesCompareEndEvent += OnVerticeCompareEnd;
        SingletonManager.Instance.dataManager.DatesRangeSelectedEvent += OnDateRangeSelected;
    }

    public void OnDespawned()
    {
        SingletonManager.Instance.preferencesManager.MappingChangedEvent -= OnMappingChanged;
        SingletonManager.Instance.dataManager.ResetEvent -= OnResetEvent;
        SingletonManager.Instance.dataManager.VerticesSelectedEvent -= OnVerticeSelected;
        SingletonManager.Instance.dataManager.DatesSelectedEvent -= OnDatesSelected;
        SingletonManager.Instance.dataManager.VerticesCompareEvent -= OnVerticeCompare;
        SingletonManager.Instance.dataManager.VerticesCompareEndEvent -= OnVerticeCompareEnd;
        SingletonManager.Instance.dataManager.DatesRangeSelectedEvent -= OnDateRangeSelected;
    }
    
    private void OnDateRangeSelected(Pair<long, List<DateTime>> pair)
    {
        if (this.projectId != pair.Left)
        {
            this.meshRenderer.material = nonHoverMaterial;
            return;
        }
        
        if (this.verticeWrapper.IsDateBetween(pair.Right[0],pair.Right[1]))
        {
            SetHighlighted(false);
        }
        else
        {
            SetHidden(true);
        }
    }


    private void OnVerticeCompareEnd(long projectId)
    {
        if (this.projectId != projectId)
        {
            OnResetEvent();
            return;
        }

        Vector3 pos = this.transform.position;
        pos.z += 0.25f;
        this.transform.position = pos;
        this.transform.localScale = new Vector3(1, 1, 1f);
    }
    private void OnVerticeCompare(long projectId)
    {
        if (this.projectId != projectId)
        {
            OnResetEvent();
            return;
        }

        Vector3 pos = this.transform.position;
        pos.z -= 0.25f;
        this.transform.position = pos;
        this.transform.localScale = new Vector3(1, 1, 0.5f);
    }

    private void OnMappingChanged(Dictionary<long, ColorMapping> colorMappings)
    {
        // TODO
        this.hightlightMaterial.color = colorMappings[ColorMapping.HIGHLIGHTED.id].color;
        this.hiddenMaterial.color = colorMappings[ColorMapping.HIDDEN.id].color;

        switch (this.verticeWrapper.verticeData.verticeType)
        {
            case VerticeType.Person:
                this.nonHoverMaterial.color = colorMappings[ColorMapping.PERSON.id].color;
                break;
            case VerticeType.Ticket:
                this.nonHoverMaterial.color = colorMappings[ColorMapping.TICKET.id].color;
                break;
            case VerticeType.File:
                this.nonHoverMaterial.color = colorMappings[ColorMapping.FILE.id].color;
                break;
            case VerticeType.Wiki:
                this.nonHoverMaterial.color = colorMappings[ColorMapping.WIKI.id].color;
                break;
            case VerticeType.Commit:
                this.nonHoverMaterial.color = colorMappings[ColorMapping.COMMIT.id].color;
                break;
            case VerticeType.RepoFile:
                this.nonHoverMaterial.color = colorMappings[ColorMapping.REPOFILE.id].color;
                break;
            default:
                break;
        }
    }

    private Color32 GetColor()
    {
        switch (this.verticeWrapper.verticeData.verticeType)
        {
            case VerticeType.Person:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.PERSON).color;
            case VerticeType.Ticket:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.TICKET).color; ;
            case VerticeType.File:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.FILE).color;
            case VerticeType.Wiki:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.WIKI).color;
            case VerticeType.Commit:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.COMMIT).color;
            case VerticeType.RepoFile:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.REPOFILE).color;
            default:
                return new Color32(0, 0, 0, 0);
        }
    }

    public override void OnHoverEnter()
    {
        if (verticeWrapper == null || !isLoaded)
        {
            return;
        }

        if (verticeWrapper.verticeData.verticeType == VerticeType.Ticket)
        {
            hoverText.text = completedCount + " / " + taskCount;
        }
        else
        {
            hoverText.text = verticeWrapper.verticeData.ToString();
        }

        if (shouldHover)
            meshRenderer.material = hoverMaterial;

    }

    public override void OnHoverExit() // This is somehow being called a lot!
    {
        if (!isLoaded) return;

        // if(hoverElement) hoverElement.SetActive(false);
        hoverElement.transform.position = offPosition;
        if (shouldHover)
            meshRenderer.material = nonHoverMaterial;

    }

    public override void OnClick()
    {
        if (!isLoaded) return;
        // sidebarScript.slideOut(projectId, verticeWrapper.verticeData);
        SingletonManager.Instance.dataManager.ProcessVerticeClick(this.projectId, this.verticeWrapper);
    }

    public void SetUpReferences(Canvas hoverCanvas, GameObject hoverElement, TMP_Text hoverText,
        SidebarController sidebarController, DataRenderer dataRenderer)
    {
        this.hoverElement = hoverElement;
        this.hoverCanvas = hoverCanvas;
        this.hoverText = hoverText;
        this.sidebarScript = sidebarController;
        this.dataRenderer = dataRenderer;
    }

    public override Canvas GetCanvas()
    {
        return hoverCanvas;
    }

    public override GameObject GetHoverObject()
    {
        return hoverElement;
    }

    public void SetVerticeData(VerticeWrapper verticeWrapper, long projectId, Material material)
    {
        this.verticeWrapper = verticeWrapper;
        this.projectId = projectId;

        // We need to duplicate material because otherwise all objects with that material will be changed
        Material newMat = new Material(material);
        newMat.color = GetColor();
        meshRenderer.material = newMat;
        this.nonHoverMaterial = newMat;
    }

    public void AddCompletedEdge(long count, long maxTasks)
    {
        this.completedCount += count;
        this.taskCount = maxTasks;
    }

    public void SetHighlighted(bool isHighlighted)
    {
        this.shouldHover = !isHighlighted;
        this.meshRenderer.material = isHighlighted ? hightlightMaterial : nonHoverMaterial;
    }

    public void SetHidden(bool isHidden)
    {
        this.shouldHover = !isHidden;
        this.meshRenderer.material = isHidden ? hiddenMaterial : nonHoverMaterial;
    }

    public void SetIsLoaded(bool isLoaded)
    {
        this.isLoaded = isLoaded;
    }

}
