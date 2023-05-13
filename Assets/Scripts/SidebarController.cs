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

    public TMP_Text infoDefaultContentText;
    public TMP_Text infoDefaultHeaderText;

    public GameObject infoAllContentHolder;
    public GameObject collabMatrix;
    public Button nextDataOverlayBtn;

    public Button prevDataOverlayBtn;
    public CollabBarChart collabBarChart;

    private Pair<VerticeData, VerticeWrapper> currentlyShownObject;

    private List<Pair<VerticeData, VerticeWrapper>> currentlyClickedObjects;

    private long projectId;

    public GameObject ticketCollabBarChart;

    public GameObject contentScrollArea;


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
            if (verticeData1.verticeType == VerticeType.Change)
            {
                slideOutChangeSidebar(projectId, objectToRender.Left, objectToRender.Right);
            }
            if (verticeData1.verticeType == VerticeType.Commit)
            {
                slideOutCommitSidebar(projectId, getWrapperForProjectVerticeId(projectId, objectToRender.Left.id));
            }
        }

        else
        {
            if (verticeWrapper2.verticeData.verticeType == VerticeType.Ticket)
            {
                slideOutTicketSidebar(projectId, verticeWrapper2);
            }
            if (verticeWrapper2.verticeData.verticeType == VerticeType.Person)
            {
                slideOutPersonSidebar(projectId, verticeWrapper2);
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
            renderThisObject(projectId, currentlyClickedObjects[indexOfNext]);
        }
    }

    public void renderPrevObject()
    {
        if (currentlyClickedObjects.Contains(currentlyShownObject))
        {
            int indexOfCurrent = currentlyClickedObjects.IndexOf(currentlyShownObject);
            int indexOfPrev = indexOfCurrent == 0 ? currentlyClickedObjects.Count - 1 : indexOfCurrent - 1;
            renderThisObject(projectId, currentlyClickedObjects[indexOfPrev]);
        }
    }

    // Open
    public void slideOut(long projectId, VerticeWrapper verticeWrapper)
    {
        if (verticeWrapper.verticeData.verticeType == VerticeType.Person)
        {
            slideOutPersonSidebar(projectId, verticeWrapper);
        }
        else if (verticeWrapper.verticeData.verticeType == VerticeType.Ticket)
        {
            slideOutTicketSidebar(projectId, verticeWrapper);
        }
        else if (verticeWrapper.verticeData.verticeType == VerticeType.Change)
        {
            slideOutChangeSidebar(projectId, verticeWrapper.verticeData, null);
        }
        else if (verticeWrapper.verticeData.verticeType == VerticeType.Commit)
        {
            slideOutCommitSidebar(projectId, verticeWrapper);
        }
    }


    public void slideOutPersonSidebar(long projectId, VerticeWrapper verticeWrapper)
    {
        VerticeData verticeData = verticeWrapper.verticeData;
        setContentScrollRectHeight(950f);
        ticketCollabBarChart.gameObject.SetActive(false);
        clearInfoAllContentHolder();
        sidebar.Open();
        verticeType.text = verticeData.verticeType.ToString();
        verticeId.text = "id: " + verticeData.id.ToString();

        addHeaderWithText("name");
        addContentWithText(verticeData.name.ToString() + "\n");

        addHeaderWithText("roles");
        if (verticeData.roles == null || verticeData.roles[0] == "") addContentWithText("NONE\n");
        else addContentWithText("" + string.Join(", ", verticeData.roles.Select(s => s)) + "\n");

        addHeaderWithText("role classes");
        if (verticeData.roleClasses == null || verticeData.roleClasses[0] == "") addContentWithText("NONE\n");
        else addContentWithText("" + string.Join(", ", verticeData.roleClasses.Select(s => s)) + "\n");

        addHeaderWithText("role super classes");
        if (verticeData.roleSuperClasses == null || verticeData.roleSuperClasses[0] == "") addContentWithText("NONE\n");
        else addContentWithText("" + string.Join(", ", verticeData.roleSuperClasses.Select(s => s)) + "\n");

        addHeaderWithText("contributions");
        foreach (VerticeWrapper relatedVerticeW in verticeWrapper.relatedChangesOrCommits.Values)
        {
            VerticeData relatedVerticeD = relatedVerticeW.verticeData;
            if (relatedVerticeD.verticeType == VerticeType.Commit)
            {
                TMP_Text newContent = addContentWithText("" + relatedVerticeD.verticeType.ToString() + " [" + relatedVerticeD.id.ToString() + "] " + relatedVerticeD.title.ToString());
                newContent.GetComponent<Button>().onClick.AddListener(() => slideOutCommitSidebar(projectId, verticeWrapper));
                newContent.GetComponent<Button>().enabled = true;
            }
            if (relatedVerticeD.verticeType == VerticeType.Change)
            {
                TMP_Text newContent = addContentWithText("" + relatedVerticeD.verticeType.ToString() + " [" + relatedVerticeD.id.ToString() + "] " + relatedVerticeD.title.ToString());
                if (relatedVerticeW.GetRelatedVerticesDict().ContainsKey(VerticeType.Ticket))
                {
                    long relatedTicketId = relatedVerticeW.GetRelatedVerticesDict()[VerticeType.Ticket][0].id;
                    newContent.GetComponent<Button>().onClick.AddListener(() => slideOutChangeSidebar(projectId, relatedVerticeD, getWrapperForProjectVerticeId(projectId, relatedTicketId)));
                }
                else
                {
                    newContent.GetComponent<Button>().onClick.AddListener(() => slideOutChangeSidebar(projectId, relatedVerticeD, null));
                }
                newContent.GetComponent<Button>().enabled = true;
            }
        }

    }

    public void slideOutTicketSidebar(long projectId, VerticeWrapper verticeWrapper)
    {
        VerticeData verticeData = verticeWrapper.verticeData;
        setContentScrollRectHeight(600f);
        ticketCollabBarChart.gameObject.SetActive(true);
        clearInfoAllContentHolder();
        collabBarChart.fillBarChart(dataRenderer.loadedProjects[projectId], verticeData.id);
        sidebar.Open();

        verticeType.text = verticeData.verticeType.ToString();

        verticeId.text = "id: " + verticeData.id.ToString();

        addHeaderWithText("title");
        addContentWithText(verticeData.title.ToString() + "\n");

        addHeaderWithText("name");
        addContentWithText(verticeData.name.ToString() + "\n");

        addHeaderWithText("description");
        addContentWithText(verticeData.description.ToString() + "\n");

        addHeaderWithText("created date");
        addContentWithText(verticeData.created.ToString() + "\n");

        addHeaderWithText("type");
        addContentWithText((verticeData.type == null ? "NONE" : verticeData.type[0].ToString()) + "\n");

        addHeaderWithText("contributors");
        if (!verticeWrapper.GetRelatedVerticesDict().ContainsKey(VerticeType.Person))
            addContentWithText("NONE\n");
        else
        {
            List<VerticeData> authorsList = verticeWrapper.GetRelatedVerticesDict()[VerticeType.Person];
            TMP_Text newContent = addContentWithText("" + string.Join(", ", authorsList.Select(s => s.name)) + "\n");
            newContent.GetComponent<Button>().enabled = true;
            VerticeWrapper relatedPerson = dataRenderer.loadedProjects[projectId].verticeWrappers[authorsList[0].id];
            newContent.GetComponent<Button>().onClick.AddListener(() => slideOutPersonSidebar(projectId, relatedPerson));
        }
    }

    public void slideOutChangeSidebar(long projectId, VerticeData changeData, VerticeWrapper ticketData)
    {
        VerticeWrapper changeWrapper = getWrapperForProjectVerticeId(projectId, changeData.id);
        setContentScrollRectHeight(950f);
        ticketCollabBarChart.gameObject.SetActive(false);
        clearInfoAllContentHolder();
        sidebar.Open();

        verticeType.text = changeData.verticeType.ToString();

        verticeId.text = "id: " + changeData.id.ToString();

        addHeaderWithText("title");
        addContentWithText(changeData.title.ToString() + "\n");

        addHeaderWithText("authors");
        if (ticketData == null)
        {
            if (changeData.author == null || changeData.author[0] == null)
                addContentWithText("NONE\n");
            else
            {
                List<VerticeData> authorsList = changeWrapper.GetRelatedVerticesDict()[VerticeType.Person];
                if (authorsList == null || authorsList.Count == 0 || authorsList[0] == null)
                    addContentWithText("NONE\n");
                else
                {
                    TMP_Text authorsContent = addContentWithText("" + string.Join(", ", authorsList.Select(s => s.name)) + "\n");
                    authorsContent.GetComponent<Button>().enabled = true;
                    authorsContent.GetComponent<Button>().onClick.AddListener(() => slideOutPersonSidebar(projectId, getWrapperForProjectVerticeId(projectId, authorsList[0].id)));
                }
            }
        }
        else if (!ticketData.relatedChangesOrCommits[changeData.id].GetRelatedVerticesDict().ContainsKey(VerticeType.Person))
            addContentWithText("NONE\n");
        else
        {
            List<VerticeData> authorsList = ticketData.relatedChangesOrCommits[changeData.id].GetRelatedVerticesDict()[VerticeType.Person];
            TMP_Text newContent = addContentWithText("" + string.Join(", ", authorsList.Select(s => s.name)) + "\n");
            newContent.GetComponent<Button>().enabled = true;
            VerticeWrapper relatedPerson = dataRenderer.loadedProjects[projectId].verticeWrappers[authorsList[0].id];
            newContent.GetComponent<Button>().onClick.AddListener(() => slideOutPersonSidebar(projectId, relatedPerson));
        }

        addHeaderWithText("parent ticket");
        if (ticketData != null)
        {
            TMP_Text parentTicketText = addContentWithText("Ticket [" + ticketData.verticeData.id.ToString() + "] " + ticketData.verticeData.name.ToString() + "\n");
            parentTicketText.GetComponent<Button>().onClick.AddListener(() => slideOutTicketSidebar(projectId, ticketData));
            parentTicketText.GetComponent<Button>().enabled = true;
        }
        else
        {
            addContentWithText("NONE\n");
        }

        addHeaderWithText("created date");
        addContentWithText("" + changeData.created.ToString() + "\n");

        addHeaderWithText("description");
        addContentWithText(changeData.changes.ToString());

        addHeaderWithText("comments");
        addContentWithText(changeData.comment.ToString());

        addHeaderWithText("related artifacts");
        if (ticketData != null)
        {
            foreach (VerticeData relatedVertice in ticketData.relatedChangesOrCommits[changeData.id].GetRelatedVertices())
            {
                if (relatedVertice.verticeType == VerticeType.Person)
                {
                    TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.name.ToString());
                    VerticeWrapper relatedPerson = dataRenderer.loadedProjects[projectId].verticeWrappers[relatedVertice.id];
                    newContent.GetComponent<Button>().onClick.AddListener(() => slideOutPersonSidebar(projectId, relatedPerson));
                    newContent.GetComponent<Button>().enabled = true;
                }
                if (relatedVertice.verticeType == VerticeType.Ticket)
                {
                    TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.name.ToString());
                    newContent.GetComponent<Button>().onClick.AddListener(() => slideOutTicketSidebar(projectId, ticketData));
                    newContent.GetComponent<Button>().enabled = true;
                }
                if (relatedVertice.verticeType == VerticeType.Change)
                {
                    TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.title.ToString());
                    newContent.GetComponent<Button>().onClick.AddListener(() => slideOutChangeSidebar(projectId, relatedVertice, ticketData));
                    newContent.GetComponent<Button>().enabled = true;
                }
                if (relatedVertice.verticeType == VerticeType.Commit)
                {
                    TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.title.ToString());
                    newContent.GetComponent<Button>().onClick.AddListener(() => slideOutCommitSidebar(projectId, getWrapperForProjectVerticeId(projectId, relatedVertice.id)));
                    newContent.GetComponent<Button>().enabled = true;
                }
            }
        }
        else
        {
            foreach (KeyValuePair<VerticeType, List<VerticeData>> relatedVerticePair in changeWrapper.GetRelatedVerticesDict())
            {
                VerticeType relatedVerticeType = relatedVerticePair.Key;
                List<VerticeData> relatedVertices = relatedVerticePair.Value;
                foreach (VerticeData relatedVertice in relatedVertices)
                {
                    if (relatedVertice.verticeType == VerticeType.Person)
                    {
                        TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.name.ToString());
                        VerticeWrapper relatedPerson = dataRenderer.loadedProjects[projectId].verticeWrappers[relatedVertice.id];
                        newContent.GetComponent<Button>().onClick.AddListener(() => slideOutPersonSidebar(projectId, relatedPerson));
                        newContent.GetComponent<Button>().enabled = true;
                    }
                    if (relatedVertice.verticeType == VerticeType.Ticket)
                    {
                        TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.name.ToString());
                        newContent.GetComponent<Button>().onClick.AddListener(() => slideOutTicketSidebar(projectId, ticketData));
                        newContent.GetComponent<Button>().enabled = true;
                    }
                    if (relatedVertice.verticeType == VerticeType.Change)
                    {
                        TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.title.ToString());
                        newContent.GetComponent<Button>().onClick.AddListener(() => slideOutChangeSidebar(projectId, relatedVertice, ticketData));
                        newContent.GetComponent<Button>().enabled = true;
                    }
                    if (relatedVertice.verticeType == VerticeType.Commit)
                    {
                        TMP_Text newContent = addContentWithText("" + relatedVertice.verticeType.ToString() + " [" + relatedVertice.id.ToString() + "] " + relatedVertice.title.ToString());
                        newContent.GetComponent<Button>().onClick.AddListener(() => slideOutCommitSidebar(projectId, getWrapperForProjectVerticeId(projectId, relatedVertice.id)));
                        newContent.GetComponent<Button>().enabled = true;
                    }
                }


            }
        }
    }

    public void slideOutCommitSidebar(long projectId, VerticeWrapper verticeWrapper)
    {
        VerticeData verticeData = verticeWrapper.verticeData;
        setContentScrollRectHeight(950f);
        ticketCollabBarChart.gameObject.SetActive(false);
        clearInfoAllContentHolder();
        sidebar.Open();

        verticeType.text = verticeData.verticeType.ToString();

        verticeId.text = "id: " + verticeData.id.ToString();

        addHeaderWithText("title");
        addContentWithText(verticeData.title.ToString() + "\n");

        addHeaderWithText("name");
        addContentWithText(verticeData.name.ToString() + "\n");

        addHeaderWithText("message");
        addContentWithText(verticeData.message.ToString() + "\n");

        addHeaderWithText("identifier");
        addContentWithText("" + verticeData.identifier.ToString() + "\n");

        addHeaderWithText("created date");
        addContentWithText("" + verticeData.created.ToString() + "\n");

        addHeaderWithText("committed date");
        addContentWithText("" + verticeData.committed.ToString() + "\n");

        addHeaderWithText("changes description");
        addContentWithText("" + verticeData.changes.ToString() + "\n");

        addHeaderWithText("branches");
        if (verticeData.branches != null && verticeData.branches.Length > 0) addContentWithText("" + string.Join(", ", verticeData.branches.Select(s => s)) + "\n");
        else addContentWithText("NONE\n");

        addHeaderWithText("artifacts");
        List<VerticeData> relatedVertices = verticeWrapper.GetRelatedVertices();
        foreach (VerticeData relatedVerticeD in relatedVertices)
        {
            VerticeWrapper relatedVerticeW = getWrapperForProjectVerticeId(projectId, relatedVerticeD.id);

            if (relatedVerticeD.verticeType == VerticeType.Person)
            {
                TMP_Text newContent = addContentWithText("" + relatedVerticeD.verticeType.ToString() + " [" + relatedVerticeD.id.ToString() + "] " + relatedVerticeD.name.ToString());
                newContent.GetComponent<Button>().onClick.AddListener(() => slideOutPersonSidebar(projectId, relatedVerticeW));
                newContent.GetComponent<Button>().enabled = true;
            }
            if (relatedVerticeD.verticeType == VerticeType.Ticket)
            {
                TMP_Text newContent = addContentWithText("" + relatedVerticeD.verticeType.ToString() + " [" + relatedVerticeD.id.ToString() + "] " + relatedVerticeD.name.ToString());
                newContent.GetComponent<Button>().onClick.AddListener(() => slideOutTicketSidebar(projectId, relatedVerticeW));
                newContent.GetComponent<Button>().enabled = true;
            }
            if (relatedVerticeD.verticeType == VerticeType.Change)
            {
                TMP_Text newContent = addContentWithText("" + relatedVerticeD.verticeType.ToString() + " [" + relatedVerticeD.id.ToString() + "] " + relatedVerticeD.name.ToString());
                newContent.GetComponent<Button>().onClick.AddListener(() => slideOutChangeSidebar(projectId, relatedVerticeW.verticeData, null));
                newContent.GetComponent<Button>().enabled = true;
            }
            if (relatedVerticeD.verticeType == VerticeType.Commit)
            {
                TMP_Text newContent = addContentWithText("" + relatedVerticeD.verticeType.ToString() + " [" + relatedVerticeD.id.ToString() + "] " + relatedVerticeD.name.ToString());
                newContent.GetComponent<Button>().onClick.AddListener(() => slideOutCommitSidebar(projectId, relatedVerticeW));
                newContent.GetComponent<Button>().enabled = true;
            }

        }

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

    public void clearInfoAllContentHolder()
    {
        foreach (Transform child in infoAllContentHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void setContentScrollRectHeight(float newHeight)
    {
        Vector2 size = contentScrollArea.GetComponent<RectTransform>().sizeDelta;
        size.y = newHeight;
        contentScrollArea.GetComponent<RectTransform>().sizeDelta = size;
    }

    public TMP_Text addHeaderWithText(string text)
    {
        TMP_Text newHeader = Instantiate(infoDefaultHeaderText, infoDefaultHeaderText.transform.position, Quaternion.identity, infoAllContentHolder.transform);
        newHeader.text = text;
        return newHeader;
    }

    public TMP_Text addContentWithText(string text)
    {
        TMP_Text newContent = Instantiate(infoDefaultContentText, infoDefaultContentText.transform.position, Quaternion.identity, infoAllContentHolder.transform);
        if (text == null || text == "" || text == "\n") newContent.text = "NONE\n";
        else newContent.text = text;
        return newContent;
    }

    public VerticeWrapper getWrapperForProjectVerticeId(long projectId, long verticeId)
    {
        return dataRenderer.loadedProjects[projectId].verticeWrappers[verticeId];
    }
}
