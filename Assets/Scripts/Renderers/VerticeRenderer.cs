using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VerticeRenderer : MouseOverRenderer
{

    private VerticeData verticeData;
    private MeshRenderer meshRenderer;

    [Header("References")] private Canvas hoverCanvas;
    private GameObject hoverElement;
    private TMP_Text hoverText;

    public Material hoverMaterial;
    public Material nonHoverMaterial;

    private Vector3 offPosition = new (2000, 20000, 2000);


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
        
        hoverText.text = verticeData.ToString();
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
        Debug.Log(verticeData.ToString());
    }

    public void SetUpReferences(Canvas hoverCanvas,GameObject hoverElement,TMP_Text hoverText)
    {
        this.hoverElement = hoverElement;
        this.hoverCanvas = hoverCanvas;
        this.hoverText = hoverText;
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
        this.nonHoverMaterial = material;
        Debug.Log(meshRenderer);
        Debug.Log(meshRenderer.materials);
        meshRenderer.materials = new[] {material};
    }

    public VerticeData GetVerticeData()
    {
        return this.verticeData;
    }
}
