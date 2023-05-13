using Helpers;
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

    public GameObject personData;
    public GameObject ticketData;
    public GameObject changeData;
    public TMP_Text personInitials;

    public TMP_Text ticketCreatedDate;
    public TMP_Text ticketDueDate;

    public GameObject overlayDataHolder;
    public GameObject collabMatrix;
    public Button nextDataOverlayBtn;

    public Button prevDataOverlayBtn;
    public CollabBarChart collabBarChart;

    private Pair<VerticeData, VerticeWrapper> currentlyShownObject;

    private List<Pair<VerticeData, VerticeWrapper>> currentlyClickedObjects;

    private long projectId;


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
                SingletonManager.Instance.dataManager.InvokeResetEvent(ResetEventReason.CLICK_OUTSIDE);
            }
        }
    }


    private void OnAllVerticesDeselected(ResetEventReason reason)
    {
        // Debug.Log("nothing selected");
        slideIn();
    }

    // project ID, list vsetkych objektov ktore su oznacene <commit/change/null, ticket/person/repo/file/wiki>
    private void OnVerticeSelected(Pair<long, List<Pair<VerticeData, VerticeWrapper>>> pair)
    {
        projectId = pair.Left;
        currentlyClickedObjects = pair.Right;

        if (currentlyClickedObjects.Count == 0) { slideIn(); }
        if (currentlyClickedObjects.Count == 1)
        {
            deactivateOverlayButtons();
        }
        if (currentlyClickedObjects.Count > 1)
        {
            activateOverlayButtons();
        }

        currentlyShownObject = currentlyClickedObjects.Last();
        renderThisObject(projectId, currentlyShownObject);

    }

    public void renderThisObject(long projectId, Pair<VerticeData, VerticeWrapper> objectToRender)
    {
        VerticeData verticeData1 = objectToRender.Left;
        VerticeWrapper verticeWrapper2 = objectToRender.Right;
        currentlyShownObject = objectToRender;

        if (verticeData1 != null)
        {
            if (verticeData1.verticeType == VerticeType.Change) Debug.Log("show on CHANGE id " + verticeData1.id.ToString());
            if (verticeData1.verticeType == VerticeType.Commit) Debug.Log("show on COMMIT id " + verticeData1.id.ToString());
        }

        else
        {
            if (verticeWrapper2.verticeData.verticeType == VerticeType.Ticket)
            {
                Debug.Log("show on TICKET " + verticeWrapper2.verticeData.id.ToString());
                slideOut(projectId, verticeWrapper2.verticeData);
            }
            if (verticeWrapper2.verticeData.verticeType == VerticeType.Person)
            {
                Debug.Log("show on PERSON " + verticeWrapper2.verticeData.id.ToString());
                slideOut(projectId, verticeWrapper2.verticeData);
            }
            if (verticeWrapper2.verticeData.verticeType == VerticeType.RepoFile) Debug.Log("show on REPOFILE " + verticeWrapper2.verticeData.id.ToString());
            if (verticeWrapper2.verticeData.verticeType == VerticeType.File) Debug.Log("show on FILE " + verticeWrapper2.verticeData.id.ToString());
            if (verticeWrapper2.verticeData.verticeType == VerticeType.Wiki) Debug.Log("show on WIKI " + verticeWrapper2.verticeData.id.ToString());
        }
    }

    public void renderNextObject()
    {
        if (currentlyClickedObjects.Contains(currentlyShownObject))
        {
            int indexOfCurrent = currentlyClickedObjects.IndexOf(currentlyShownObject);
            int indexOfNext = indexOfCurrent == currentlyClickedObjects.Count - 1 ? 0 : indexOfCurrent + 1;
            // Debug.Log("RENDER NEXT ON INDEX " + indexOfNext.ToString());
            renderThisObject(projectId, currentlyClickedObjects[indexOfNext]);
        }
    }

    public void renderPrevObject()
    {
        if (currentlyClickedObjects.Contains(currentlyShownObject))
        {
            int indexOfCurrent = currentlyClickedObjects.IndexOf(currentlyShownObject);
            int indexOfPrev = indexOfCurrent == 0 ? currentlyClickedObjects.Count - 1 : indexOfCurrent - 1;
            // Debug.Log("RENDER PREV ON INDEX " + indexOfPrev.ToString());
            renderThisObject(projectId, currentlyClickedObjects[indexOfPrev]);
        }
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


    public void clearSidebarVertices()
    {

    }

    public void deactivateOverlayButtons()
    {
        nextDataOverlayBtn.gameObject.SetActive(false);
        prevDataOverlayBtn.gameObject.SetActive(false);
    }

    public void activateOverlayButtons()
    {
        nextDataOverlayBtn.gameObject.SetActive(true);
        prevDataOverlayBtn.gameObject.SetActive(true);
    }

}
