using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
   

    // Start is called before the first frame update
    void Start()
    {
        collabMatrix = this.gameObject;
    }


    public void fillMatrix(DataHolder dataHolder) {

        // Get list of collaborants
        List<string> collaborants = new List<string>();
        foreach (KeyValuePair<long,VerticeData> vertice in dataHolder.verticeData) {
            if (vertice.Value.verticeType == VerticeType.Person) {
                if (vertice.Value.name.ToString() == "unknown") {
                    collaborants.Add("?");
                }
                else {
                    collaborants.Add(vertice.Value.name.ToString());
                }
            };
        }

        // Generate collaborant labels in canvas
        int xOffset = 530/collaborants.Count;
        int yOffset = -530/collaborants.Count;
        for (int i = 0; i < collaborants.Count; i++) {
            int j = i;

            Vector3 pos = matrixDefaultTextElement.transform.position;
            pos.x = pos.x + xOffset*i + xOffset;
            TMP_Text newText = Instantiate(matrixDefaultTextElement, pos, Quaternion.identity, matrixArea.transform);
            newText.text = collaborants[i];
            newText.GetComponent<Button>().onClick.AddListener(() => onClickPerson(dataHolder.verticeData[j]));
            
            pos = matrixDefaultTextElement.transform.position;
            pos.y = pos.y + yOffset*i + yOffset;
            TMP_Text newText2 = Instantiate(matrixDefaultTextElement, pos, Quaternion.identity, matrixArea.transform);
            newText2.text = collaborants[i];
            newText2.GetComponent<Button>().onClick.AddListener(() => onClickPerson(dataHolder.verticeData[j]));
        }

        // All ticket IDs in data
        HashSet<long> ticketIds = dataHolder.verticeData.Values.Where(e=>e.verticeType == VerticeType.Ticket).Select(i=>i.id).ToHashSet();

        // Dictionary<long, List<long>> ticketChangesIDs = new Dictionary<long, List<long>>();
        Dictionary<long, List<long>> ticketChangesIDs = ticketIds.ToDictionary(h => h, h => new List<long>());

        // Dict where key is ticket ID and value is a list of contributors to that ticket
        Dictionary<long, List<string>> ticketContributors = ticketIds.ToDictionary(h => h, h => new List<string>());

        // Fill the helping dicts
        foreach (EdgeData edge in dataHolder.edgeData.Values) {
            if (!ticketIds.Contains(edge.from)) continue;
            if (dataHolder.verticeData[edge.to].verticeType != VerticeType.Change) continue;
            VerticeData ticket = dataHolder.verticeData[edge.from];
            VerticeData change = dataHolder.verticeData[edge.to];
            // If change ID not yet logged, log it in ticketChangesIDs
            if (!ticketChangesIDs[ticket.id].Contains(change.id)) ticketChangesIDs[ticket.id].Add(change.id);
            // If author is unknown and is not yet logged, log it in ticketContributors as ?
            if ((change.author == null || change.author[0] == null) && !ticketContributors[ticket.id].Contains("?")) {
                ticketContributors[ticket.id].Add("?");
            }            
            // If author is not unknown and is not yet logged, log it in ticketContributors
            else if (change.author != null && change.author[0] != null && !ticketContributors[ticket.id].Contains(change.author[0])) {
                ticketContributors[ticket.id].Add(change.author[0]);
            }
        }

        // Get confusion matrix
        int[,] matrixValues = new int[collaborants.Count, collaborants.Count];
        foreach (List<string> contributors in ticketContributors.Values) {
            if (contributors.Count < 2) continue;
            for (int i = 0; i < contributors.Count; i++) {
                for (int j = i+1; j < contributors.Count; j++) {
                    matrixValues[collaborants.IndexOf(contributors[i]), collaborants.IndexOf(contributors[j])] += 1;
                    matrixValues[collaborants.IndexOf(contributors[j]), collaborants.IndexOf(contributors[i])] += 1;
                }
            }
        }        
        int min = matrixValues.Cast<int>().Min();
        int max = matrixValues.Cast<int>().Max();

        // Generate conf matrix representation in canvas
        for (int i = 0; i < matrixValues.GetLength(0); i++) {
            for (int j = 0; j < matrixValues.GetLength(1); j++) {
                Vector3 pos = matrixDefaultTextElement.transform.position;
                pos.x = pos.x + xOffset*i + xOffset;
                pos.y = pos.y + yOffset*j + yOffset;
                Image newImage2 = Instantiate(matrixDefaultColorElement, pos, Quaternion.identity, matrixArea.transform);
                // current number - matrixValues[i,j]
                float r = (matrixValues[i,j] < max/2 ? 0.5f : 0.5f - 0.5f/max*matrixValues[i,j]);
                float g = (matrixValues[i,j] < max/2 ? 0.5f/max*matrixValues[i,j]*2 : 0.5f);
                newImage2.color = new Color(r, g, 0.1f, 1.0f);
                if (i == j) newImage2.color = new Color(0.15294f, 0.15686f, 0.16863f, 1.0f);
                TMP_Text newText = Instantiate(matrixDefaultTextElement, pos, Quaternion.identity, matrixArea.transform);
                newText.text = matrixValues[i,j].ToString();
                newText.fontSize = 30 - collaborants.Count;
                if (i == j) continue;
                List<VerticeData> relatedTickets = new List<VerticeData>();
                int k = i;
                int l = j;
                foreach (KeyValuePair<long, List<string>> ticketCon in ticketContributors) {
                    if (ticketCon.Value.Contains(collaborants[k]) && ticketCon.Value.Contains(collaborants[l])) {
                        relatedTickets.Add(dataHolder.verticeData[ticketCon.Key]);
                    }
                }
                newText.GetComponent<Button>().onClick.AddListener(() => onClickTicketList(relatedTickets, collaborants[k], collaborants[l]));
            }
        }
    }

    public void close() {
        collabMatrix.SetActive(false);
        ticketListViewHolder.SetActive(false);
    }

    public void onClickPerson(VerticeData clickedPerson) {
        sidebarController.slideOutPersonSidebar(clickedPerson);
    }

    public void onClickTicketList(List<VerticeData> relatedTickets, string a, string b) {
        ticketListViewHeader.text = "Collaborations " + a + " - " + b;
        ticketListView.Clear();
        foreach (VerticeData relatedTicket in relatedTickets) {
            ticketListView.Add(relatedTicket.id + " " + relatedTicket.name);
        }
        ticketListView.ItemsEvents.PointerClick.RemoveAllListeners();
        ticketListView.ItemsEvents.PointerClick.AddListener((x,y,z) => onClickTicket(relatedTickets, x));
        ticketListViewHolder.SetActive(true);
    }

    public void onClickTicket(List<VerticeData> ticketsInList, int indexOfClicked) {
        sidebarController.slideOutTicketSidebar(ticketsInList[indexOfClicked]);
    }

    public void writeDebugClicked() {
        Debug.Log("CLICKED!!!");
    }
}
