using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class EdgeRenderer : MouseOverRenderer
{
    private MeshRenderer meshRenderer;
    public EdgeData edgeData;

    [Header("References")] private Canvas hoverCanvas;
    private GameObject hoverElement;
    private TMP_Text hoverText;

    public Material hoverMaterial;
    public Material nonHoverMaterial;

    protected new void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        base.Start();
    }

    public override void OnHoverEnter()
    {
        if (edgeData == null)
        {
            Debug.Log("IS NULL");
            return;
        }
        hoverElement.SetActive(true);

        hoverText.text = edgeData.ToString();
        meshRenderer.material = hoverMaterial;
    }

    public override void OnHoverExit()
    {
        if(hoverElement) hoverElement.SetActive(false);
        meshRenderer.material = nonHoverMaterial;
    }
    
    public override void OnClick()
    {
        Debug.Log(edgeData.ToString());
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
}
