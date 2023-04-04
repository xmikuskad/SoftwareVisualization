using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Helpers;
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
    private Dictionary<long, Dictionary<long, EdgeRenderer>> edges = new();

    private Dictionary<VerticeType, Material> verticeMaterial = new();
    private Dictionary<EdgeType, Material> edgeMaterial = new();

    private List<Pair<bool, LineRenderer>> lineRenderers = new();

    [Header("Properties")] public List<VerticeMaterial> verticeMaterialsList;
    public List<EdgeMaterial> edgeMaterialsList;

    public float renderDistanceBetweenObjs = 3;
    public float distanceOnComplete = 4f;

    [Header("References")] public EventRenderer eventRenderer;
    public GameObject verticePrefab;
    public GameObject edgePrefab;

    public Canvas hoverCanvas;
    public GameObject hoverElement;
    public TMP_Text hoverText;

    public GameObject loadingBar;
    public GameObject loadBtn;

    public Material lineMaterial;
    public TMP_Text currentDateText;
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
        // StartCoroutine(DestroyObjects());

        foreach (var keyPair in vertices)
        {
            foreach (var keyPairChild in vertices[keyPair.Key])
            {
                Destroy(vertices[keyPair.Key][keyPairChild.Key].gameObject);
            }
        }

        vertices.Clear();

        foreach (var keyPair in edges)
        {
            foreach (var keyPairChild in edges[keyPair.Key])
            {
                Destroy(edges[keyPair.Key][keyPairChild.Key].gameObject);
            }
        }

        edges.Clear();
        DOTween.Clear();
        this.eventRenderer.queue.Clear();
    }

    public void RenderData(bool rerender)
    {
        if (loadedProjects == null) return;
        SpawnPeople(1L);
        this.eventRenderer.Init(this.loadedProjects[1].eventData);
        SetLoading(false);
        this.eventRenderer.NextQueue();
    }

    public void RerenderProject(long projectId)
    {
        DataHolder holder = this.loadedProjects[projectId];
        ResetData();
        AddData(holder,false);
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
        currentDateText.text = data.when + " (Day "+(data.when.Subtract(loadedProjects[data.projectId].startDate).Days+1)+")";
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
        // TODO make color change into transparent?
        // foreach (var lineRenderer in lineRenderers)
        // {
        //     sequence.Append(lineRenderer.Second.DOColor(new Color2(lineRenderer.Second.startColor, lineRenderer.Second.endColor),
        //         new Color2(Color.clear, Color.clear), SingletonManager.Instance.animationManager.GetLineDisappearTime()));
        // }
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
                GameObject go = new GameObject("Line");
                go.AddComponent<LineRenderer>();
                renderer = new Pair<bool, LineRenderer>(false, go.GetComponent<LineRenderer>());
                renderer.Second.material = new Material(lineMaterial);
                this.lineRenderers.Add(renderer);
            }

            renderer.First = true; // Set as used;
            renderer.Second.startColor = Color.yellow;
            renderer.Second.endColor = Color.yellow;
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
            
            sequence.Append( DOTween.To(() => oldPos, x =>
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
        GameObject obj = Instantiate(verticePrefab, new Vector3(x, 0, y), Quaternion.identity);

        obj.transform.localScale = new Vector3(0, 0, 0);
        spawnTheta[eventData.projectId] += renderDistanceBetweenObjs / idk;

        VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
        verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText);
        VerticeData d = loadedProjects[eventData.projectId].verticeData[eventData.verticeId];
        verticeRenderer.SetVerticeData(d, verticeMaterial[d.verticeType]);
        if (!vertices.ContainsKey(eventData.projectId))
        {
            vertices.Add(eventData.projectId, new());
        }

        vertices[eventData.projectId].Add(eventData.verticeId, verticeRenderer);
    }

    private void SpawnPeople(long projectId)
    {
        long counter = 1;
        foreach (long personId in this.loadedProjects[projectId].personIds.Values)
        {

            GameObject obj = Instantiate(verticePrefab, new Vector3(counter, 10+counter, 0), Quaternion.identity);


            VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
            verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText);
            VerticeData d = loadedProjects[projectId].verticeData[personId];
            verticeRenderer.SetVerticeData(d, verticeMaterial[d.verticeType]);
            if (!vertices.ContainsKey(projectId))
            {
                vertices.Add(projectId, new());
            }

            vertices[projectId].Add(personId, verticeRenderer);

            counter+=2;
        }

        for (int i = 0; i < 20; i++)
        {
            GameObject go = new GameObject("Line");
            go.AddComponent<LineRenderer>();
            LineRenderer ren = go.GetComponent<LineRenderer>();
            Pair<bool, LineRenderer> renderer = new Pair<bool, LineRenderer>(false, ren);
            this.lineRenderers.Add(renderer);
            renderer.Second.startWidth = 0.2f;
            renderer.Second.endWidth = 0.2f;
            renderer.Second.positionCount = 2;
            renderer.Second.useWorldSpace = true;
            renderer.Second.material = new Material(lineMaterial);
            renderer.Second.enabled = false;
        }
    }
}