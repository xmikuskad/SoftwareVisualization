﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataRenderer: MonoBehaviour
{
    public List<DataHolder> loadedProjects = new ();

    private Dictionary<long, Dictionary<long, VerticeRenderer>> vertices = new ();
    private Dictionary<long, Dictionary<long, EdgeRenderer>> edges = new();

    [Header("Properties")] public int instantiatePerFrame = 10;
    
    [Header("References")] 
    public GameObject verticePrefab;
    public GameObject edgePrefab;
    
    public Canvas hoverCanvas;
    public GameObject hoverElement;
    public TMP_Text hoverText;
    
    public GameObject loadingBar;
    public GameObject loadBtn;

    private void Awake()
    {
        // hoverElement = GameObject.FindGameObjectWithTag("HoverElement");
        // hoverCanvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        // hoverText = hoverElement.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void AddData(List<DataHolder> dataHolder, bool rerender)
    {
        SetLoading(true);
        this.loadedProjects.AddRange(dataHolder);
        RenderData(rerender);
    }
    
    public void AddData(DataHolder dataHolder, bool rerender)
    {
        SetLoading(true);
        this.loadedProjects.Add(dataHolder);
        RenderData(rerender);
    }

    public void ResetData()
    {
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
        StartCoroutine(InstantiateObjects(rerender));
    }
    
    IEnumerator InstantiateObjects (bool rerender)
    {

        foreach (var project in loadedProjects)
        {
            if (!rerender && (vertices.ContainsKey(project.projectId) || edges.ContainsKey(project.projectId)))
            {
                continue;
            }
            long x = 0;
            long index = instantiatePerFrame;
            Dictionary<long, VerticeRenderer> verticeTmp = new ();
            foreach (var verticeEl in project.verticeData)
            {
                GameObject obj = Instantiate(verticePrefab, new Vector3(x, 0, 0), Quaternion.identity);
                VerticeRenderer verticeRenderer = obj.GetComponent<VerticeRenderer>();
                verticeRenderer.SetUpReferences(hoverCanvas,hoverElement,hoverText);
                verticeRenderer.verticeData = project.verticeData[verticeEl.Key];
                verticeTmp.Add(verticeRenderer.verticeData.id,verticeRenderer);
                x += 2;
                index--;
                if (index < 0)
                {
                    index = instantiatePerFrame;
                    yield return null;
                }
            }
            vertices.Add(project.projectId,verticeTmp);

            x = 0;
            Dictionary<long, EdgeRenderer> edgeTmp = new ();
            foreach (var edgeEl in project.edgeData)
            {
                GameObject obj = Instantiate(edgePrefab, new Vector3(x, 2, 0), Quaternion.identity);
                EdgeRenderer edgeRenderer = obj.GetComponent<EdgeRenderer>();
                edgeRenderer.SetUpReferences(hoverCanvas,hoverElement,hoverText);
                edgeRenderer.edgeData = project.edgeData[edgeEl.Key];
                edgeTmp.Add(edgeRenderer.edgeData.id,edgeRenderer);
                x += 2;
                index--;
                if (index < 0)
                {
                    index = instantiatePerFrame;
                    yield return null;
                }
            }
            edges.Add(project.projectId,edgeTmp);
        }

        yield return null;
        SetLoading(false);
    }

    // IEnumerator DestroyObjects()
    // {
    //     foreach (var keyPair in vertices)
    //     {
    //         foreach (var keyPairChild in vertices[keyPair.Key])
    //         {
    //             Destroy(vertices[keyPair.Key][keyPairChild.Key].gameObject);
    //             yield return null;
    //         }
    //     }
    //     vertices.Clear();
    //     
    //     foreach (var keyPair in edges)
    //     {
    //         foreach (var keyPairChild in edges[keyPair.Key])
    //         {
    //             Destroy(edges[keyPair.Key][keyPairChild.Key].gameObject);
    //             yield return null;
    //         }
    //     }
    //     edges.Clear();
    // }

    // Manages loading screen
    public void SetLoading(bool status)
    {
        loadingBar.SetActive(status);
        loadBtn.SetActive(!status);
    }
}