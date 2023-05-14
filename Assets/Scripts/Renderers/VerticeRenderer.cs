using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class VerticeRenderer : MouseOverRenderer
{

    public bool isGhost = false;
    private long projectId = -1;
    [CanBeNull] public VerticeData commitOrChange;
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

    public Mesh cubeMesh;
    public Mesh sphereMesh;
    public Mesh capsuleMesh;
    public Mesh cylinderMesh;

    private Vector3 offPosition = new(2000, 20000, 2000);

    private SidebarController sidebarScript;
    private DataRenderer dataRenderer;

    public DateTime beforeDate = DateTime.MinValue.Date;
    public DateTime afterDate = DateTime.MinValue.Date;

    private bool isLast = false;

    private MeshFilter meshFilter;
    private VerticeShape shape;

    protected void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        this.transform.localScale = new Vector3(1, 1, 1);
    }

    protected new void Start()
    {
        base.Start();
        this.hightlightMaterial = new Material(hightlightMaterial);
        this.hiddenMaterial = new Material(hiddenMaterial);
        this.hoverMaterial = new Material(hoverMaterial);
        this.hightlightMaterial.color = SingletonManager.Instance.preferencesManager
            .GetColorMapping(ColorMapping.HIGHLIGHTED).color;
        this.hiddenMaterial.color =
            SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.HIDDEN).color;
        this.hoverMaterial.color =
            SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.VERTICE_HOVER).color;
    }

    private void OnResetEvent(ResetEventReason reason)
    {
        this.meshRenderer.material = nonHoverMaterial;
        ChangeScale(false);
    }

    private void OnVerticeSelected(List<Pair<VerticeData, VerticeWrapper>> list)
    {
        if (list.Any(x => this.verticeWrapper.IsConnected(x, this.commitOrChange)))
        {
            SetHighlighted(true);
            ChangeScale(false);
        }
        else
        {
            SetHidden(true);
            ChangeScale(false);
        }

    }

    private void OnDatesSelected(Pair<long, List<DateTime>> pair)
    {
        if (this.projectId != pair.Left)
        {
            this.meshRenderer.material = nonHoverMaterial;
            return;
        }
        if (this.commitOrChange?.HasDatesWithoutHours(pair.Right) ?? this.verticeWrapper.ContainsDate(pair.Right))
        {
            SetHighlighted(true);
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
        SingletonManager.Instance.dataManager.DatesRangeSelectedEvent += OnDateRangeSelected;
        SingletonManager.Instance.dataManager.SpecificVerticeSelected += OnSpecificVerticeSelected;
    }

    public void OnDespawned()
    {
        SingletonManager.Instance.preferencesManager.MappingChangedEvent -= OnMappingChanged;
        SingletonManager.Instance.dataManager.ResetEvent -= OnResetEvent;
        SingletonManager.Instance.dataManager.VerticesSelectedEvent -= OnVerticeSelected;
        SingletonManager.Instance.dataManager.DatesSelectedEvent -= OnDatesSelected;
        SingletonManager.Instance.dataManager.DatesRangeSelectedEvent -= OnDateRangeSelected;
        SingletonManager.Instance.dataManager.SpecificVerticeSelected -= OnSpecificVerticeSelected;
    }

    private void ChangeScale(bool makeBigger)
    {
        if (this.verticeWrapper.verticeData.verticeType == VerticeType.Person)
            return;
        this.transform.localScale = makeBigger ? new Vector3(2, 2, 2) : new Vector3(1, 1, 1);
    }

    private void OnSpecificVerticeSelected(long projectId, VerticeWrapper verticeWrapper)
    {
        if (this.projectId != projectId)
        {
            this.meshRenderer.material = nonHoverMaterial;
            ChangeScale(false);
            return;
        }

        if (this.verticeWrapper.verticeData.id == verticeWrapper.verticeData.id)
        {
            SetHighlighted(true);
            ChangeScale(true);
        }
        else
        {
            SetHidden(true);
            ChangeScale(false);
        }
    }
    private void OnDateRangeSelected(Pair<long, List<DateTime>> pair)
    {
        if (this.projectId != pair.Left)
        {
            this.meshRenderer.material = nonHoverMaterial;
            return;
        }

        if (this.commitOrChange?.IsDateBetween(pair.Right[0], pair.Right[1]) ?? this.verticeWrapper.IsDateBetween(pair.Right[0], pair.Right[1]))
        {
            SetHighlighted(true);
        }
        else
        {
            SetHidden(true);
        }
    }

    private void OnMappingChanged(Dictionary<long, ColorMapping> colorMappings,Dictionary<long, ShapeMapping> shapeMappings )
    {
        this.hightlightMaterial.color = colorMappings[ColorMapping.HIGHLIGHTED.id].color;
        this.hiddenMaterial.color = colorMappings[ColorMapping.HIDDEN.id].color;

        VerticeShape newShape = VerticeShape.CUBE;  
        switch (this.verticeWrapper.verticeData.verticeType)
        {
            case VerticeType.Person:
                newShape= shapeMappings[ShapeMapping.PERSON.id].shape;
                break;
            case VerticeType.Ticket:
                newShape= shapeMappings[ShapeMapping.TICKET.id].shape;
                break;
            case VerticeType.File:
                newShape= shapeMappings[ShapeMapping.FILE.id].shape;
                break;
            case VerticeType.Wiki:
                newShape= shapeMappings[ShapeMapping.WIKI.id].shape;
                break;
            case VerticeType.RepoFile:
                newShape= shapeMappings[ShapeMapping.REPOFILE.id].shape;
                break;
            default:
                break;
        }

        if (newShape != shape)
        {
            shape = newShape;
            switch (shape)
            {
                case VerticeShape.CUBE:
                    this.meshFilter.mesh = cubeMesh;
                    break;
                case VerticeShape.SPHERE:
                    this.meshFilter.mesh = sphereMesh;
                    break;
                // case VerticeShape.CYLINDER:
                //     this.meshFilter.mesh = cylinderMesh;
                //     break;
                // case VerticeShape.CAPSULE:
                //     this.meshFilter.mesh = capsuleMesh;
                //     break;
            }
        }
        
        if (isLast) // Dont change color if it is last, we want it to be black
            return;
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
            case VerticeType.RepoFile:
                return SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.REPOFILE).color;
            default:
                return new Color32(0, 0, 0, 0);
        }
    }
    
    private VerticeShape GetShape()
    {
        switch (this.verticeWrapper.verticeData.verticeType)
        {
            case VerticeType.Person:
                return SingletonManager.Instance.preferencesManager.GetShapeMapping(ShapeMapping.PERSON).shape;
            case VerticeType.Ticket:
                return SingletonManager.Instance.preferencesManager.GetShapeMapping(ShapeMapping.TICKET).shape; ;
            case VerticeType.File:
                return SingletonManager.Instance.preferencesManager.GetShapeMapping(ShapeMapping.FILE).shape;
            case VerticeType.Wiki:
                return SingletonManager.Instance.preferencesManager.GetShapeMapping(ShapeMapping.WIKI).shape;
            case VerticeType.RepoFile:
                return SingletonManager.Instance.preferencesManager.GetShapeMapping(ShapeMapping.REPOFILE).shape;
            default:
                return VerticeShape.CUBE;
        }
    }

    public override void OnHoverEnter()
    {
        if (verticeWrapper == null || !isLoaded)
        {
            return;
        }

        string commitPart = (commitOrChange?.id ?? -1) <0 ? "" : ("["+commitOrChange?.verticeType+"]"+commitOrChange?.title+"+\n");
        hoverText.text = commitPart + "[" + verticeWrapper.verticeData.verticeType + "]" + verticeWrapper.verticeData.title;
        // hoverText.text = "Vertice ID: " + verticeWrapper.verticeData.id + " | Vertice: " + verticeWrapper.TmpGetDateNoHours() + " | Change ID: " + (commitOrChange?.id.ToString() ?? "???") + " | Change: " + (commitOrChange?.created ?? commitOrChange?.begin ?? DateTime.MinValue.Date);

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
        sidebarScript.slideOut(projectId, verticeWrapper);
        SingletonManager.Instance.dataManager.ProcessVerticeClick(new Pair<VerticeData, VerticeWrapper>(this.commitOrChange, this.verticeWrapper));
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

    public void SetVerticeData(VerticeWrapper verticeWrapper, long projectId, Material material, VerticeData changeOrCommit, Pair<DateTime, DateTime> pair)
    {
        this.verticeWrapper = verticeWrapper;
        this.projectId = projectId;
        this.commitOrChange = changeOrCommit;
        this.beforeDate = pair.Left;
        this.afterDate = pair.Right;

        isLast = (changeOrCommit?.changes ?? "").Contains("-> Closed");
        // We need to duplicate material because otherwise all objects with that material will be changed
        Material newMat = new Material(material);
        newMat.color = isLast ? Color.black : GetColor();
        meshRenderer.material = newMat;
        this.nonHoverMaterial = newMat;

        this.meshFilter.mesh = GetShape() == VerticeShape.CUBE ? cubeMesh : sphereMesh;
        Debug.Log("Setting "+GetShape().ToString());
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

    public bool ContainsDate(DateTime date)
    {
        if (isLast)
        {
            return (this.commitOrChange?.created ?? this.commitOrChange?.begin ?? DateTime.MaxValue).Date <= date;
        }

        return this.beforeDate < date && this.afterDate > date &&
               (this.commitOrChange?.created ?? this.commitOrChange?.begin ?? DateTime.MaxValue).Date <= date;
    }

}
