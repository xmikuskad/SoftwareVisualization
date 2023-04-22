using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class VerticeRenderer : MouseOverRenderer
{

    private VerticeData verticeData;
    private MeshRenderer meshRenderer;

    private long completedCount = 0;
    private long taskCount = 0;

    [Header("References")] private Canvas hoverCanvas;
    private GameObject hoverElement;
    private TMP_Text hoverText;

    public Material hoverMaterial;
    public Material nonHoverMaterial;

    private Vector3 offPosition = new (2000, 20000, 2000);

    private SidebarController sidebarScript;


    protected void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    protected new void Start()
    {
        base.Start();
    }

    public override void OnHoverEnter()
    {
        if (verticeData == null) {
            return;
        }
        // hoverElement.SetActive(true);
        
        // hoverText.text = verticeData.ToString();
        if (verticeData.verticeType == VerticeType.Ticket)
        {
            hoverText.text = completedCount + " / " + taskCount;
        }
        else
        {
            hoverText.text = verticeData.ToString();
        }

        meshRenderer.sharedMaterial = hoverMaterial;
    }

    public override void OnHoverExit()
    {
        // if(hoverElement) hoverElement.SetActive(false);
        hoverElement.transform.position = offPosition;
        meshRenderer.material = nonHoverMaterial;
    }
    
    public override void OnClick()
    {
        // Debug.Log(verticeData.ToString());
        sidebarScript.slideOut(verticeData);
    }

    public void SetUpReferences(Canvas hoverCanvas,GameObject hoverElement,TMP_Text hoverText, SidebarController sidebarController)
    {
        this.hoverElement = hoverElement;
        this.hoverCanvas = hoverCanvas;
        this.hoverText = hoverText;
        this.sidebarScript = sidebarController;
    }

    public override Canvas GetCanvas()
    {
        return hoverCanvas;
    }

    public override GameObject GetHoverObject()
    {
        return hoverElement;
    }

    public void SetVerticeData(VerticeData verticeData, Material material)
    {
        this.verticeData = verticeData;
        
        // We need to duplicate material because otherwise all objects with that material will be changed
        Material newMat = new Material(material);
        if (verticeData.verticeType == VerticeType.Ticket)
        {
            newMat.color = new Color(1f, 0f, 0f);
        }

        meshRenderer.materials = new[] {newMat};
        
        this.nonHoverMaterial = newMat;
    }

    public void AddCompletedEdge(long count, long maxTasks)
    {
        this.completedCount+=count;
        this.taskCount = maxTasks;
        this.nonHoverMaterial.DOColor(
            GetColorFromRedYellowGreenGradient((this.completedCount * 1.0f) / (maxTasks * 1.0f) * 100f), SingletonManager.Instance.animationManager.GetColorChangeAnimTime());
        // this.nonHoverMaterial.color = GetColorFromRedYellowGreenGradient((this.completedCount*1.0f) / (maxTasks * 1.0f) * 100f);
    }

    public VerticeData GetVerticeData()
    {
        return this.verticeData;
    }
    
    private Color GetColorFromRedYellowGreenGradient(float percentage)
    {
        // Green colors too similar, taken from https://stackoverflow.com/questions/6394304/algorithm-how-do-i-fade-from-red-to-green-via-yellow-using-rgb-values
        // float red = (percentage > 50f ? 1f - 2f * (percentage - 50f) / 100.0f : 1.0f);
        // float green = (percentage > 50f ? 1.0f : 2f * percentage / 100.0f);
        // float blue = 0.0f;
        // Color result = new Color(red, green, blue);
        // return result;

        if (percentage > 99.9f)
        {
            return new Color(0f, 0f, 0f);
            // return new Color(0f, 1f, 0f);
        }

        return new Color((percentage > 50f ? 1f - 2f * (percentage - 50f) / 100.0f : 1.0f), (percentage > 50f ? 1.0f : 2f * percentage / 100.0f), 0f);
        // return new Color((percentage > 50f ? 1f - 2f * (percentage - 50f) / 100.0f : 1.0f), (percentage > 50f ? 0.6f : 2f * percentage / 100.0f), 0f);
    }
}
