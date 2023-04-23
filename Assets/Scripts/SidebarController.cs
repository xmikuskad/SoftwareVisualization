using System.Collections;
using System.Collections.Generic;
using UIWidgets;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SidebarController : MonoBehaviour
{

    [SerializeField] private LayerMask layerMaskToIgnore;

    public DataRenderer dataRenderer;
  
    public Sidebar sidebar;

    public TMP_Text verticeType;
    public TMP_Text verticeId;

    public TMP_Text personInitials;
    public TMP_Text ticketCreatedDate;
    public TMP_Text ticketDueDate;
    public GameObject personData;
    public GameObject ticketData;

    public GameObject collabMatrix;

    public Button showCollabMatrixBtn;

    void Update()
    {
        // TODO this should be moved somewhere ?
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
                SingletonManager.Instance.dataManager.Unhighlight();
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
        ticketDueDate.text = "due: "+verticeData.due.ToString();
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

    public void showCollabMatrix() {
        collabMatrix.SetActive(true);
    }

}
