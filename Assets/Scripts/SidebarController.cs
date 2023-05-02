using UIWidgets;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Helpers;
using System.Collections.Generic;
using Data;
using System.Linq;

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

    public CollabBarChart collabBarChart;

    private List<VerticeWrapper> verticesCurrentlyInSidebar = new List<VerticeWrapper>();


    private void Start()
    {
        SingletonManager.Instance.dataManager.VerticesSelectedEvent += OnVerticeSelected;
        SingletonManager.Instance.dataManager.ResetEvent += OnAllVerticesDeselected;
    }
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
                SingletonManager.Instance.dataManager.InvokeResetEvent();
            }
        }
    }


    private void OnAllVerticesDeselected()
    {
        // Debug.Log("nothing selected");
        slideIn();
    }
    private void OnVerticeSelected(Pair<long, List<VerticeWrapper>> pair)
    {
        // Debug.Log("currently " + pair.Right.Count.ToString() + " onClicked vertices");

        // foreach (VerticeWrapper verticeWrapper in pair.Right)
        // {
        //     if (!verticesCurrentlyInSidebar.Contains(verticeWrapper))
        //     {
        //         verticesCurrentlyInSidebar.Add(verticeWrapper);
        //         addVerticeToSidebarPages(verticeWrapper);
        //     }
        // }

        // if (verticesCurrentlyInSidebar.Count == 0)
        // {
        //     addVerticesToSidebarPages(pair.Right);
        // }

        List<VerticeWrapper> newClickedVertices = pair.Right.Except(verticesCurrentlyInSidebar).ToList();
        foreach (VerticeWrapper diff in newClickedVertices)
            if (pair.Right.Contains(diff) && !verticesCurrentlyInSidebar.Contains(diff))
                addVerticeToSidebarPages(diff);

        List<VerticeWrapper> unclickedVertices = verticesCurrentlyInSidebar.Except(pair.Right).ToList();
        foreach (VerticeWrapper diff in newClickedVertices)
            if (!pair.Right.Contains(diff) && verticesCurrentlyInSidebar.Contains(diff))
                removeVerticeFromSidebarPages(diff);
    }

    // Open
    public void slideOut(long projectId, VerticeData verticeData)
    {
        if (verticeData.verticeType == VerticeType.Person)
        {
            slideOutPersonSidebar(projectId, verticeData);
        }
        else if (verticeData.verticeType == VerticeType.Ticket)
        {
            slideOutTicketSidebar(projectId, verticeData);
        }
    }


    public void slideOutPersonSidebar(long projectId, VerticeData verticeData)
    {
        sidebar.Open();
        focusSidebar(verticeData.verticeType);
        verticeType.text = verticeData.verticeType.ToString();
        verticeId.text = "id: " + verticeData.id.ToString();
        personInitials.text = "initials: " + verticeData.name.ToString();
        personInitials.gameObject.transform.parent.gameObject.SetActive(true);
    }

    public void slideOutTicketSidebar(long projectId, VerticeData verticeData)
    {
        collabBarChart.fillBarChart(dataRenderer.loadedProjects[projectId], verticeData.id);
        sidebar.Open();
        focusSidebar(verticeData.verticeType);
        verticeType.text = verticeData.verticeType.ToString();
        verticeId.text = "id: " + verticeData.id.ToString();
        ticketCreatedDate.text = "created: " + verticeData.start.ToString();
        ticketDueDate.text = "due: " + verticeData.due.ToString();
        personInitials.gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void focusSidebar(VerticeType verticeType)
    {
        if (verticeType == VerticeType.Person) personData.gameObject.SetActive(true);
        else personData.gameObject.SetActive(false);
        if (verticeType == VerticeType.Ticket) ticketData.gameObject.SetActive(true);
        else ticketData.gameObject.SetActive(false);
    }

    // Close
    public void slideIn()
    {
        sidebar.Close();
    }

    public void showCollabMatrix()
    {
        collabMatrix.SetActive(true);
    }

    public void addVerticesToSidebarPages(List<VerticeWrapper> verticeWrappers)
    {
        verticesCurrentlyInSidebar.AddRange(verticeWrappers);
        // Debug.Log("adding vertice " + verticeWrappers[0].verticeData.id.ToString() + " to sidebar pages");
    }

    public void addVerticeToSidebarPages(VerticeWrapper verticeWrapper)
    {
        verticesCurrentlyInSidebar.Add(verticeWrapper);
        slideOut(1, verticeWrapper.verticeData);
        // Debug.Log("adding vertice " + verticeWrapper.verticeData.id.ToString() + " to sidebar pages");
    }

    public void removeVerticeFromSidebarPages(VerticeWrapper verticeWrapper)
    {
        verticesCurrentlyInSidebar.Remove(verticeWrapper);
        // Debug.Log("removing vertice " + verticeWrapper.verticeData.id.ToString() + " from sidebar pages");
    }
}
