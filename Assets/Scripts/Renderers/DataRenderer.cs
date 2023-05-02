using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Helpers;
using PathologicalGames;
using Renderers;
using TMPro;
using UnityEngine;

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

    [Header("Properties")]

    public float renderDistanceBetweenObjs = 3;
    public float distanceOnComplete = 4f;
    public bool spawnTopOutlinesForSpiral = false;
    public long spaceBetweenWallObjs = 2;

    [Header("References")]
    public GameObject verticePrefab;
    public Material verticeMaterial;

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

    private void Awake()
    {
        SingletonManager.Instance.dataManager.DatesRangeSelectedEvent += OnDatesRangeSelectedSelected;
    }
    
    private void OnDatesRangeSelectedSelected(Pair<long, List<DateTime>> pair)
    {
        // TODO rerender twice
    }

    public void AddData(List<DataHolder> dataHolder, bool rerender)
    {
        SetLoading(true);
        foreach (var holder in dataHolder)
        {
            this.loadedProjects.Add(holder.projectId, holder);
            spawnTheta[holder.projectId] = renderDistanceBetweenObjs;
        }

        RenderData(rerender,true);
    }

    public void AddData(DataHolder dataHolder, bool rerender, bool renderFirst)
    {
        SetLoading(true);
        this.loadedProjects.Add(dataHolder.projectId, dataHolder);
        this.dateIndexTracker[dataHolder.projectId] = -1;
        spawnTheta[dataHolder.projectId] = renderDistanceBetweenObjs;
        RenderData(rerender,renderFirst);
    }

    public void ResetData()
    {
        this.spawnTheta.Clear();
        this.loadedProjects.Clear();
        PoolManager.Pools[PoolNames.VERTICE].DespawnAll(); // This removes all vertices from scene :)
        PoolManager.Pools[PoolNames.VERTICE_OUTLINE].DespawnAll(); // This removes all outline vertices from scene :)

        vertices.Clear();
        this.dateIndexTracker.Clear();
        commitPosTracker.Clear();
        wikiPosTracker.Clear();
        filePosTracker.Clear();
        repoPosTracker.Clear();
        
        projectSizesX.Clear();
        projectSizesZ.Clear();

        DOTween.Clear();
    }

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
        ResetData();
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
        currentDateText.text = dateTime + " (Day " + (dateTime.Subtract(loadedProjects[projectId].startDate).Days + 1) + ")";
        foreach (var verticeWrapper in this.loadedProjects[projectId].changesByDate[dateTime])
        {
            Dictionary<VerticeType,List<VerticeData>> dict = new ();
            dict = verticeWrapper.GetRelatedVerticesDict();
            // if (dateTime != DateTime.MinValue.Date || verticeWrapper.verticeData.verticeType == VerticeType.Change)
            // {
            //     dict = verticeWrapper.GetRelatedVerticesDict();
            // }
            // else
            // {
            //     dict[verticeWrapper.verticeData.verticeType] = new List<VerticeData>() { verticeWrapper.verticeData };
            // }

            // We can skip person, we already spawned them
            if(dict.ContainsKey(VerticeType.Ticket))
                SpawnByType(dict[VerticeType.Ticket],projectId);
            
            //TODO commits
            // if(dict.ContainsKey(VerticeType.Commit))
            //     SpawnByType(dict[VerticeType.Commit],projectId);            
            if(dict.ContainsKey(VerticeType.Wiki))
                SpawnByType(dict[VerticeType.Wiki],projectId);
            if(dict.ContainsKey(VerticeType.RepoFile))
                SpawnByType(dict[VerticeType.RepoFile],projectId);
            if(dict.ContainsKey(VerticeType.File))
                SpawnByType(dict[VerticeType.File],projectId);
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
            if(date > dateTime) return;
            RenderDate(date,projectId);
        }
    }

    public void RenderNext(long projectId)
    {
        if (dateIndexTracker[projectId] <= -2) return;
        dateIndexTracker[projectId] += 1;
        if (this.loadedProjects[projectId].orderedDates.Count <= dateIndexTracker[projectId])
        {
            dateIndexTracker[projectId] = -2;
            return;
        }
        
        if (timelineRenderer.datePair.Right != DateTime.MinValue.Date)
        {
            RenderUntilDateWithReset(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]],projectId);
        }
        else
        {
            RenderDate(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]], projectId);
            if (this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]] == DateTime.MinValue.Date)
            {
                RenderNext(projectId);
                return;
            }
            timelineRenderer.SetCurrentDate(this.loadedProjects[projectId].orderedDates[this.dateIndexTracker[projectId]], projectId);
        }

    }

    public void RenderPrevious(long projectId)
    {
        if (dateIndexTracker[projectId] <= 1) return;
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

    private void SpawnByType(List<VerticeData> verticeDatas, long projectId)
    {
        foreach (var verticeData in verticeDatas)
        {
            switch (verticeData.verticeType)
            {
                case VerticeType.Ticket:
                    if (!vertices[projectId].ContainsKey(verticeData.id))
                    {
                        float idk = spawnTheta[projectId];
                        float x = Mathf.Cos(idk) * idk;
                        float y = Mathf.Sin(idk) * idk;
                        Transform obj = SpawnGeneralVertice(projectId, new Vector3(x, 0, y), verticeData.id);
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
                // TODO
                // case VerticeType.Commit:
                //     if (!vertices[projectId].ContainsKey(verticeData.id))
                //     {
                //         float idk = spawnTheta[projectId];
                //         float x = Mathf.Cos(idk) * idk;
                //         float y = Mathf.Sin(idk) * idk;
                //         Transform obj = SpawnGeneralVertice(projectId, new Vector3(x, 0, y), verticeData.id);
                //         spawnTheta[projectId] += renderDistanceBetweenObjs / idk;
                //     }
                //     else
                //     {
                //         VerticeRenderer ver = vertices[projectId][verticeData.id];
                //         float distanceToMove = distanceOnComplete / ver.verticeWrapper.updateCount;
                //         ver.AddCompletedEdge(1L, ver.verticeWrapper.updateCount);
                //         Vector3 newPos = ver.transform.position - new Vector3(0, distanceToMove, 0);
                //         ver.transform.position = newPos;
                //     }
                //
                //     break;
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
                            wikiPosTracker[projectId] = new Vector3(projectSizesX[projectId].Left-(2*spaceBetweenWallObjs), 0,
                                projectSizesZ[projectId].Left);
                        }
                        Transform obj = SpawnGeneralVertice(projectId, wikiPosTracker[projectId], verticeData.id);
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
                                pos.y -= spaceBetweenWallObjs;
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
                            repoPosTracker[projectId] = new Vector3(projectSizesX[projectId].Left, 0,
                                projectSizesZ[projectId].Right+(2*spaceBetweenWallObjs));
                        }
                        Transform obj = SpawnGeneralVertice(projectId, repoPosTracker[projectId], verticeData.id);
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
                            filePosTracker[projectId] = new Vector3(projectSizesX[projectId].Right+(2*spaceBetweenWallObjs), 0,
                                projectSizesZ[projectId].Left);
                        }
                        Transform obj = SpawnGeneralVertice(projectId, filePosTracker[projectId], verticeData.id);
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
        long counter = 1;
        // foreach (long personId in this.loadedProjects[projectId].personIds.Values)
        // {
        //     if (!this.loadedProjects[projectId].verticeData.ContainsKey(personId)) continue;
        //     SpawnGeneralVertice(projectId, new Vector3(counter, 10 + counter, 0), personId);
        //     counter += 2;
        // }
        
        foreach (var (key, value) in this.loadedProjects[projectId].verticeWrappers.Where(x=>x.Value.verticeData.verticeType == VerticeType.Person))
        {
            if (!this.loadedProjects[projectId].verticeData.ContainsKey(key)) continue;
            SpawnGeneralVertice(projectId, new Vector3(counter, 10 + counter, 0), key);
            counter += 2;
        }
    }

    private Transform SpawnGeneralVertice(long projectId, Vector3 spawnPos, long verticeId)
    {
        Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(verticePrefab, spawnPos, Quaternion.identity);

        VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
        verticeRenderer.SetUpReferences(hoverCanvas, hoverElement, hoverText, sidebarController, this);
        VerticeWrapper d = loadedProjects[projectId].verticeWrappers[verticeId];
        verticeRenderer.SetVerticeData(d, projectId, verticeMaterial);
        if (!vertices.ContainsKey(projectId))
        {
            vertices[projectId] = new();
        }
        vertices[projectId][verticeId]= verticeRenderer;
        CheckAndApplyHighlight(projectId, verticeId);
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
        Transform obj = PoolManager.Pools[PoolNames.VERTICE_OUTLINE].Spawn(verticePrefab, new Vector3(x, yPos, z), Quaternion.identity);
        if(obj.TryGetComponent<VerticeRenderer>(out VerticeRenderer vr))
        {
            vr.OnDespawned();
        };
        Destroy(obj.GetComponent<VerticeRenderer>());
        Destroy(obj.GetComponent<BoxCollider>());
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.materials = new[] { outlineMaterial };
        
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