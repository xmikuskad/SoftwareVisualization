using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class VerticePlatformRenderer : MouseOverRenderer
{
    public long projectId = -1;
    public VerticeWrapper verticeWrapper;
    private MeshRenderer meshRenderer;

    protected void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetUp(long projectId, VerticeWrapper verticeWrapper)
    {
        this.projectId = projectId;
        this.verticeWrapper = verticeWrapper;
    }

    public override void OnHoverEnter()
    {
    }

    public override void OnHoverExit()
    {
    }

    public override void OnClick()
    {
        // Debug.Log("Clicked " + projectId + " and vertice " + this.verticeWrapper.verticeData.id);
        // SingletonManager.Instance.dataManager.ProcessVerticeClick(this.projectId, this.verticeWrapper);
    }

    public override Canvas GetCanvas()
    {
        return null;
    }

    public override GameObject GetHoverObject()
    {
        return null;
    }
}
