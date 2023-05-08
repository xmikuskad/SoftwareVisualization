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
    private Dictionary<long, Dictionary<long,List<GameObject>>> verticePlatforms = new();
    private Dictionary<long, DateTime> currentProjectDate = new();
    private Dictionary<long, GameObject> datePlatformTrackers = new();

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

    
    [Header("Prefabs")]
    public GameObject verticePrefab;
    public Material verticeMaterial;
    public GameObject platformPrefab;
    public GameObject clickablePlatformPrefab;
    public Material transparentMaterial;
    public GameObject arrowPrefab;
    
    
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

    private FilterHolder filterHolder;

    public Button nextDateBtn;
    public Button previousDateBtn;
    public GameObject dateTrackerPrefab;

    private void Start()
    {
        SingletonManager.Instance.dataManager.DataFilterEvent += OnDataFilter; //TODO fix
        SingletonManager.Instance.dataManager.DateChangeEvent += OnDateChangeEvent;
        filterHolder = new();
    }

    private void OnDateChangeEvent(long projectId, DateTime date)
    {
        foreach (var (key, value) in verticesWithEdges[projectId])
        {
            foreach (var verticeRenderer in value)
            {
                if(verticeRenderer.verticeWrapper.verticeData.verticeType != VerticeType.Person)
                    verticeRenderer.gameObject.SetActive(verticeRenderer.ContainsDate(date));
            }
        }
    }
    
    private void OnDataFilter(FilterHolder f)
    {
        this.filterHolder = f;
        long projectId = 1L;
        DateTime date = dateFilter.Right != DateTime.MinValue.Date
            ? dateFilter.Right
            : this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]];
        RerenderProject(projectId, false);
        RenderUntilDate(date,1L);
    } 
    
    public void AddData(DataHolder dataHolder, bool rerender, bool renderFirst)
    {
        SetLoading(true);
        this.loadedProjects.Add(dataHolder.projectId, dataHolder);
        this.dateIndexTracker[dataHolder.projectId] = -1;
        spawnTheta[dataHolder.projectId] = renderDistanceBetweenObjs;
        // RenderData(rerender,renderFirst);
        
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
        collabMatrix.fillMatrix(this.loadedProjects[projectId]);
        contributionsCalendar.fillContributionsCalendar(this.loadedProjects[projectId], this.loadedProjects[projectId].startDate.Year);
        timelineRenderer.LoadTimeline(this.loadedProjects[projectId]);
        kiviatDiagram.initiateKiviat(this.loadedProjects[projectId]);
        
        SetLoading(false);
    }

    private void SpawnHelper(long projectId)
    {
        // GameObject go = Instantiate(arrowPrefab);
        
        
        // float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        // float height = platformHeight * 5 - (5) * platformDistanceBetween;
        // Vector3 from = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos, baseZPos - helperDistanceFromGraph);
        // Vector3 to = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos + height, baseZPos - helperDistanceFromGraph);
        // go.GetComponent<ArrowRenderer>().SetUpNoArrows(from, to, 2f,"Person",Direction.DOWN);
        
        // float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        // float height = platformHeight * 5 - (5) * platformDistanceBetween;
        // Vector3 from = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos, baseZPos - helperDistanceFromGraph);
        // Vector3 to = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos + height, baseZPos - helperDistanceFromGraph);
        // go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f,"Person",Direction.DOWN);
        
        SpawnHelperPlatformText(projectId,0, "Person");
        SpawnHelperPlatformText(projectId,1, "Ticket");
        SpawnHelperPlatformText(projectId,2,"Repository file");
        SpawnHelperPlatformText(projectId,3,"File");
        SpawnHelperPlatformText(projectId,4,"Wiki");
        
        
        TimeSpan diff = this.loadedProjects[projectId].maxDate - this.loadedProjects[projectId].minDate;
        int projectDays = (int)diff.TotalDays;
        float platformWidth = projectDays*(spaceBetweenObjects+1);
        
        GameObject go = Instantiate(arrowPrefab);
        Vector3 from = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos, baseZPos - helperDistanceFromGraph);
        Vector3 to = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos, baseZPos - helperDistanceFromGraph+ platformWidth);
        go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f,"Changes",Direction.LEFT);
        
                
        go = Instantiate(arrowPrefab);
        from = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos, baseZPos - helperDistanceFromGraph);
         to = new Vector3(baseXPos - helperDistanceFromGraph+platformWidth, baseYPos, baseZPos - helperDistanceFromGraph);
        go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f,"Elements",Direction.RIGHT);
    }

    private void SpawnHelperPlatformText(long projectId, int index, string text)
    {
        GameObject go = Instantiate(arrowPrefab);
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        float height = platformHeight + platformDistanceBetween;
        Vector3 from = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos -index*height, baseZPos - helperDistanceFromGraph);
        Vector3 to = new Vector3(baseXPos - helperDistanceFromGraph, baseYPos - height - index*height, baseZPos - helperDistanceFromGraph);
        go.GetComponent<ArrowRenderer>().SetUpNoArrows(from, to, 2f,text,Direction.DOWN);
    }

    
    public void SpawnPlatform(int index, VerticeType type, long projectId, float platformWidth, int alreadySpawnedCount)
    {
        Color32 color = SingletonManager.Instance.preferencesManager.GetColorMappingByType(type).color;
        color.a = platformAlpha;
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount - alreadySpawnedCount ) * (1 + spaceBetweenObjects);

        Vector3 pos = new Vector3(baseXPos + (alreadySpawnedCount)* (1 + spaceBetweenObjects)+ maxCount/2f -1f, baseYPos + (-index) * platformDistanceBetween + platformHeight/2f -1f, baseZPos + platformWidth/2f -1f);

        GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity);
        platform.transform.localScale = new Vector3(maxCount+1, platformHeight, platformWidth+1);
        
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
        platform.transform.localScale = new Vector3(spaceBetweenObjects+1f, platformHeight, platformWidth);
        
        Material newMat = new Material(transparentMaterial);
        newMat.color = color;
        platform.GetComponent<MeshRenderer>().material = newMat;

        platform.GetComponent<VerticePlatformRenderer>().SetUp(projectId,v);
    }

    public void SpawnVerticesAndEdges(long projectId)
    {
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

        this.datePlatformTrackers[projectId] = platform;
    }

    private Vector3 GetDefaultTrackerPlatformPos(long projectId)
    {
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        float height = platformHeight * 4 - (4) * platformDistanceBetween;
        return new Vector3(baseXPos + maxCount / 2 -1, baseYPos + height / 2f - platformDistanceBetween / 2f, baseZPos);
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
                    // Transform t = SpawnVerticeEdge(projectId, new Vector3(baseXPos + zIndex * (1 + spaceBetweenObjects) * 10 -5f,
                    //     baseYPos + (-index) * platformDistanceBetween -5f, baseZPos + xPos -5f), change.Right, change.Left);
                    
                    Transform t = SpawnVerticeEdge(projectId, new Vector3(baseXPos +this.loadedProjects[projectId].maxVerticeCount * (1 + spaceBetweenObjects)/2f+ -5f,
                        baseYPos + (-index) * platformDistanceBetween, baseZPos + zIndex * (1 + spaceBetweenObjects) * 10 +5f), change.Right, change.Left, new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MaxValue.Date));
                    t.localScale = new Vector3(10, 10, 10);
                }
                else
                {
                    SpawnVerticeEdge(projectId, new Vector3(baseXPos + zIndex * (1 + spaceBetweenObjects),
                        baseYPos + (-index) * platformDistanceBetween, baseZPos + xPos), change.Right, change.Left,
                        GetDatetimeForPos(changes,xIndex));
                }
            }

            if (type != VerticeType.Person)
            {
                SpawnVerticePlatform(zIndex, type,projectId,platformWidth,new Vector3(baseXPos + zIndex * (1 + spaceBetweenObjects),
                    baseYPos + (-index) * platformDistanceBetween, baseZPos  + platformWidth/2f), changes[0].Right);
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
        // TODO foreach project?
        RenderNextDateForProject(1L);
    }
    
    public void RenderNextDateForProject(long projectId)
    {
        previousDateBtn.interactable = true;
        if (!currentProjectDate.ContainsKey(projectId))
        {
            currentProjectDate[projectId] = DateTime.MinValue.Date;
        }

        if (currentProjectDate[projectId] >= loadedProjects[projectId].maxDate)
        {
            return;
        }

        DateTime newDate =
            loadedProjects[projectId].dates.Where(x => x > currentProjectDate[projectId]).Min();
        
        TimeSpan diff = currentProjectDate[projectId] != DateTime.MinValue.Date ? newDate - currentProjectDate[projectId] : newDate - newDate;
        int days = (int)diff.TotalDays;    
        Vector3 platformPos = datePlatformTrackers[projectId].transform.position;
        platformPos.z += (1 + spaceBetweenObjects)*days;
        datePlatformTrackers[projectId].transform.position = platformPos;
        
        currentProjectDate[projectId] =newDate;

        SingletonManager.Instance.dataManager.InvokeDateChangedEvent(projectId, currentProjectDate[projectId]);
        
        
        if (currentProjectDate[projectId] >= loadedProjects[projectId].maxDate)
        {
            nextDateBtn.interactable =false;
        }
    }
    
    // Called from UI
    public void RenderPreviousDate()
    {
        // TODO foreach project?
        RenderPreviousDateForProject(1L);
    }
    
    public void RenderPreviousDateForProject(long projectId)
    {
        nextDateBtn.interactable = true;
        if (!currentProjectDate.ContainsKey(projectId))
        {
            currentProjectDate[projectId] = DateTime.MinValue.Date;
        }

        if (currentProjectDate[projectId] <= loadedProjects[projectId].minDate)
        {
            return;
        }

        DateTime newDate = loadedProjects[projectId].dates.Where(x => x < currentProjectDate[projectId]).Max();
        
        TimeSpan diff = currentProjectDate[projectId] - newDate;
        int days = (int)diff.TotalDays;    
        Vector3 platformPos = datePlatformTrackers[projectId].transform.position;
        platformPos.z -= (1 + spaceBetweenObjects)*days;
        datePlatformTrackers[projectId].transform.position = platformPos;
        
        currentProjectDate[projectId] = newDate;
        
        SingletonManager.Instance.dataManager.InvokeDateChangedEvent(projectId, currentProjectDate[projectId]);
        if (currentProjectDate[projectId] <= loadedProjects[projectId].minDate)
        {
            previousDateBtn.interactable = false;
        }
    }

    public void RenderAllDates()
    {
        ShowAllDatesForProject(1L);
    }
    public void ShowAllDatesForProject(long projectId)
    {
        currentProjectDate.Clear();
        nextDateBtn.interactable = true;
        previousDateBtn.interactable = true;
        
        foreach (var (key, value) in verticesWithEdges[projectId])
        {
            foreach (var verticeRenderer in value)
            {
                verticeRenderer.gameObject.SetActive(true);
            }
        }

        datePlatformTrackers[projectId].transform.position = GetDefaultTrackerPlatformPos(projectId);
    }
    
    /*
     * OLD CODE BELOW
     *
     * 
     */
    
    public void RenderData(bool rerender, bool renderFirst)
    {
        if (loadedProjects == null) return;
        SpawnPeople(1L);
        SpawnOutlineObjects(1L);
        SpawnStartObjects(1L);
        collabMatrix.fillMatrix(this.loadedProjects[1]);
        contributionsCalendar.fillContributionsCalendar(this.loadedProjects[1], this.loadedProjects[1].startDate.Year);
        timelineRenderer.LoadTimeline(this.loadedProjects[1]);
        kiviatDiagram.initiateKiviat(this.loadedProjects[1]);
        SetLoading(false);
        if(renderFirst)
            RenderNext(1L);
    }

    private void SpawnStartObjects(long projectId)
    {
        SpawnByType(this.loadedProjects[projectId].spawnAtStart.Select(x=>x.verticeData).ToList(),projectId);
    }

    public void RerenderProject(long projectId, bool renderFirst)
    {
        DataHolder holder = this.loadedProjects[projectId];
        ResetData(renderFirst);
        AddData(holder, false, renderFirst);
    }

    // Manages loading screen
    public void SetLoading(bool status)
    {
        SingletonManager.Instance.pauseManager.SetInteractionPaused(status);
        loadingBar.SetActive(status);
        loadBtn.SetActive(!status);
    }

    private void RenderDate(DateTime dateTime, long projectId)
    {
        currentDateText.text = dateTime.ToString("dd/MM/yyyy") + " (Day " + (dateTime.Subtract(loadedProjects[projectId].startDate).Days + 1) + ")";
        foreach (var verticeWrapper in this.loadedProjects[projectId].changesByDate[dateTime])
        {
            Dictionary<VerticeType,List<VerticeData>> dict = verticeWrapper.GetRelatedVerticesDict();

            // We can skip person, we already spawned them
            if(dict.ContainsKey(VerticeType.Ticket))
                SpawnByType(dict[VerticeType.Ticket],projectId);
            if(dict.ContainsKey(VerticeType.Wiki))
                SpawnByType(dict[VerticeType.Wiki],projectId);
            if(dict.ContainsKey(VerticeType.RepoFile))
                SpawnByType(dict[VerticeType.RepoFile],projectId);
            if(dict.ContainsKey(VerticeType.File))
                SpawnByType(dict[VerticeType.File],projectId);
            if(dict.ContainsKey(VerticeType.Commit))
                SpawnByType(dict[VerticeType.Commit],projectId);
        }
    }

    public void RenderComparisionForDate(DateTime dateTime, long projectId)
    {
        // TODO comparisions??
    }

    public void RenderUntilDateWithReset(DateTime dateTime, long projectId)
    {
        RerenderProject(projectId, false);
        RenderUntilDate(dateTime,projectId);
    }

    public void RenderUntilDate(DateTime dateTime, long projectId)
    {
        this.dateIndexTracker[projectId] = this.loadedProjects[projectId].orderedDates.IndexOf(dateTime);
        for (var i = 0; i < this.loadedProjects[projectId].orderedDates.Count; i++)
        {
            DateTime date = this.loadedProjects[projectId].orderedDates[i];
            if (dateFilter.Left != DateTime.MinValue.Date && dateFilter.Right != DateTime.MaxValue)
            {
                if (date > dateTime) return;
                RenderDate(date, projectId);
            }
        }
    }

    public void RenderNext(long projectId)
    {
        if (dateIndexTracker[projectId] <= -2 ||
            this.loadedProjects[projectId].orderedDates.Count <= dateIndexTracker[projectId]) return;
        dateIndexTracker[projectId] += 1;

        RenderDate(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]], projectId);
        if (this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]] == DateTime.MinValue.Date)
        {
            RenderNext(projectId);
            return;
        }

        timelineRenderer.SetCurrentDate(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]],
            projectId);

    }

    public void RenderPrevious(long projectId)
    {
        if (dateIndexTracker[projectId] <= 0) return;
        dateIndexTracker[projectId] -= 1;
        RenderUntilDateWithReset(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]],projectId);
        timelineRenderer.SetCurrentDate(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]], projectId);
    }
    
    public void RenderNextBtn()
    {
        RenderNext(1L);
    }
    
    public void RenderPreviousBtn()
    {
        RenderPrevious(1L);
    }

    public bool HasActiveDateFilter()
    {
        return this.dateFilter.Left != DateTime.MinValue.Date && this.dateFilter.Right != DateTime.MinValue.Date;
    }

    private void SpawnByType(List<VerticeData> verticeDatas, long projectId)
    {
        foreach (var verticeData in verticeDatas)
        {
            if (filterHolder.disabledVertices.Contains(verticeData.verticeType))
                continue;
            switch (verticeData.verticeType)
            {
                case VerticeType.Ticket:
                    if (!vertices[projectId].ContainsKey(verticeData.id))
                    {
                        float idk = spawnTheta[projectId];
                        float x = Mathf.Cos(idk) * idk;
                        float y = Mathf.Sin(idk) * idk;
                        Transform obj = SpawnGeneralVertice(projectId, new Vector3(x, 0, y), verticeData.id,-1);
                        spawnTheta[projectId] += renderDistanceBetweenObjs / idk;
                    }
                    else
                    {
                        VerticeRenderer ver = vertices[projectId][verticeData.id];
                        float distanceToMove = distanceOnComplete / ver.verticeWrapper.updateCount;
                        ver.AddCompletedEdge(1L, ver.verticeWrapper.updateCount);
                        Vector3 newPos = ver.transform.position - new Vector3(0, distanceToMove, 0);
                        ver.transform.position = newPos;
                    }

                    break;
                case VerticeType.Commit:
                    if (!vertices[projectId].ContainsKey(verticeData.id))
                    {
                        if (commitPosTracker.ContainsKey(projectId))
                        {
                            if (commitPosTracker[projectId].x > projectSizesX[projectId].Right)
                            {
                                Vector3 pos = commitPosTracker[projectId];
                                pos.z -= spaceBetweenWallObjs;
                                pos.x = projectSizesX[projectId].Left;
                                commitPosTracker[projectId] = pos;
                            }
                            else
                            {
                                Vector3 pos = commitPosTracker[projectId];
                                pos.x += spaceBetweenWallObjs;
                                commitPosTracker[projectId] = pos;
                            }
                        }
                        else
                        {
                            commitPosTracker[projectId] = new Vector3(projectSizesX[projectId].Left, -(distanceFromMiddleGraph + distanceOnComplete),
                                projectSizesZ[projectId].Right);
                        }
                        Transform obj = SpawnGeneralVertice(projectId, commitPosTracker[projectId], verticeData.id,-1);
                    }
                    
                    break;
                case VerticeType.Wiki:
                    if (!vertices[projectId].ContainsKey(verticeData.id))
                    {
                        if (wikiPosTracker.ContainsKey(projectId))
                        {
                            if (wikiPosTracker[projectId].z > projectSizesZ[projectId].Right)
                            {
                                Vector3 pos = wikiPosTracker[projectId];
                                pos.y -= spaceBetweenWallObjs;
                                pos.z = projectSizesZ[projectId].Left;
                                wikiPosTracker[projectId] = pos;
                            }
                            else
                            {
                                Vector3 pos = wikiPosTracker[projectId];
                                pos.z += spaceBetweenWallObjs;
                                wikiPosTracker[projectId] = pos;
                            }
                        }
                        else
                        {
                            wikiPosTracker[projectId] = new Vector3(projectSizesX[projectId].Left-(distanceFromMiddleGraph), 0,
                                projectSizesZ[projectId].Left);
                        }
                        Transform obj = SpawnGeneralVertice(projectId, wikiPosTracker[projectId], verticeData.id,-1);
                    }

                    break;
                
                case VerticeType.RepoFile:
                    if (!vertices[projectId].ContainsKey(verticeData.id))
                    {
                        if (repoPosTracker.ContainsKey(projectId))
                        {
                            if (repoPosTracker[projectId].x > projectSizesX[projectId].Right)
                            {
                                Vector3 pos = repoPosTracker[projectId];
                                pos.y += spaceBetweenWallObjs;
                                pos.x = projectSizesX[projectId].Left;
                                repoPosTracker[projectId] = pos;
                            }
                            else
                            {
                                Vector3 pos = repoPosTracker[projectId];
                                pos.x += spaceBetweenWallObjs;
                                repoPosTracker[projectId] = pos;
                            }
                        }
                        else
                        {
                            repoPosTracker[projectId] = new Vector3(projectSizesX[projectId].Left, -(distanceOnComplete),
                                projectSizesZ[projectId].Right+(distanceFromMiddleGraph));
                        }
                        Transform obj = SpawnGeneralVertice(projectId, repoPosTracker[projectId], verticeData.id,-1);
                    }

                    break;
                case VerticeType.File:
                    if (!vertices[projectId].ContainsKey(verticeData.id))
                    {
                        if (filePosTracker.ContainsKey(projectId))
                        {
                            if (filePosTracker[projectId].z > projectSizesZ[projectId].Right)
                            {
                                Vector3 pos = filePosTracker[projectId];
                                pos.y -= spaceBetweenWallObjs;
                                pos.z = projectSizesZ[projectId].Left;
                                filePosTracker[projectId] = pos;
                            }
                            else
                            {
                                Vector3 pos = filePosTracker[projectId];
                                pos.z += spaceBetweenWallObjs;
                                filePosTracker[projectId] = pos;
                            }
                        }
                        else
                        {
                            filePosTracker[projectId] = new Vector3(projectSizesX[projectId].Right+(distanceFromMiddleGraph), 0,
                                projectSizesZ[projectId].Left);
                        }
                        Transform obj = SpawnGeneralVertice(projectId, filePosTracker[projectId], verticeData.id,-1);
                    }

                    break;
                case VerticeType.Change:
                case VerticeType.Person:
                    break;
                default:
                    break;
            }    
        }
    }


    private void SpawnPeople(long projectId)
    {
        if (filterHolder.disabledVertices.Contains(VerticeType.Person))
            return;
        float counter = spaceBetweenWallObjs;

        foreach (var (key, value) in this.loadedProjects[projectId].verticeWrappers.Where(x=>x.Value.verticeData.verticeType == VerticeType.Person))
        {
            if (!this.loadedProjects[projectId].verticeData.ContainsKey(key)) continue;
            SpawnGeneralVertice(projectId, new Vector3(counter, distanceFromMiddleGraph + counter, 0), key, -1);
            counter += spaceBetweenWallObjs;
        }
    }

    private Transform SpawnGeneralVertice(long projectId, Vector3 spawnPos, long verticeId, long edgeId)
    {
        Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(verticePrefab, spawnPos, Quaternion.identity);

        VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
        verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText, sidebarController, this);
        VerticeWrapper d = loadedProjects[projectId].verticeWrappers[verticeId];
        // verticeRenderer.SetVerticeData(d, projectId, verticeMaterial);
        if (!vertices.ContainsKey(projectId))
        {
            vertices[projectId] = new();
        }
        vertices[projectId][verticeId]= verticeRenderer;
        // CheckAndApplyHighlight(projectId, verticeId); //TODO return
        verticeRenderer.SetIsLoaded(true);
        return obj;
    }

    private void SpawnOutlineObjects(long projectId)
    {
        float pos = renderDistanceBetweenObjs;
        for (int i = 0; i < loadedProjects[projectId].GetTicketCount(); i++)
        {
            if (spawnTopOutlinesForSpiral)
            {
                SpawnOutlineObject(pos, 0, projectId); // TODO do we want the top one too?
            }

            pos += SpawnOutlineObject(pos, -distanceOnComplete, projectId);
        }
    }

    private float SpawnOutlineObject(float pos, float yPos, long projectId)
    {
        float x = Mathf.Cos(pos) * pos;
        float z = Mathf.Sin(pos) * pos;

        if (!filterHolder.disabledVertices.Contains(VerticeType.Ticket))
        {
            Transform obj = PoolManager.Pools[PoolNames.VERTICE_OUTLINE]
                .Spawn(verticePrefab, new Vector3(x, yPos, z), Quaternion.identity);
            if (obj.TryGetComponent<VerticeRenderer>(out VerticeRenderer vr))
            {
                vr.OnDespawned();
            }
            Destroy(obj.GetComponent<VerticeRenderer>());
            Destroy(obj.GetComponent<BoxCollider>());
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.materials = new[] { outlineMaterial };
        }

        // keeping track of max/min Z and X axes
        if (!projectSizesZ.ContainsKey(projectId))
        {
            projectSizesZ[projectId] = new Pair<float, float>(x, z);
            projectSizesX[projectId] = new Pair<float, float>(x, z);
        }
        else
        {
            Pair<float, float> zAxe = projectSizesZ[projectId];
            if (zAxe.Left > z) zAxe.Left = z;
            if (zAxe.Right < z) zAxe.Right = z;
            
            Pair<float, float> xAxe = projectSizesX[projectId];
            if (xAxe.Left > x) xAxe.Left = x;
            if (xAxe.Right < x) xAxe.Right = x;
        }

        return renderDistanceBetweenObjs / pos;
    }

    public void CheckAndApplyHighlight(long projectId, long verticeId)
    {
        if (!loadedProjects.ContainsKey(projectId) || !this.vertices.ContainsKey(projectId) || !this.vertices[projectId].ContainsKey(verticeId))
            return;
        // Check for active highlights
        if (SingletonManager.Instance.dataManager.selectedDates.Count > 0)
        {
            if (this.loadedProjects[projectId].verticeWrappers[verticeId].ContainsDate(SingletonManager.Instance.dataManager.selectedDates))
            {
                this.vertices[projectId][verticeId].SetHighlighted(true);
            }
            else
            {
                this.vertices[projectId][verticeId].SetHidden(true);
            }
        }
        else if (SingletonManager.Instance.dataManager.selectedVertices.Count > 0)
        {
            if (SingletonManager.Instance.dataManager.selectedVertices.Contains(this.loadedProjects[projectId].verticeWrappers[verticeId]))
            {
                this.vertices[projectId][verticeId].SetHighlighted(true);
            }
            else
            {
                this.vertices[projectId][verticeId].SetHidden(true);
            }
        }
    }
}