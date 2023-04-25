using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Helpers;
using PathologicalGames;
using Renderers;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DataRenderer : MonoBehaviour
{
    public Dictionary<long, DataHolder> loadedProjects = new();

    //Used to keep track of next spawn position
    public Dictionary<long, float> spawnTheta = new(); // <ProjectId,Theta> 

    // Instantiated objects
    private Dictionary<long, Dictionary<long, VerticeRenderer>> vertices = new();
    // private Dictionary<long, Dictionary<long, EdgeRenderer>> edges = new();

    private Dictionary<VerticeType, Material> verticeMaterial = new();
    private Dictionary<EdgeType, Material> edgeMaterial = new();

    private List<Pair<bool, LineRenderer>> lineRenderers = new();

    private List<GameObject> gameObjectsToClean = new();    // Storing object which needs to be cleaned for later

    [Header("Properties")] public List<VerticeMaterial> verticeMaterialsList;
    public List<EdgeMaterial> edgeMaterialsList;

    public float renderDistanceBetweenObjs = 3;
    public float distanceOnComplete = 4f;
    public bool spawnTopOutlinesForSpiral = false;

    [Header("References")] public EventRenderer eventRenderer;
    public GameObject verticePrefab;
    public GameObject edgePrefab;

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

    private void Awake()
    {
        verticeMaterial = verticeMaterialsList.ToDictionary(key => key.verticeType, value => value.material);
        edgeMaterial = edgeMaterialsList.ToDictionary(key => key.edgeType, value => value.material);
    }

    public void AddData(List<DataHolder> dataHolder, bool rerender)
    {
        SetLoading(true);
        foreach (var holder in dataHolder)
        {
            this.loadedProjects.Add(holder.projectId, holder);
            spawnTheta[holder.projectId] = renderDistanceBetweenObjs;
        }

        RenderData(rerender);
    }

    public void AddData(DataHolder dataHolder, bool rerender)
    {
        SetLoading(true);
        this.loadedProjects.Add(dataHolder.projectId, dataHolder);
        spawnTheta[dataHolder.projectId] = renderDistanceBetweenObjs;
        RenderData(rerender);
    }

    public void ResetData()
    {
        this.spawnTheta.Clear();
        this.loadedProjects.Clear();
        PoolManager.Pools[PoolNames.VERTICE].DespawnAll(); // This removes all vertices from scene :)

        vertices.Clear();

        // foreach (var keyPair in edges)
        // {
        //     foreach (var keyPairChild in edges[keyPair.Key])
        //     {
        //         Destroy(edges[keyPair.Key][keyPairChild.Key].gameObject);
        //     }
        // }

        // edges.Clear();
        DOTween.Clear();
        this.eventRenderer.queue.Clear();
    }

    public void RenderData(bool rerender)
    {
        if (loadedProjects == null) return;
        this.eventRenderer.Init(this.loadedProjects[1].eventData);
        SpawnPeople(1L);
        collabMatrix.fillMatrix(this.loadedProjects[1]);
        contributionsCalendar.fillContributionsCalendar(this.loadedProjects[1]);
        SpawnOutlineObjects(1L);
        timelineRenderer.LoadTimeline(this.loadedProjects[1]);
        SetLoading(false);
        this.eventRenderer.NextQueue();
    }

    public void RerenderProject(long projectId)
    {
        DataHolder holder = this.loadedProjects[projectId];
        ResetData();
        AddData(holder, false);
    }

    // Manages loading screen
    public void SetLoading(bool status)
    {
        SingletonManager.Instance.pauseManager.SetInteractionPaused(status);
        loadingBar.SetActive(status);
        loadBtn.SetActive(!status);
    }

    public void ProcessEvents(List<EventData> data)
    {

    }

    public void ProcessEvent(EventData data)
    {
        // currentDateText.DOText(data.when + " (Day "+(data.when.Subtract(loadedProjects[data.projectId].startDate).Days+1)+")",SingletonManager.Instance.animationManager.GetSpawnAnimTime(),false);
        currentDateText.text = data.when + " (Day " + (data.when.Subtract(loadedProjects[data.projectId].startDate).Days + 1) + ")";
        Sequence sequence = DOTween.Sequence();

        if (data.actionType == EventActionType.CREATE)
        {
            // Instantiate
            SpawnTicket(data);
            VerticeRenderer ver = vertices[data.projectId][data.verticeId];
            sequence.Append(
                ver.transform.DOScale(new Vector3(1, 1, 1),
                    SingletonManager.Instance.animationManager.GetSpawnAnimTime()));
            ProcessPerson(data.projectId, data.personIds, sequence, ver.transform.position, ver.transform.position);
        }
        else if (data.actionType == EventActionType.MOVE)
        {
            VerticeRenderer ver = vertices[data.projectId][data.verticeId];
            float distanceToMove =
                distanceOnComplete / loadedProjects[data.projectId].edgeCountForTickets[data.verticeId];
            ver.AddCompletedEdge(1L, loadedProjects[data.projectId].edgeCountForTickets[data.verticeId]);
            Vector3 newPos = ver.transform.position - new Vector3(0, distanceToMove, 0);
            ProcessPerson(data.projectId, data.personIds, sequence, ver.transform.position, newPos);
            sequence.Append(
                ver.transform.DOMoveY(ver.transform.position.y - distanceToMove,
                    SingletonManager.Instance.animationManager.GetMoveAnimTime()));
        }
        else if (data.actionType == EventActionType.UPDATE)
        {
            // throw new NotImplementedException();
            // eventRenderer.NextQueue();
        }

        sequence.AppendInterval(SingletonManager.Instance.animationManager.GetWaitTime());
        sequence.OnComplete(() =>
        {
            foreach (var lineRenderer in this.lineRenderers)
            {
                lineRenderer.First = false;
                lineRenderer.Second.enabled = false;
            }

            eventRenderer.NextQueue();
        });
        sequence.Play();
    }

    private void ProcessPerson(long projectId, List<long> personIds, Sequence sequence, Vector3 oldPos, Vector3 newPos)
    {
        foreach (var personId in personIds)
        {
            Pair<bool, LineRenderer> renderer = this.lineRenderers.FirstOrDefault(x => !x.First);
            if (renderer == null)
            {
                LineRenderer ren = PoolManager.Pools[PoolNames.VERTICE_PERSON_LINE].Spawn(lineRenderer.gameObject).GetComponent<LineRenderer>();
                // LineRenderer ren = Instantiate(lineRenderer);
                renderer = new Pair<bool, LineRenderer>(false, ren);
                this.lineRenderers.Add(renderer);
            }

            renderer.First = true; // Set as used;
            renderer.Second.startWidth = 0.2f;
            renderer.Second.endWidth = 0.2f;
            renderer.Second.positionCount = 2;
            renderer.Second.useWorldSpace = true;
            renderer.Second.enabled = true;
            renderer.Second.SetPosition(0,
                vertices[projectId][personId].transform
                    .position); //x,y and z position of the starting point of the line
            renderer.Second.SetPosition(1, oldPos); //x,y and z position of the end point of the line

            // TODO maybe increase scale with number of changes?
            // sequence.Append(vertices[projectId][personId].transform.DOScaleX(vertices[projectId][personId].transform.localScale.x+0.05f,SingletonManager.Instance.animationManager.GetMoveAnimTime()) );

            sequence.Append(DOTween.To(() => oldPos, x =>
            {
                oldPos = x;
                renderer.Second.SetPosition(1, x);
            }, newPos, SingletonManager.Instance.animationManager.GetMoveAnimTime()));


        }
    }

    // Used algo for spiral spawn https://stackoverflow.com/questions/13894715/draw-equidistant-points-on-a-spiral
    private void SpawnTicket(EventData eventData)
    {
        float idk = spawnTheta[eventData.projectId];
        float x = Mathf.Cos(idk) * idk;
        float y = Mathf.Sin(idk) * idk;
        Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(verticePrefab, new Vector3(x, 0, y), Quaternion.identity);

        obj.transform.localScale = new Vector3(0, 0, 0);
        spawnTheta[eventData.projectId] += renderDistanceBetweenObjs / idk;

        VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
        verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText, sidebarController, this);
        VerticeData d = loadedProjects[eventData.projectId].verticeData[eventData.verticeId];
        verticeRenderer.SetVerticeData(d, eventData.projectId, verticeMaterial[d.verticeType]);
        if (!vertices.ContainsKey(eventData.projectId))
        {
            vertices.Add(eventData.projectId, new());
        }

        vertices[eventData.projectId].Add(eventData.verticeId, verticeRenderer);

        CheckAndApplyHighlight(eventData.projectId, eventData.verticeId);
        verticeRenderer.SetIsLoaded(true);
    }

    private void SpawnPeople(long projectId)
    {
        long counter = 1;
        foreach (long personId in this.loadedProjects[projectId].personIds.Values)
        {

            if (!this.loadedProjects[projectId].verticeData.ContainsKey(personId)) continue;

            Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(verticePrefab, new Vector3(counter, 10 + counter, 0), Quaternion.identity);

            VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
            verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText, sidebarController, this);
            VerticeData d = loadedProjects[projectId].verticeData[personId];
            verticeRenderer.SetVerticeData(d, projectId, verticeMaterial[d.verticeType]);
            if (!vertices.ContainsKey(projectId))
            {
                vertices.Add(projectId, new());
            }

            vertices[projectId].Add(personId, verticeRenderer);
            verticeRenderer.SetIsLoaded(true);
            counter += 2;
        }
    }

    private void SpawnOutlineObjects(long projectId)
    {
        float pos = renderDistanceBetweenObjs;
        for (int i = 0; i < loadedProjects[projectId].GetTicketCount(); i++)
        {
            if (spawnTopOutlinesForSpiral)
            {
                SpawnOutlineObject(pos, 0); // TODO do we want the top one too?
            }

            pos += SpawnOutlineObject(pos, -distanceOnComplete);
        }
    }

    private float SpawnOutlineObject(float pos, float yPos)
    {
        float x = Mathf.Cos(pos) * pos;
        float y = Mathf.Sin(pos) * pos;
        Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(verticePrefab, new Vector3(x, yPos, y), Quaternion.identity);
        Destroy(obj.GetComponent<VerticeRenderer>());
        Destroy(obj.GetComponent<BoxCollider>());
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.materials = new[] { outlineMaterial };
        gameObjectsToClean.Add(obj.gameObject);

        return renderDistanceBetweenObjs / pos;
    }

    public void UnhighlightElements(long projectId)
    {
        if (!this.vertices.ContainsKey(projectId))
            return;
        foreach (var keyValuePair in this.vertices[projectId])
        {
            keyValuePair.Value.SetHighlighted(false);
        }
    }

    public void HighlightVertice(long projectId, long verticeId)
    {
        foreach (var keyValuePair in this.vertices[projectId])
        {
            keyValuePair.Value.SetHidden(true);
        }

        VerticeRenderer renderer;
        if (this.vertices[projectId].TryGetValue(verticeId, out renderer))
        {
            renderer.SetHighlighted(true);
        }
    }

    public void HighlightVerticeByDate(long projectId, DateTime date)
    {
        foreach (var keyValuePair in this.vertices[projectId])
        {
            keyValuePair.Value.SetHidden(true);
        }
        // this.vertices[projectId][verticeId].SetHighlighted(true);
        foreach (var l in this.loadedProjects[projectId].eventsByDate[date.Date].Select(x => x.verticeId).Distinct())
        {
            VerticeRenderer renderer;
            if (this.vertices[projectId].TryGetValue(l, out renderer))
            {
                renderer.SetHighlighted(true);
            }
        }
    }

    // TODO optimize
    public void CheckAndApplyHighlight(long projectId, long verticeId)
    {
        if (!loadedProjects.ContainsKey(projectId) || !this.vertices.ContainsKey(projectId) || !this.vertices[projectId].ContainsKey(verticeId))
            return;
        // Check for active highlights
        if (SingletonManager.Instance.dataManager.highlightedDate.HasValue)
        {
            if (this.loadedProjects[projectId].rawDatesForVertice[verticeId]
                .Contains(SingletonManager.Instance.dataManager.highlightedDate.Value))
            {
                this.vertices[projectId][verticeId].SetHighlighted(true);
            }
            else
            {
                this.vertices[projectId][verticeId].SetHidden(true);
            }
        }
        else if (SingletonManager.Instance.dataManager.highlightedVerticeId >= 0)
        {
            if (SingletonManager.Instance.dataManager.highlightedVerticeId == verticeId && SingletonManager.Instance.dataManager.highlightedProjectId == projectId)
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