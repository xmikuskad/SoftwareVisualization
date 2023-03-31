using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Renderers;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DataRenderer: MonoBehaviour
{
    public Dictionary<long,DataHolder> loadedProjects = new ();
    
    //Used to keep track of next spawn position
    public Dictionary<long, float> spawnTheta = new();    // <ProjectId,Theta> 

    // Instantiated objects
    private Dictionary<long, Dictionary<long, VerticeRenderer>> vertices = new ();
    private Dictionary<long, Dictionary<long, EdgeRenderer>> edges = new();

    private Dictionary<VerticeType, Material> verticeMaterial = new();
    private Dictionary<EdgeType, Material> edgeMaterial = new();

    [Header("Properties")]
    public List<VerticeMaterial> verticeMaterialsList;
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
            this.loadedProjects.Add(holder.projectId,holder);
            spawnTheta[holder.projectId] = renderDistanceBetweenObjs;
        }
        RenderData(rerender);
    }
    
    public void AddData(DataHolder dataHolder, bool rerender)
    {
        SetLoading(true);
        this.loadedProjects.Add(dataHolder.projectId,dataHolder);
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
    }

    public void RenderData(bool rerender)
    {
        if(loadedProjects == null) return;
        this.eventRenderer.Init(this.loadedProjects[1].eventData);
        SetLoading(false);
        this.eventRenderer.NextQueue();
    }

    // Manages loading screen
    public void SetLoading(bool status)
    {
        SingletonManager.Instance.pauseManager.SetPaused(status);
        loadingBar.SetActive(status);
        loadBtn.SetActive(!status);
    }

    public void ProcessEvent(EventData data)
    {
        if (data.actionType == EventActionType.CREATE)
        {
            // Instantiate
            SpawnTicket(data);
            vertices[data.projectId][data.verticeId].transform.DOScale(new Vector3(1, 1, 1), SingletonManager.Instance.animationManager.GetSpawnAnimTime())
                .OnComplete(() => eventRenderer.NextQueue());
        } else if (data.actionType == EventActionType.MOVE)
        {
            float distanceToMove = distanceOnComplete / loadedProjects[data.projectId].edgeCountForTickets[data.verticeId];
            vertices[data.projectId][data.verticeId].AddCompletedEdge(loadedProjects[data.projectId].edgeCountForTickets[data.verticeId]);
            vertices[data.projectId][data.verticeId].transform.DOMoveZ(vertices[data.projectId][data.verticeId].transform.position.z+distanceToMove, SingletonManager.Instance.animationManager.GetMoveAnimTime())
                .OnComplete(() => eventRenderer.NextQueue());
        } else if (data.actionType == EventActionType.UPDATE)
        {
            // throw new NotImplementedException();
            eventRenderer.NextQueue();
        }
        
    }

    // Used algo for spiral spawn https://stackoverflow.com/questions/13894715/draw-equidistant-points-on-a-spiral
    private void SpawnTicket(EventData eventData)
    {
        float idk = spawnTheta[eventData.projectId];
        float x = Mathf.Cos ( idk ) * idk;
        float y = Mathf.Sin ( idk ) * idk;
        GameObject obj = Instantiate(verticePrefab, new Vector3(x,y, 0), Quaternion.identity);

        obj.transform.localScale = new Vector3(0, 0, 0);
        spawnTheta[eventData.projectId] += renderDistanceBetweenObjs / idk;
        
        VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
        verticeRenderer.SetUpReferences(hoverCanvas,hoverElement,hoverText);
        VerticeData d =  loadedProjects[eventData.projectId].verticeData[eventData.verticeId];
        verticeRenderer.SetVerticeData(d, verticeMaterial[d.verticeType]);
        if (!vertices.ContainsKey(eventData.projectId))
        {
            vertices.Add(eventData.projectId,new ());
        }
        vertices[eventData.projectId].Add(eventData.verticeId,verticeRenderer);
    }
}