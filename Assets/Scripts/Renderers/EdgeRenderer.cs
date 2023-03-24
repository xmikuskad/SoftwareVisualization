using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class EdgeRenderer : MouseOverRenderer
{
    private MeshRenderer meshRenderer;
    private EdgeData edgeData;

    [Header("References")] private Canvas hoverCanvas;
    private GameObject hoverElement;
    private TMP_Text hoverText;

    public Material hoverMaterial;
    public Material nonHoverMaterial;
    
    private Vector3 offPosition = new(2000, 2000, 2000);
    
    protected new void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    protected new void Start()
    {
        base.Start();
    }

    public override void OnHoverEnter()
    {
        if (edgeData == null)
        {
            return;
        }
        // hoverElement.SetActive(true);

        hoverText.text = edgeData.ToString();
        meshRenderer.material = hoverMaterial;
    }

    public override void OnHoverExit()
    {
        // if(hoverElement) hoverElement.SetActive(false);
        hoverElement.transform.position = offPosition;
        meshRenderer.material = nonHoverMaterial;
    }
    
    public override void OnClick()
    {
        // Debug.Log(edgeData.ToString());
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
    
    // public void SetEdgeData(EdgeData edgeData)
    // {
    //     this.edgeData = edgeData;
    //     // meshRenderer.material.color = Color.blue;
    // }
    
    public void SetEdgeData(EdgeData edgeData, Material material)
    {
        this.edgeData = edgeData;
        this.nonHoverMaterial = material;
        meshRenderer.materials = new[] {material};
    }

    public EdgeData GetEdgeData()
    {
        return this.edgeData;
    }
}
