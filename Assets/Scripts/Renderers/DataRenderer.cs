using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Helpers;
using JetBrains.Annotations;
using PathologicalGames;
using Renderers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataRenderer : MonoBehaviour
{
    public Dictionary<long, DataHolder> loadedProjects = new();
    public Dictionary<long, int> dateIndexTracker = new(); // Tracks next date to render

    //Used to keep track of next spawn position
    public Dictionary<long, float> spawnTheta = new(); // <ProjectId,Theta> 
    public Dictionary<long, Vector3> commitPosTracker = new();
    public Dictionary<long, Vector3> wikiPosTracker = new();
    public Dictionary<long, Vector3> filePosTracker = new();
    public Dictionary<long, Vector3> repoPosTracker = new();

    // Instantiated objects
    private Dictionary<long, Dictionary<long, VerticeRenderer>> vertices = new();

    private Dictionary<long, Pair<float, float>> projectSizesZ = new(); //Keeping min and max Z sizes for a project
    private Dictionary<long, Pair<float, float>> projectSizesX = new(); //Keeping min and max X sizes for a project

    public Pair<DateTime, DateTime> dateFilter = new(DateTime.MinValue.Date,DateTime.MinValue.Date);
    
    
    
    
    // New variables
    public Dictionary<long,Dictionary<VerticeType, GameObject>> platforms = new();
    private Dictionary<long, Dictionary<long, List<VerticeRenderer>>> verticesWithEdges = new();
    private Dictionary<long, Dictionary<VerticeType,List<GameObject>>> verticePlatforms = new();
    private Dictionary<long, DateTime> currentProjectDate = new();
    private Dictionary<long, GameObject> datePlatformTrackers = new();
    private DateTime lastDate = DateTime.MinValue.Date;

    public Dictionary<long, Dictionary<long, Vector3>> linePosHolder = new();

    [Header("Properties")]

    public float renderDistanceBetweenObjs = 3;
    public float distanceOnComplete = 4f;
    public bool spawnTopOutlinesForSpiral = false;
    public long spaceBetweenWallObjs = 2;
    public long distanceFromMiddleGraph = 5;
    
    
    // New Properties
    [Header("New props")] 
    public long baseXPos = 0;
    public long baseZPos = 0;
    public long baseYPos = 0;
    public float platformHeight = 10f;
    public float platformDistanceBetween = 2f;
    public float spaceBetweenObjects = 1f;
    public byte platformAlpha = 100;
    public long helperDistanceFromGraph = 100;
    public float lineWidth = 2f;
    public long distanceBetweenProjects = 300;

    [Header("Prefabs")]
    public GameObject verticePrefab;
    public Material verticeMaterial;
    public GameObject platformPrefab;
    public GameObject clickablePlatformPrefab;
    public Material transparentMaterial;
    public GameObject arrowPrefab;
    public GameObject linePrefab;
    
    [Header("References")]

    public Canvas hoverCanvas;
    public GameObject hoverElement;
    public TMP_Text hoverText;

    public GameObject loadingBar;
    public GameObject loadBtn;

    public TMP_Text currentDateText;

    public CollabMatrix collabMatrix;

    public Material outlineMaterial;
    public LineRenderer lineRenderer;
    public SidebarController sidebarController;
    public TimelineRenderer timelineRenderer;

    public ContributionsCalendar contributionsCalendar;

    public KiviatDiagram kiviatDiagram;

    public FilterHolder filterHolder;

    public Button nextDateBtn;
    public Button previousDateBtn;
    public GameObject dateTrackerPrefab;

    private void Start()
    {
        SingletonManager.Instance.dataManager.DataFilterEvent += OnDataFilter;
        SingletonManager.Instance.dataManager.DateChangeEvent += OnDateChangeEvent;
        SingletonManager.Instance.dataManager.DateRenderChangedEvent += OnDateRenderChanged;
        SingletonManager.Instance.dataManager.ResetEvent += OnReset;
        SingletonManager.Instance.dataManager.VerticesSelectedEvent += OnVerticeSelected;
        filterHolder = new();
    }

    private void OnVerticeSelected(Pair<long,List<Pair<VerticeData,VerticeWrapper>>> pair)
    {
        PoolManager.Pools[PoolNames.LINES].DespawnAll();
        if(linePosHolder.ContainsKey(pair.Left))
            linePosHolder[pair.Left].Clear();

        foreach (Pair<VerticeData,VerticeWrapper> p in pair.Right)
        {
            if (p.Left == null)
                continue;
            foreach (var verticeData in p.Right.relatedChangesOrCommits[p.Left.id].GetRelatedVertices().Where(x=>x.id != p.Right.verticeData.id))
            {
                List<VerticeRenderer> from = verticesWithEdges[pair.Left][p.Right.verticeData.id]
                    .Where(x => (x.commitOrChange ?? p.Left).id == p.Left.id).ToList();

                List<VerticeRenderer> to = verticesWithEdges[pair.Left][verticeData.id]
                    .Where(x => (x.commitOrChange ?? p.Left).id == p.Left.id).ToList();

                if (from.Count == 0 || to.Count == 0)
                {
                    continue;
                }
                foreach (var f in from)
                {
                    foreach (var t in to)
                    {
                        CreatePath(f,t,pair.Left);
                    }
                }
            }
        }
    }

    private void CreatePath(VerticeRenderer from, VerticeRenderer to, long projectId)
    {
        Transform t = PoolManager.Pools[PoolNames.LINES].Spawn(linePrefab);
        LineRenderer lr = t.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        
        lr.SetPosition(0, from.transform.position);
        lr.SetPosition(1, to.transform.position);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
    }

    private void OnReset(ResetEventReason reason)
    {
        if (reason == ResetEventReason.CLICK_OUTSIDE)
        {
            if (dateFilter.Right != DateTime.MinValue.Date)
            {
                dateFilter = new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MinValue.Date);
                RenderAllDates();
            }
            PoolManager.Pools[PoolNames.LINES].DespawnAll();
            linePosHolder.Clear();
        }
    }
    
    private void OnDateRenderChanged(Pair<long, Pair<DateTime, DateTime>> pair)
    {
        lastDate = DateTime.MinValue.Date;
        currentProjectDate[pair.Left] = DateTime.MinValue.Date;
        nextDateBtn.interactable = true;
        previousDateBtn.interactable = true;

        foreach (var (key, value) in verticesWithEdges[pair.Left])
        {
            foreach (var verticeRenderer in value)
            {
                if (filterHolder.disabledVertices.Contains(verticeRenderer.verticeWrapper.verticeData.verticeType))
                {
                    verticeRenderer.gameObject.SetActive(false);
                    continue;
                }
                verticeRenderer.gameObject.SetActive(verticeRenderer.commitOrChange?.IsDateBetween(pair.Right.Left,pair.Right.Right) ?? true);
            }
        }

        datePlatformTrackers[pair.Left].transform.position = GetDefaultTrackerPlatformPos(pair.Left);
        datePlatformTrackers[pair.Left].gameObject.SetActive(false);
    }

    private void OnDateChangeEvent(long projectId, DateTime date)
    {
        foreach (var (key, value) in verticesWithEdges[projectId])
        {
            foreach (var verticeRenderer in value)
            {
                if (filterHolder.disabledVertices.Contains(verticeRenderer.verticeWrapper.verticeData.verticeType))
                {
                    verticeRenderer.gameObject.SetActive(false);
                    continue;
                }
                if(verticeRenderer.verticeWrapper.verticeData.verticeType != VerticeType.Person)
                    verticeRenderer.gameObject.SetActive(verticeRenderer.ContainsDate(date));
            }
        }
        currentProjectDate[projectId] = date;
        TimeSpan diff = lastDate != DateTime.MinValue.Date ? lastDate - currentProjectDate[projectId] : this.loadedProjects[projectId].startDate - currentProjectDate[projectId];
        int days = (int)diff.TotalDays;    
        Vector3 platformPos = datePlatformTrackers[projectId].transform.position;
        platformPos.z -= (1 + spaceBetweenObjects) * days;
        datePlatformTrackers[projectId].transform.position = platformPos;

        if (lastDate == DateTime.MinValue.Date)
        {
            datePlatformTrackers[projectId].gameObject.SetActive(true);
        }
        
        lastDate = date;
    }
    
    private void OnDataFilter(FilterHolder f)
    {
        this.filterHolder = f;
        
        // TODO move specific platforms up/down?
        foreach (var (projectId, ignored) in this.loadedProjects)
        {
                    
            foreach (var (key, value) in platforms[projectId])
            {
                value.SetActive(!f.disabledVertices.Contains(key));
            }
        
            foreach (var (key, value) in verticePlatforms[projectId])
            {
                if (f.disabledVertices.Contains(key))
                {
                    foreach (var o in value)
                    {
                        o.SetActive(false);
                    }
                }
                else
                {
                    foreach (var o in value)
                    {
                        o.SetActive(true);
                    }
                }
            }
        
            if (currentProjectDate[projectId] == DateTime.MinValue.Date)
            {
                RenderAllDates();
            }
            else
            {
                OnDateChangeEvent(projectId, currentProjectDate[projectId]);
            }
        }

    } 
    
    public void AddData(DataHolder dataHolder, bool rerender, bool renderFirst)
    {
        SetLoading(true);
        this.loadedProjects.Add(dataHolder.projectId, dataHolder);
        this.dateIndexTracker[dataHolder.projectId] = -1;
        
        RenderDataNew(dataHolder.projectId);
    }

    public void ResetData(bool resetAll)
    {
        this.spawnTheta.Clear();
        this.loadedProjects.Clear();
        PoolManager.Pools[PoolNames.VERTICE].DespawnAll(); // This removes all vertices from scene :)
        PoolManager.Pools[PoolNames.VERTICE_OUTLINE].DespawnAll(); // This removes all outline vertices from scene :)
        PoolManager.Pools[PoolNames.HIGHLIGHT_OBJECTS].DespawnAll(); // This removes all outline vertices from scene :)

        vertices = new();
        this.dateIndexTracker.Clear();
        commitPosTracker.Clear();
        wikiPosTracker.Clear();
        filePosTracker.Clear();
        repoPosTracker.Clear();
        
        projectSizesX.Clear();
        projectSizesZ.Clear();
        
        
        // New things
        platforms.Clear();
        currentProjectDate.Clear();

        if (resetAll)
            this.dateFilter = new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MinValue.Date);

        DOTween.Clear();
    }

    public void RenderDataNew(long projectId)
    {
        if (loadedProjects == null) return;
        SpawnVerticesAndEdges(projectId);
        SpawnDateTrackerPlatform(projectId);
        SpawnHelper(projectId);
        
        // Load viz techniques
        if (projectId == 1)
        {
            collabMatrix.fillMatrix(this.loadedProjects[projectId]);
            contributionsCalendar.fillContributionsCalendar(this.loadedProjects[projectId],
                this.loadedProjects[projectId].startDate.Year);
            timelineRenderer.LoadTimeline(this.loadedProjects[projectId]);
            kiviatDiagram.initiateKiviat(this.loadedProjects[projectId]);
        }

        SetLoading(false);
    }

    private void SpawnHelper(long projectId)
    {
        SpawnHelperPlatformText(projectId,0, "Person");
        SpawnHelperPlatformText(projectId,1, "Ticket");
        SpawnHelperPlatformText(projectId,2,"Repository file");
        SpawnHelperPlatformText(projectId,3,"File");
        SpawnHelperPlatformText(projectId,4,"Wiki");
        
        
        TimeSpan diff = this.loadedProjects[projectId].maxDate - this.loadedProjects[projectId].minDate;
        int projectDays = (int)diff.TotalDays;
        float platformWidth = projectDays*(spaceBetweenObjects+1);
        
        GameObject go = Instantiate(arrowPrefab);
        Vector3 from = new Vector3( - helperDistanceFromGraph, 0,  - helperDistanceFromGraph) + GetSpawnVector(projectId);
        Vector3 to = new Vector3( - helperDistanceFromGraph, 0,  - helperDistanceFromGraph+ platformWidth) + GetSpawnVector(projectId);
        go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f,"Changes",Direction.LEFT);
        
                
        go = Instantiate(arrowPrefab);
        from = new Vector3(- helperDistanceFromGraph, 0, - helperDistanceFromGraph) + GetSpawnVector(projectId);
         to = new Vector3(- helperDistanceFromGraph+platformWidth, 0, - helperDistanceFromGraph) + GetSpawnVector(projectId);
        go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f,"Elements",Direction.RIGHT);
    }

    private void SpawnHelperPlatformText(long projectId, int index, string text)
    {
        GameObject go = Instantiate(arrowPrefab);
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        float height = platformHeight + platformDistanceBetween;
        Vector3 from = new Vector3(- helperDistanceFromGraph, 0 -index*height, - helperDistanceFromGraph) + GetSpawnVector(projectId);
        Vector3 to = new Vector3(- helperDistanceFromGraph, 0 - height - index*height, - helperDistanceFromGraph)+ GetSpawnVector(projectId);
        go.GetComponent<ArrowRenderer>().SetUpNoArrows(from, to, 2f,text,Direction.DOWN);
    }

    private Vector3 GetSpawnVector(long projectId)
    {
        Vector3 vector = new Vector3(baseXPos, baseYPos, baseZPos);

        for (int i = 1; i < projectId; i++)
        {
            TimeSpan diff = this.loadedProjects[i].maxDate - this.loadedProjects[i].minDate;
            int projectDays = (int)diff.TotalDays;
            float platformWidth = projectDays * (spaceBetweenObjects + 1);
            vector.z += platformWidth + distanceBetweenProjects;
        }

        return vector;
    }
    
    public void SpawnPlatform(int index, VerticeType type, long projectId, float platformWidth, int alreadySpawnedCount)
    {
        Color32 color = SingletonManager.Instance.preferencesManager.GetColorMappingByType(type).color;
        color.a = platformAlpha;
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount - alreadySpawnedCount ) * (1 + spaceBetweenObjects);
        
        Vector3 pos = new Vector3( (alreadySpawnedCount)* (1 + spaceBetweenObjects)+ maxCount/2f -1f,  (-index) * platformDistanceBetween,  platformWidth/2f) + GetSpawnVector(projectId);

        GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity);
        platform.transform.localScale = new Vector3(maxCount+1, platformHeight, platformWidth+1f);
        
        Material newMat = new Material(transparentMaterial);
        newMat.color = color;
        platform.GetComponent<MeshRenderer>().material = newMat;

        if (!platforms.ContainsKey(projectId))
        {
            platforms[projectId] = new();
        }

        platforms[projectId][type] = platform;
    }
    
    public void SpawnVerticePlatform(int index, VerticeType type, long projectId, float platformWidth, Vector3 platformPos, VerticeWrapper v)
    {
        Color32 color = SingletonManager.Instance.preferencesManager.GetColorMappingByType(type).color;
        color.a = platformAlpha;
        if (index % 2 == 1)
        {
            color.r = (byte)Math.Clamp(color.r * 1.25f,0,255);
            color.g = (byte)Math.Clamp(color.g * 1.25f,0,255);
            color.b = (byte)Math.Clamp(color.b * 1.25f,0,255);
            color.a = (byte)Math.Clamp(color.a * 1.25f,0,255);
        }
        GameObject platform = Instantiate(clickablePlatformPrefab, platformPos, Quaternion.identity);
        platform.transform.localScale = new Vector3(spaceBetweenObjects+1f, platformHeight, platformWidth+1f);
        
        Material newMat = new Material(transparentMaterial);
        newMat.color = color;
        platform.GetComponent<MeshRenderer>().material = newMat;
        platform.GetComponent<VerticePlatformRenderer>().SetUp(projectId,v);

        if (!verticePlatforms.ContainsKey(projectId))
        {
            verticePlatforms[projectId] = new();
        }

        if (!verticePlatforms[projectId].ContainsKey(type))
        {
            verticePlatforms[projectId][type] = new();
        }
        verticePlatforms[projectId][type].Add(platform);
    }

    public void SpawnVerticesAndEdges(long projectId)
    {
        if (!currentProjectDate.ContainsKey(projectId))
        {
            currentProjectDate[projectId] = DateTime.MinValue.Date;
        }
        
        
        float datePosAdd = (spaceBetweenObjects+1);
        
        TimeSpan diff = this.loadedProjects[projectId].maxDate - this.loadedProjects[projectId].minDate;
        int projectDays = (int)diff.TotalDays;
        
        float platformWidth = projectDays*(spaceBetweenObjects+1);
        
        SpawnVerticeAndEdges(0,VerticeType.Person,projectId,datePosAdd,platformWidth);
        SpawnVerticeAndEdges(1,VerticeType.Ticket,projectId,datePosAdd,platformWidth);
        SpawnVerticeAndEdges(2,VerticeType.RepoFile,projectId,datePosAdd,platformWidth);
        SpawnVerticeAndEdges(3,VerticeType.File,projectId,datePosAdd,platformWidth);
        SpawnVerticeAndEdges(4,VerticeType.Wiki,projectId,datePosAdd,platformWidth);
    }

    public void SpawnDateTrackerPlatform(long projectId)
    {
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        
        Color32 color = Color.magenta;
        color.a = (byte)(platformAlpha*2);

        float height = platformHeight * 4 - (4) * platformDistanceBetween;

        GameObject platform = Instantiate(dateTrackerPrefab, GetDefaultTrackerPlatformPos(projectId), Quaternion.identity);
        platform.transform.localScale = new Vector3(maxCount+1, -height, 1f);
        
        Material newMat = new Material(transparentMaterial);
        newMat.color = color;
        platform.GetComponent<MeshRenderer>().material = newMat;

        platform.SetActive(false);
        this.datePlatformTrackers[projectId] = platform;
    }

    private Vector3 GetDefaultTrackerPlatformPos(long projectId)
    {
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        float height = platformHeight * 4 - (4) * platformDistanceBetween;
        return new Vector3(maxCount / 2 -1,  height / 2f - platformDistanceBetween / 2f, 0) + GetSpawnVector(projectId);
    }

    public void SpawnVerticeAndEdges(int index, VerticeType type, long projectId, float datePosAdd, float platformWidth)
    {
        List<List<Pair<VerticeData, VerticeWrapper>>>
            list = this.loadedProjects[projectId].GetVerticesForPlatform(type);


        for (int zIndex = 0; zIndex < list.Count; zIndex++)
        {
            List<Pair<VerticeData, VerticeWrapper>> changes = list[zIndex];
            if (changes.Count == 0)
                continue;
            for (int xIndex = 0; xIndex < changes.Count; xIndex++)
            {
                Pair<VerticeData, VerticeWrapper> change = changes[xIndex];
                float xPos;
                if (type == VerticeType.Person)
                {
                    TimeSpan diff = this.loadedProjects[projectId].maxDate - this.loadedProjects[projectId].minDate;
                    int daysFromStart = (int)diff.TotalDays;
                    xPos = (daysFromStart * 1.0f) * datePosAdd / 2f;
                }
                else
                {
                    DateTime date = change.Left.created ?? change.Left.begin ?? this.loadedProjects[projectId].minDate;
                    TimeSpan diff = date - this.loadedProjects[projectId].minDate;
                    int daysFromStart = (int)diff.TotalDays;
                    xPos = (daysFromStart * 1.0f) * datePosAdd;
                }

                if (type == VerticeType.Person)
                {
                    Transform t = SpawnVerticeEdge(projectId, new Vector3(
                                                                  +this.loadedProjects[projectId].maxVerticeCount *
                                                                  (1 + spaceBetweenObjects) / 2f + -5f,
                                                                  (-index) * platformDistanceBetween,
                                                                  zIndex * (1 + spaceBetweenObjects) * 10 + 5f) +
                                                              GetSpawnVector(projectId), change.Right, change.Left,
                        new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MaxValue.Date));
                    t.localScale = new Vector3(10, 10, 10);
                }
                else
                {
                    SpawnVerticeEdge(projectId, new Vector3(zIndex * (1 + spaceBetweenObjects),
                            (-index) * platformDistanceBetween, xPos) + GetSpawnVector(projectId), change.Right,
                        change.Left,
                        GetDatetimeForPos(changes, xIndex));
                }
            }

            if (type != VerticeType.Person)
            {
                SpawnVerticePlatform(zIndex, type, projectId, platformWidth, new Vector3(
                        zIndex * (1 + spaceBetweenObjects),
                        (-index) * platformDistanceBetween, platformWidth / 2f) + GetSpawnVector(projectId),
                    changes[0].Right);
            }
        }

        if (list.Count < this.loadedProjects[1].maxVerticeCount && type != VerticeType.Person)
        {
            SpawnPlatform(index,type,projectId,platformWidth,list.Count);
        }
    }

    private Pair<DateTime, DateTime> GetDatetimeForPos(List<Pair<VerticeData, VerticeWrapper>> changes, int xIndex)
    {
        Pair<DateTime, DateTime> pair = new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MaxValue.Date);
        DateTime current = (changes[xIndex].Left.created ??
                           changes[xIndex].Left.begin ?? DateTime.MinValue).Date;

        int tracker = xIndex - 1;    
        while (tracker > 0)
        {
            DateTime date = (changes[tracker].Left.created ??
                            changes[tracker].Left.begin ?? DateTime.MinValue).Date;

            if (date == current)
            {
                tracker--;
            }
            else
            {
                pair.Left = date;
                break;
            }
        }
        
        tracker = xIndex + 1;    
        while (tracker < changes.Count-1)
        {
            DateTime date = (changes[tracker].Left.created ??
                            changes[tracker].Left.begin ?? DateTime.MinValue).Date;

            if (date == current)
            {
                tracker++;
            }
            else
            {
                pair.Right = date;
                break;
            }
        }

        return pair;
    }
    
    private Transform SpawnVerticeEdge(long projectId, Vector3 spawnPos, VerticeWrapper verticeWrapper, VerticeData changeOrCommit, Pair<DateTime, DateTime> pair)
    {
        Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(verticePrefab, spawnPos, Quaternion.identity);

        VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
        verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText, sidebarController, this);
        verticeRenderer.SetVerticeData(verticeWrapper, projectId, verticeMaterial, changeOrCommit, pair);
        if (!verticesWithEdges.ContainsKey(projectId))
        {
            verticesWithEdges[projectId] = new();
        }

        if (!verticesWithEdges[projectId].ContainsKey(verticeWrapper.verticeData.id))
        {
            verticesWithEdges[projectId][verticeWrapper.verticeData.id] = new();
        }
        verticesWithEdges[projectId][verticeWrapper.verticeData.id].Add(verticeRenderer);
        // CheckAndApplyHighlight(projectId, verticeId); //TODO return
        verticeRenderer.SetIsLoaded(true);
        return obj;
    }

    // Called from UI
    public void RenderNextDate()
    {
        foreach (var (key, value) in this.loadedProjects)
        {
            RenderNextDateForProject(key);
        }
    }
    
    public void RenderNextDateForProject(long projectId)
    {
        previousDateBtn.interactable = true;
        if (currentProjectDate[projectId] >= loadedProjects[projectId].maxDate)
        {
            return;
        }

        DateTime newDate =
            loadedProjects[projectId].dates.Where(x => x > currentProjectDate[projectId]).Min();
        SingletonManager.Instance.dataManager.InvokeDateChangedEvent(projectId, newDate);
        
        
        if (currentProjectDate[projectId] >= loadedProjects[projectId].maxDate)
        {
            nextDateBtn.interactable =false;
        }
    }
    
    // Called from UI
    public void RenderPreviousDate()
    {
        foreach (var (key, value) in this.loadedProjects)
        {
            RenderPreviousDateForProject(key);
        }
    }
    
    public void RenderPreviousDateForProject(long projectId)
    {
        nextDateBtn.interactable = true;
        if (currentProjectDate[projectId] <= loadedProjects[projectId].minDate)
        {
            return;
        }

        DateTime newDate = loadedProjects[projectId].dates.Where(x => x < currentProjectDate[projectId]).Max();
        SingletonManager.Instance.dataManager.InvokeDateChangedEvent(projectId, newDate);
        if (currentProjectDate[projectId] <= loadedProjects[projectId].minDate)
        {
            previousDateBtn.interactable = false;
        }
    }

    public void RenderAllDates()
    {
        foreach (var (key, value) in this.loadedProjects)
        {
            ShowAllDatesForProject(key);
        }
    }

    public void ShowAllDatesForProject(long projectId)
    {
        lastDate = DateTime.MinValue.Date;
        currentProjectDate[projectId] = DateTime.MinValue.Date;
        nextDateBtn.interactable = true;
        previousDateBtn.interactable = true;

        foreach (var (key, value) in verticesWithEdges[projectId])
        {
            foreach (var verticeRenderer in value)
            {
                if (filterHolder.disabledVertices.Contains(verticeRenderer.verticeWrapper.verticeData.verticeType))
                {
                    verticeRenderer.gameObject.SetActive(false);
                    continue;
                }
                verticeRenderer.gameObject.SetActive(true);
            }
        }

        datePlatformTrackers[projectId].transform.position = GetDefaultTrackerPlatformPos(projectId);
        datePlatformTrackers[projectId].gameObject.SetActive(false);

    }
    
    // Manages loading screen
    public void SetLoading(bool status)
    {
        SingletonManager.Instance.pauseManager.SetInteractionPaused(status);
        loadingBar.SetActive(status);
        loadBtn.SetActive(!status);
    }
    
    public bool HasActiveDateFilter()
    {
        return this.dateFilter.Left != DateTime.MinValue.Date && this.dateFilter.Right != DateTime.MinValue.Date;
    }

    // public void CheckAndApplyHighlight(long projectId, long verticeId)
    // {
    //     if (!loadedProjects.ContainsKey(projectId) || !this.vertices.ContainsKey(projectId) || !this.vertices[projectId].ContainsKey(verticeId))
    //         return;
    //     // Check for active highlights
    //     if (SingletonManager.Instance.dataManager.selectedDates.Count > 0)
    //     {
    //         if (this.loadedProjects[projectId].verticeWrappers[verticeId].ContainsDate(SingletonManager.Instance.dataManager.selectedDates))
    //         {
    //             this.vertices[projectId][verticeId].SetHighlighted(true);
    //         }
    //         else
    //         {
    //             this.vertices[projectId][verticeId].SetHidden(true);
    //         }
    //     }
    //     else if (SingletonManager.Instance.dataManager.selectedVertices.Count > 0)
    //     {
    //         if (SingletonManager.Instance.dataManager.selectedVertices.Contains(this.loadedProjects[projectId].verticeWrappers[verticeId]))
    //         {
    //             this.vertices[projectId][verticeId].SetHighlighted(true);
    //         }
    //         else
    //         {
    //             this.vertices[projectId][verticeId].SetHidden(true);
    //         }
    //     }
    // }
}