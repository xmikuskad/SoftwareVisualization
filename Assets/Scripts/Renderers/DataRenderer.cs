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

    // public Pair<DateTime, DateTime> dateFilter = new(DateTime.MinValue.Date, DateTime.MinValue.Date);
    public Dictionary<long, Pair<DateTime, DateTime>> dateFilter = new();




    // New variables
    public Dictionary<long, Dictionary<VerticeType, GameObject>> platforms = new();
    private Dictionary<long, Dictionary<long, List<VerticeRenderer>>> verticesWithEdges = new();
    private Dictionary<long, Dictionary<VerticeType, List<GameObject>>> verticePlatforms = new();
    private Dictionary<long, DateTime> currentProjectDate = new();
    private Dictionary<long, GameObject> datePlatformTrackers = new();
    private Dictionary<long, DateTime> lastDate = new();

    public Dictionary<long, GameObject> projectNamesObjects = new();
    private long activeProjectId = -1;
    private List<LineRenderer> spawnedLines = new();

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
    public long spaceBetweenObjects = 2;
    public long helperDistanceFromGraph = 100;
    public float lineWidth = 2f;
    public long distanceBetweenProjects = 300;
    public long projectNameDistance = 50;

    [Header("Prefabs")]
    public GameObject verticePrefab;
    public Material verticeMaterial;
    public GameObject platformPrefab;
    public GameObject clickablePlatformPrefab;
    public Material transparentMaterial;
    public GameObject arrowPrefab;
    public GameObject linePrefab;
    public GameObject projectNamePrefab;
    public Material lineRendererMaterial;

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
    public GameObject dateTrackerPrefab;

    private Transform mainCameraTransform;

    private void Awake()
    {
        SingletonManager.Instance.dataManager.DataFilterEvent += OnDataFilter;
        SingletonManager.Instance.dataManager.DateChangeEvent += OnDateChangeEvent;
        SingletonManager.Instance.dataManager.DateRenderChangedEvent += OnDateRenderChanged;
        SingletonManager.Instance.dataManager.ResetEvent += OnReset;
        SingletonManager.Instance.dataManager.VerticesSelectedEvent += OnVerticeSelected;
        SingletonManager.Instance.preferencesManager.MappingChangedEvent += OnMappingChanged;
        SingletonManager.Instance.dataManager.SelectedProjectChanged += OnSelectedProjectChanged;
        filterHolder = new();
        mainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        foreach (var (key, value) in projectNamesObjects)
        {
            Vector3 directionToCamera = mainCameraTransform.position - value.transform.position;
            Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera);

            // Apply the rotation to the text
            value.transform.rotation = rotationToCamera * Quaternion.Euler(0, 180, 0);
        }
    }

    private void Update()
    {
        if (this.loadedProjects.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RenderNextDate();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RenderPreviousDate();
        }
    }

    private void OnSelectedProjectChanged(DataHolder dataHolder)
    {
        this.activeProjectId = dataHolder.projectId;
        if (!lastDate.ContainsKey(activeProjectId))
        {
            lastDate[activeProjectId] = DateTime.MinValue.Date;
        }

        foreach (var (key, value) in projectNamesObjects)
        {
            value.GetComponent<TMP_Text>().color = key == activeProjectId ? Color.red : Color.black;
        }

        if(!dateFilter.ContainsKey(activeProjectId))
            dateFilter[activeProjectId] = new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MinValue.Date);
        // Load viz techniques
        collabMatrix.fillMatrix(dataHolder); // <-- subscribed in collabMatrix to project change
        contributionsCalendar.fillContributionsCalendar(dataHolder, dataHolder.startDate.Year);
        timelineRenderer.LoadTimeline(dataHolder);
        kiviatDiagram.initiateKiviat(dataHolder);
    }


    private void OnMappingChanged(Dictionary<long, ColorMapping> colorMappings, Dictionary<long, ShapeMapping> shapeMappings)
    {
        foreach (var (pid, val) in verticePlatforms)
        {
            int index = 0;
            foreach (var (key, value) in val)
            {
                Color32 color = colorMappings[SingletonManager.Instance.preferencesManager.GetColorMappingByType(key).id].color;

                // verticePlatforms[val]
                foreach (var obj in value)
                {
                    Color32 color2 = color;
                    if (index % 2 == 1)
                    {
                        color2.r = (byte)Math.Clamp(color.r * 1.25f, 0, 255);
                        color2.g = (byte)Math.Clamp(color.g * 1.25f, 0, 255);
                        color2.b = (byte)Math.Clamp(color.b * 1.25f, 0, 255);
                        color2.a = (byte)Math.Clamp(color.a * 1.25f, 0, 255);
                    }
                    Material newMat = new Material(transparentMaterial);
                    newMat.color = color2;
                    obj.GetComponent<MeshRenderer>().material = newMat;
                    index++;
                }
            }
        }

        foreach (var (pid, val) in platforms)
        {
            foreach (var (key, value) in val)
            {
                Color32 color =
                    colorMappings[SingletonManager.Instance.preferencesManager.GetColorMappingByType(key).id].color;
                Material newMat = new Material(transparentMaterial);
                newMat.color = color;
                value.GetComponent<MeshRenderer>().material = newMat;
            }
        }

        foreach (var (key, platform) in datePlatformTrackers)
        {
            Material newMat = new Material(verticeMaterial);
            newMat.color = colorMappings[ColorMapping.DATE_PLATFORM.id].color;
            platform.GetComponent<MeshRenderer>().material = newMat;
        }

        foreach (var spawnedLine in spawnedLines)
        {
            Material lrMaterial = new Material(lineRendererMaterial);
            lrMaterial.color = colorMappings[ColorMapping.LINE_COLOR.id].color;
            spawnedLine.startColor = colorMappings[ColorMapping.LINE_COLOR.id].color;
            spawnedLine.endColor = colorMappings[ColorMapping.LINE_COLOR.id].color;
            spawnedLine.material = lrMaterial;
        }
    }

    private void OnVerticeSelected(List<Pair<VerticeData, VerticeWrapper>> list)
    {
        PoolManager.Pools[PoolNames.LINES].DespawnAll();

        // pair<Commit/Change, VErtice of commit>
        foreach (Pair<VerticeData, VerticeWrapper> p in list)
        {
            if (p.Left == null || p.Right.verticeData.verticeType == VerticeType.Change || p.Right.verticeData.verticeType == VerticeType.Commit) // No commit/change = no lines
                continue;

            VerticeRenderer from = verticesWithEdges[p.Right.projectId][p.Right.verticeData.id]
                .Where(x => x.commitOrChange?.id == p.Left.id).ToList()[0];

            if (filterHolder.disabledVertices.Contains(from.verticeWrapper.verticeData.verticeType))
            {
                continue;
            }

            // Related vertices of commit/changes
            foreach (var verticeData in p.Right.relatedChangesOrCommits[p.Left.id].GetRelatedVertices().Where(x =>
                         x.id != p.Right.verticeData.id && x.verticeType != VerticeType.Change &&
                         x.verticeType != VerticeType.Commit))
            {
                List<VerticeRenderer> to = verticesWithEdges[p.Right.projectId][verticeData.id]
                    .Where(x => (x.commitOrChange ?? p.Left).id == p.Left.id).ToList();

                if (to.Count == 0)
                {
                    continue;
                }
                foreach (var t in to)
                {
                    if (filterHolder.disabledVertices.Contains(t.verticeWrapper.verticeData.verticeType) || !t.gameObject.activeInHierarchy)
                    {
                        continue;
                    }
                    CreatePath(from, t);
                }
            }
        }
    }

    private void CreatePath(VerticeRenderer from, VerticeRenderer to)
    {
        Transform t = PoolManager.Pools[PoolNames.LINES].Spawn(linePrefab);
        LineRenderer lr = t.GetComponent<LineRenderer>();
        lr.positionCount = 2;

        Material lrMaterial = new Material(lineRendererMaterial);
        lrMaterial.color = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.LINE_COLOR).color;
        lr.material = lrMaterial;

        lr.startColor = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.LINE_COLOR).color;
        lr.endColor = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.LINE_COLOR).color;

        lr.SetPosition(0, from.transform.position);
        lr.SetPosition(1, to.transform.position);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        spawnedLines.Add(lr);
    }

    private void OnReset(ResetEventReason reason)
    {
        if (loadedProjects.Count == 0)
            return;
        if (reason == ResetEventReason.CLICK_OUTSIDE)
        {
            if (dateFilter[activeProjectId].Right != DateTime.MinValue.Date)
            {
                dateFilter[activeProjectId] = new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MinValue.Date);
                RenderAllDates();
            }
            ClearLines();
        }
        else if (reason == ResetEventReason.FILTER)
        {
            ClearLines();
        }
    }

    private void ClearLines()
    {
        spawnedLines.Clear();
        PoolManager.Pools[PoolNames.LINES].DespawnAll();
    }

    private void OnDateRenderChanged(Pair<long, Pair<DateTime, DateTime>> pair)
    {
        lastDate[pair.Left] = DateTime.MinValue.Date;
        currentProjectDate[pair.Left] = DateTime.MinValue.Date;

        foreach (var (key, value) in verticesWithEdges[pair.Left])
        {
            foreach (var verticeRenderer in value)
            {
                if (filterHolder.disabledVertices.Contains(verticeRenderer.verticeWrapper.verticeData.verticeType))
                {
                    verticeRenderer.gameObject.SetActive(false);
                    continue;
                }
                verticeRenderer.gameObject.SetActive(verticeRenderer.commitOrChange?.IsDateBetween(pair.Right.Left, pair.Right.Right) ?? true);
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
                if (verticeRenderer.verticeWrapper.verticeData.verticeType != VerticeType.Person)
                    verticeRenderer.gameObject.SetActive(verticeRenderer.ContainsDate(date));
            }
        }
        currentProjectDate[projectId] = date;
        TimeSpan diff = lastDate[projectId] != DateTime.MinValue.Date ? lastDate[projectId] - currentProjectDate[projectId] : this.loadedProjects[projectId].startDate - currentProjectDate[projectId];
        int days = (int)diff.TotalDays;
        Vector3 platformPos = datePlatformTrackers[projectId].transform.position;
        platformPos.z -= (1 + spaceBetweenObjects) * days;

        Debug.Log("Changing pos for " + projectId + " from " + datePlatformTrackers[projectId].transform.position + " to " + platformPos + " | Last date " + lastDate[projectId] + " Current date " + currentProjectDate[projectId]);

        datePlatformTrackers[projectId].transform.position = platformPos;

        if (lastDate[projectId] == DateTime.MinValue.Date)
        {
            SingletonManager.Instance.dataManager.InvokeResetEvent(ResetEventReason.CLEAR_LINES);
            datePlatformTrackers[projectId].gameObject.SetActive(true);
        }

        lastDate[projectId] = date;
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

        if (this.filterHolder == null)
        {
            this.filterHolder = new();
        }
        RenderDataNew(dataHolder.projectId);
        if (this.filterHolder.disabledVertices.Count > 0)
        {
            SingletonManager.Instance.dataManager.InvokeDataFilterEvent(filterHolder);
        }
    }

    // public void ResetData(bool resetAll)
    // {
    //     this.spawnTheta.Clear();
    //     this.loadedProjects.Clear();
    //     PoolManager.Pools[PoolNames.VERTICE].DespawnAll(); // This removes all vertices from scene :)
    //     PoolManager.Pools[PoolNames.VERTICE_OUTLINE].DespawnAll(); // This removes all outline vertices from scene :)
    //     PoolManager.Pools[PoolNames.HIGHLIGHT_OBJECTS].DespawnAll(); // This removes all outline vertices from scene :)
    //
    //     vertices = new();
    //     this.dateIndexTracker.Clear();
    //     commitPosTracker.Clear();
    //     wikiPosTracker.Clear();
    //     filePosTracker.Clear();
    //     repoPosTracker.Clear();
    //
    //     projectSizesX.Clear();
    //     projectSizesZ.Clear();
    //
    //
    //     // New things
    //     platforms.Clear();
    //     currentProjectDate.Clear();
    //
    //     if (resetAll)
    //         this.dateFilter = new Pair<DateTime, DateTime>(DateTime.MinValue.Date, DateTime.MinValue.Date);
    //
    //     DOTween.Clear();
    // }

    public void RenderDataNew(long projectId)
    {
        if (loadedProjects == null) return;
        SpawnVerticesAndEdges(projectId);
        SpawnDateTrackerPlatform(projectId);
        SpawnHelper(projectId);
        SetLoading(false);
    }

    private void SpawnHelper(long projectId)
    {
        SpawnHelperPlatformText(projectId, 0, "Person");
        SpawnHelperPlatformText(projectId, 1, "Ticket");
        SpawnHelperPlatformText(projectId, 2, "Repository file");
        SpawnHelperPlatformText(projectId, 3, "File");
        SpawnHelperPlatformText(projectId, 4, "Wiki");


        TimeSpan diff = this.loadedProjects[projectId].maxDate - this.loadedProjects[projectId].minDate;
        int projectDays = (int)diff.TotalDays;
        float platformWidth = projectDays * (spaceBetweenObjects + 1);

        float maxCount = (this.loadedProjects[projectId].maxVerticeCount) * (1 + spaceBetweenObjects);

        GameObject go = Instantiate(arrowPrefab);
        Vector3 from = new Vector3(-helperDistanceFromGraph, 0, -helperDistanceFromGraph) + GetSpawnVector(projectId);
        Vector3 to = new Vector3(-helperDistanceFromGraph, 0, -helperDistanceFromGraph + platformWidth) + GetSpawnVector(projectId);
        go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f, "Changes", Direction.LEFT);


        go = Instantiate(arrowPrefab);
        from = new Vector3(-helperDistanceFromGraph, 0, -helperDistanceFromGraph) + GetSpawnVector(projectId);
        to = new Vector3(-helperDistanceFromGraph + platformWidth, 0, -helperDistanceFromGraph) + GetSpawnVector(projectId);
        go.GetComponent<ArrowRenderer>().SetUp(from, to, 2f, "Elements", Direction.RIGHT);


        go = Instantiate(projectNamePrefab, GetSpawnVector(projectId) + new Vector3(maxCount / 2f, projectNameDistance, platformWidth / 2f), Quaternion.identity);
        projectNamesObjects[projectId] = go;
        go.GetComponent<TMP_Text>().text = this.loadedProjects[projectId].projectName;

    }

    private void SpawnHelperPlatformText(long projectId, int index, string text)
    {
        GameObject go = Instantiate(arrowPrefab);
        // float maxCount = (this.loadedProjects[projectId].maxVerticeCount ) * (1 + spaceBetweenObjects);
        float height = platformHeight + platformDistanceBetween;
        Vector3 from = new Vector3(-helperDistanceFromGraph, 0 - index * height, -helperDistanceFromGraph) + GetSpawnVector(projectId);
        Vector3 to = new Vector3(-helperDistanceFromGraph, 0 - height - index * height, -helperDistanceFromGraph) + GetSpawnVector(projectId);
        go.GetComponent<ArrowRenderer>().SetUpNoArrows(from, to, 2f, text, Direction.DOWN);
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
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount - alreadySpawnedCount) *
                         (1 + spaceBetweenObjects);

        Vector3 pos =
            new Vector3((alreadySpawnedCount) * (1 + spaceBetweenObjects) + maxCount / 2f - 1f,
                (-index) * platformDistanceBetween, platformWidth / 2f) + GetSpawnVector(projectId);

        GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity);
        platform.transform.localScale = new Vector3(maxCount + 1, platformHeight, platformWidth + 1f);

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
        if (index % 2 == 1)
        {
            color.r = (byte)Math.Clamp(color.r * 1.25f, 0, 255);
            color.g = (byte)Math.Clamp(color.g * 1.25f, 0, 255);
            color.b = (byte)Math.Clamp(color.b * 1.25f, 0, 255);
            color.a = (byte)Math.Clamp(color.a * 1.25f, 0, 255);
        }
        GameObject platform = Instantiate(clickablePlatformPrefab, platformPos, Quaternion.identity);
        platform.transform.localScale = new Vector3(spaceBetweenObjects + 1f, platformHeight, platformWidth + 1f);

        Material newMat = new Material(transparentMaterial);
        newMat.color = color;
        platform.GetComponent<MeshRenderer>().material = newMat;
        platform.GetComponent<VerticePlatformRenderer>().SetUp(projectId, v);

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


        float datePosAdd = (spaceBetweenObjects + 1);

        TimeSpan diff = this.loadedProjects[projectId].maxDate - this.loadedProjects[projectId].minDate;
        int projectDays = (int)diff.TotalDays;

        float platformWidth = projectDays * (spaceBetweenObjects + 1);

        SpawnVerticeAndEdges(0, VerticeType.Person, projectId, datePosAdd, platformWidth);
        SpawnVerticeAndEdges(1, VerticeType.Ticket, projectId, datePosAdd, platformWidth);
        SpawnVerticeAndEdges(2, VerticeType.RepoFile, projectId, datePosAdd, platformWidth);
        SpawnVerticeAndEdges(3, VerticeType.File, projectId, datePosAdd, platformWidth);
        SpawnVerticeAndEdges(4, VerticeType.Wiki, projectId, datePosAdd, platformWidth);
    }

    public void SpawnDateTrackerPlatform(long projectId)
    {
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount) * (1 + spaceBetweenObjects);

        Color32 color = SingletonManager.Instance.preferencesManager.GetColorMapping(ColorMapping.DATE_PLATFORM).color;

        float height = platformHeight * 4 - (4) * platformDistanceBetween;

        GameObject platform = Instantiate(dateTrackerPrefab, GetDefaultTrackerPlatformPos(projectId), Quaternion.identity);
        platform.transform.localScale = new Vector3(maxCount + 1, -height, 1.01f);

        Material newMat = new Material(verticeMaterial);
        newMat.color = color;
        platform.GetComponent<MeshRenderer>().material = newMat;

        platform.SetActive(false);
        this.datePlatformTrackers[projectId] = platform;
    }

    private Vector3 GetDefaultTrackerPlatformPos(long projectId)
    {
        float maxCount = (this.loadedProjects[projectId].maxVerticeCount) * (1 + spaceBetweenObjects);
        float height = platformHeight * 4 - (4) * platformDistanceBetween;
        return new Vector3(maxCount / 2 - 1, height / 2f - platformDistanceBetween / 2f, 1f) + GetSpawnVector(projectId);
    }

    public void SpawnVerticeAndEdges(int index, VerticeType type, long projectId, float datePosAdd, float platformWidth)
    {
        List<List<Pair<VerticeData, VerticeWrapper>>>
            list = this.loadedProjects[projectId].GetVerticesForPlatform(type);


        Dictionary<long, Dictionary<DateTime, float>> heightTracker = new();
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
                                                    (-index) * platformDistanceBetween +
                                                    GetVerticeHeight(heightTracker, change), xPos) +
                                                GetSpawnVector(projectId), change.Right,
                        change.Left,
                        GetDatetimeForPos(changes, xIndex));
                }
            }

            if (type != VerticeType.Person)
            {
                SpawnVerticePlatform(zIndex, type, projectId, platformWidth, new Vector3(
                        zIndex * (1 + spaceBetweenObjects) + 0.5f,
                        (-index) * platformDistanceBetween, platformWidth / 2f) + GetSpawnVector(projectId),
                    changes[0].Right);
            }
        }

        if (list.Count < this.loadedProjects[projectId].maxVerticeCount && type != VerticeType.Person)
        {
            SpawnPlatform(index, type, projectId, platformWidth, list.Count);
        }
    }

    private float GetVerticeHeight(Dictionary<long, Dictionary<DateTime, float>> heightTracker, Pair<VerticeData, VerticeWrapper> change)
    {
        DateTime current = (change.Left.created ?? change.Left.begin ?? DateTime.MinValue).Date;
        if (!heightTracker.ContainsKey(change.Right.verticeData.id))
        {
            heightTracker[change.Right.verticeData.id] = new();
        }

        if (!heightTracker[change.Right.verticeData.id].ContainsKey(current))
        {
            heightTracker[change.Right.verticeData.id][current] = 1.5f;
        }

        float height = heightTracker[change.Right.verticeData.id][current];
        heightTracker[change.Right.verticeData.id][current] = height + 1;
        return height;
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
        while (tracker < changes.Count)
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
        verticeRenderer.SetIsLoaded(true);
        return obj;
    }

    // Called from UI
    public void RenderNextDate()
    {
        RenderNextDateForProject(this.activeProjectId);
    }

    public void RenderNextDateForProject(long projectId)
    {
        if (currentProjectDate[projectId] >= loadedProjects[projectId].maxDate)
        {
            return;
        }

        DateTime newDate =
            loadedProjects[projectId].dates.Where(x => x > currentProjectDate[projectId]).Min();
        SingletonManager.Instance.dataManager.InvokeDateChangedEvent(projectId, newDate);
    }

    // Called from UI
    public void RenderPreviousDate()
    {
        RenderPreviousDateForProject(this.activeProjectId);
    }

    public void RenderPreviousDateForProject(long projectId)
    {
        if (currentProjectDate[projectId] <= loadedProjects[projectId].minDate)
        {
            return;
        }

        DateTime newDate = loadedProjects[projectId].dates.Where(x => x < currentProjectDate[projectId]).Max();
        SingletonManager.Instance.dataManager.InvokeDateChangedEvent(projectId, newDate);
    }

    public void RenderAllDates()
    {
        SingletonManager.Instance.dataManager.InvokeResetEvent(ResetEventReason.RERENDER);
        ShowAllDatesForProject(this.activeProjectId);
    }

    public void ShowAllDatesForProject(long projectId)
    {
        lastDate[projectId] = DateTime.MinValue.Date;
        currentProjectDate[projectId] = DateTime.MinValue.Date;

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