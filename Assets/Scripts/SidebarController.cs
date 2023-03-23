using System.Collections;
using System.Collections.Generic;
using UIWidgets;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SidebarController : MonoBehaviour
{

    [SerializeField] private LayerMask layerMaskToIgnore;
  
    public Sidebar sidebar;

    public TMP_Text verticeType;
    public TMP_Text verticeId;
    public TMP_Text verticeCreatedDate;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMaskToIgnore) || isOverUI)
            {
                // Debug.Log("Hit " + hit.collider.gameObject.name);
            }
            else
            {
                this.slideIn();
            }
        }
    }

    // Open
    public void slideOut(VerticeData verticeData) {
        sidebar.Open();
        verticeType.text = verticeData.verticeType.ToString();
        verticeId.text = "id: "+verticeData.id.ToString();
        verticeCreatedDate.text = "created: "+verticeData.created.ToString();
        // Debug.Log(verticeData);
    }

    // Close
    public void slideIn() {
        sidebar.Close();
    }
}
