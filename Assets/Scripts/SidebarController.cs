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
    public TMP_Text ticketCreatedDate;
    public TMP_Text personInitials;

    public GameObject personData;

    public GameObject ticketData;

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
        if (verticeData.verticeType == VerticeType.Person) {
            slideOutPersonSidebar(verticeData);
        }
        else if (verticeData.verticeType == VerticeType.Ticket) {
            slideOutTicketSidebar(verticeData);
        }
    }


    public void slideOutPersonSidebar(VerticeData verticeData) {
        sidebar.Open();
        focusSidebar(verticeData.verticeType);
        verticeType.text = verticeData.verticeType.ToString();
        verticeId.text = "id: "+verticeData.id.ToString();
        personInitials.text = "initials: "+verticeData.name.ToString();
        personInitials.gameObject.transform.parent.gameObject.SetActive(true);
    }

    public void slideOutTicketSidebar(VerticeData verticeData) {
        sidebar.Open();
        focusSidebar(verticeData.verticeType);
        verticeType.text = verticeData.verticeType.ToString();
        verticeId.text = "id: "+verticeData.id.ToString();
        ticketCreatedDate.text = "created: "+verticeData.start.ToString();
        personInitials.gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void focusSidebar(VerticeType verticeType) {
        if (verticeType == VerticeType.Person) personData.gameObject.SetActive(true);
        else personData.gameObject.SetActive(false);
        if (verticeType == VerticeType.Ticket) ticketData.gameObject.SetActive(true);
        else ticketData.gameObject.SetActive(false);
    }

    // Close
    public void slideIn() {
        sidebar.Close();
    }
}
