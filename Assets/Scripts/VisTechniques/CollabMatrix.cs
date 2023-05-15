using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;

public class CollabMatrix : MonoBehaviour
{

    public GameObject collabMatrix;

    public GameObject matrixArea;

    public TMP_Text matrixDefaultTextElement;
    public SidebarController sidebarController;

    public ListViewString ticketListView;

    public TMP_Text ticketListViewHeader;

    public GameObject ticketListViewHolder;

    public Image matrixDefaultColorElement;

    private DataHolder currentDataHolder;

    private long currentProjectId;

    private Dictionary<long, Pair<DateTime, DateTime>> projectIdToDateFilters = new Dictionary<long, Pair<DateTime, DateTime>>();


    // Start is called before the first frame update
    void Start()
    {
        collabMatrix = this.gameObject;
        SingletonManager.Instance.dataManager.DateRenderChangedEvent += OnDateRenderChanged;
        SingletonManager.Instance.dataManager.ResetEvent += OnResetEvent;
    }

    private void OnResetEvent(ResetEventReason reason)
    {
        if (reason != ResetEventReason.DATES_UNSELECTED && reason != ResetEventReason.CLICK_OUTSIDE && reason != ResetEventReason.CLEARING_DATES) return;
        projectIdToDateFilters[currentProjectId].Left = DateTime.MinValue;
        projectIdToDateFilters[currentProjectId].Right = DateTime.MaxValue;
        fillMatrix(this.currentDataHolder);
    }

    public void OnDateRenderChanged(Pair<long, Pair<DateTime, DateTime>> pair)
    {
        projectIdToDateFilters[currentProjectId].Left = pair.Right.Left;
        projectIdToDateFilters[currentProjectId].Right = pair.Right.Right;
        fillMatrix(this.currentDataHolder);
    }

    private void removeCurrentMatrix()
    {
        for (int i = 0; i < matrixArea.transform.childCount; i++)
        {
            if (i > 1)
            {
                GameObject.Destroy(matrixArea.transform.GetChild(i).gameObject);
            }
        }

        // foreach (Transform child in matrixArea.transform)
        // {
        //     GameObject.Destroy(child.gameObject);
        // }
    }


    public void fillMatrix(DataHolder dataHolder)
    {
        currentProjectId = dataHolder.projectId;
        if (!projectIdToDateFilters.ContainsKey(dataHolder.projectId))
            projectIdToDateFilters[dataHolder.projectId] = new Pair<DateTime, DateTime>(DateTime.MinValue, DateTime.MaxValue);

        removeCurrentMatrix();
        this.currentDataHolder = dataHolder;

        // Get list of collaborants
        List<string> collaborants = new List<string>();
        List<VerticeData> collaborants2 = new List<VerticeData>();
        foreach (KeyValuePair<long, VerticeData> vertice in dataHolder.verticeData)
        {
            if (vertice.Value.verticeType == VerticeType.Person)
            {
                collaborants.Add(vertice.Value.name.ToString());
                collaborants2.Add(vertice.Value);
            };
        }

        // Generate collaborant labels in canvas
        int xOffset = 530 / collaborants2.Count;
        int yOffset = -530 / collaborants2.Count;
        int helpIndex = 0;
        foreach (VerticeData collaborant in collaborants2)
        {

            int helpIndex2 = helpIndex;

            Vector3 pos = matrixDefaultTextElement.transform.position;
            pos.x = pos.x + xOffset * helpIndex + xOffset;
            TMP_Text newText = Instantiate(matrixDefaultTextElement, pos, Quaternion.identity, matrixArea.transform);
            // newText.text = collaborant.name;
            newText.text = DataUtils.PersonNameToInitials(collaborant.name);
            if (collaborant.name == "unknown") newText.text = "??";
            newText.GetComponent<Button>().onClick.AddListener(() => onClickPerson(dataHolder.verticeWrappers[helpIndex2]));

            pos = matrixDefaultTextElement.transform.position;
            pos.y = pos.y + yOffset * helpIndex + yOffset;
            TMP_Text newText2 = Instantiate(matrixDefaultTextElement, pos, Quaternion.identity, matrixArea.transform);
            newText2.text = DataUtils.PersonNameToInitials(collaborant.name);
            if (collaborant.name == "unknown") newText2.text = "??";
            newText2.GetComponent<Button>().onClick.AddListener(() => onClickPerson(dataHolder.verticeWrappers[helpIndex2]));

            helpIndex++;
        }

        // All ticket IDs in data
        HashSet<long> ticketIds = dataHolder.verticeData.Values.Where(e => e.verticeType == VerticeType.Ticket).Select(i => i.id).ToHashSet();

        // Dictionary<long, List<long>> ticketChangesIDs = new Dictionary<long, List<long>>();
        Dictionary<long, List<long>> ticketChangesIDs = ticketIds.ToDictionary(h => h, h => new List<long>());

        // Dict where key is ticket ID and value is a list of contributor names to that ticket
        Dictionary<long, List<string>> ticketContributorsStrs = ticketIds.ToDictionary(h => h, h => new List<string>());

        // Dict where key is ticket ID and value is a list of contributor vertices to that ticket
        Dictionary<long, List<VerticeData>> ticketContributors = ticketIds.ToDictionary(h => h, h => new List<VerticeData>());

        // Fill the helping dicts
        foreach (EdgeData edge in dataHolder.edgeData.Values)
        {
            if (!ticketIds.Contains(edge.from)) continue;
            if (dataHolder.verticeData[edge.to].verticeType != VerticeType.Change) continue;
            VerticeData ticket = dataHolder.verticeData[edge.from];
            VerticeData change = dataHolder.verticeData[edge.to];
            // If either change or ticket should be filtered out, just filter it out
            if (!ticket.IsDateBetween(projectIdToDateFilters[dataHolder.projectId].Left, projectIdToDateFilters[dataHolder.projectId].Right)) continue;
            if (!change.IsDateBetween(projectIdToDateFilters[dataHolder.projectId].Left, projectIdToDateFilters[dataHolder.projectId].Right)) continue;
            // If change ID not yet logged, log it in ticketChangesIDs
            if (!ticketChangesIDs[ticket.id].Contains(change.id)) ticketChangesIDs[ticket.id].Add(change.id);
            // If author is unknown and is not yet logged, log it in ticketContributors as ?
            if ((change.author == null || change.author[0] == null) && !ticketContributorsStrs[ticket.id].Contains("unknown"))
            {
                ticketContributorsStrs[ticket.id].Add("unknown");
            }
            // If author is not unknown and is not yet logged, log it in ticketContributors
            else if (change.author != null && change.author[0] != null && !ticketContributorsStrs[ticket.id].Contains(change.author[0]))
            {
                ticketContributorsStrs[ticket.id].Add(change.author[0]);
            }
        }

        // Get confusion matrix
        int[,] matrixValues = new int[collaborants2.Count, collaborants2.Count];
        foreach (List<string> contributors in ticketContributorsStrs.Values)
        {
            if (contributors.Count < 2) continue;
            for (int i = 0; i < contributors.Count; i++)
            {
                for (int j = i + 1; j < contributors.Count; j++)
                {
                    matrixValues[collaborants.IndexOf(contributors[i]), collaborants.IndexOf(contributors[j])] += 1;
                    matrixValues[collaborants.IndexOf(contributors[j]), collaborants.IndexOf(contributors[i])] += 1;
                }
            }
        }
        int min = matrixValues.Cast<int>().Min();
        int max = matrixValues.Cast<int>().Max();

        // Generate conf matrix representation in canvas
        for (int i = 0; i < matrixValues.GetLength(0); i++)
        {
            for (int j = 0; j < matrixValues.GetLength(1); j++)
            {
                Vector3 pos = matrixDefaultTextElement.transform.position;
                pos.x = pos.x + xOffset * i + xOffset;
                pos.y = pos.y + yOffset * j + yOffset;
                Image newImage2 = Instantiate(matrixDefaultColorElement, pos, Quaternion.identity, matrixArea.transform);
                // current number - matrixValues[i,j]
                float r = (matrixValues[i, j] < max / 2 ? 0.5f : 0.5f - 0.5f / max * matrixValues[i, j]);
                float g = (matrixValues[i, j] < max / 2 ? 0.5f / max * matrixValues[i, j] * 2 : 0.5f);
                newImage2.color = new Color(r, g, 0.1f, 1.0f);
                if (i == j) newImage2.color = new Color(0.15294f, 0.15686f, 0.16863f, 1.0f);
                TMP_Text newText = Instantiate(matrixDefaultTextElement, pos, Quaternion.identity, matrixArea.transform);
                newText.text = matrixValues[i, j].ToString();
                newText.fontSize = 30 - collaborants2.Count;
                if (i == j) continue;
                List<VerticeWrapper> relatedTickets = new List<VerticeWrapper>();
                int k = i;
                int l = j;
                foreach (KeyValuePair<long, List<string>> ticketCon in ticketContributorsStrs)
                {
                    // TODO filter aj zde
                    if (ticketCon.Value.Contains(collaborants[k]) && ticketCon.Value.Contains(collaborants[l]))
                    {
                        relatedTickets.Add(dataHolder.verticeWrappers[ticketCon.Key]);
                    }
                }
                newText.GetComponent<Button>().onClick.AddListener(() => onClickTicketList(relatedTickets, collaborants[k], collaborants[l], dataHolder.projectId));
            }
        }
    }

    public void close()
    {
        collabMatrix.SetActive(false);
        ticketListViewHolder.SetActive(false);
    }

    public void onClickPerson(VerticeWrapper verticeWrapper)
    {
        sidebarController.slideOutPersonSidebar(currentDataHolder.projectId, verticeWrapper);
        SingletonManager.Instance.dataManager.InvokeVerticeSelect(new List<Pair<VerticeData, VerticeWrapper>>() { new(null, verticeWrapper) });
    }

    public void onClickTicketList(List<VerticeWrapper> relatedTickets, string a, string b, long projectId)
    {
        ticketListViewHeader.text = "Collaborations " + (a.Equals("unknown") ? "??" : DataUtils.PersonNameToInitials(a)) + " - " +
            (b.Equals("unknown") ? "??" : DataUtils.PersonNameToInitials(b));
        ticketListView.Clear();
        foreach (VerticeWrapper relatedTicket in relatedTickets)
        {
            ticketListView.Add(relatedTicket.verticeData.id + " " + relatedTicket.verticeData.name);
        }
        ticketListView.ItemsEvents.PointerClick.RemoveAllListeners();
        ticketListView.ItemsEvents.PointerClick.AddListener((x, y, z) => onClickTicket(relatedTickets, x));
        ticketListViewHolder.SetActive(true);
        
        SingletonManager.Instance.dataManager.InvokeVerticeSelect(relatedTickets.Select(x => new Pair<VerticeData, VerticeWrapper>(null, x)).ToList());
    }

    public void onClickTicket(List<VerticeWrapper> ticketsInList, int indexOfClicked)
    {
        sidebarController.slideOutTicketSidebar(currentDataHolder.projectId, ticketsInList[indexOfClicked]);
        SingletonManager.Instance.dataManager.InvokeVerticeSelect(new List<Pair<VerticeData, VerticeWrapper>>() { new(null, ticketsInList[indexOfClicked]) });
    }

    public void writeDebugClicked()
    {
        Debug.Log("CLICKED!!!");
    }
}
