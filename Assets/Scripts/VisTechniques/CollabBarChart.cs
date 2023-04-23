using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;

public class CollabBarChart : MonoBehaviour
{

    public GameObject collabBarChart;

    public TMP_Text defaultLabelElement;

    public Image defaultBarElement;

    public TMP_Text defaultCountElement;

    public GameObject barsHolder;

    public GameObject labelsHolder;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Redo bar chart in regard to specified dataset and ticket id
    // 
    public void fillBarChart(DataHolder dataHolder, long ticketId)
    {
        clearBarChart();

        int personCount = dataHolder.ticketToChangeListPerAuthor[ticketId].Keys.Count;
        int maxChangeCount = 0;
        foreach (var changeList in dataHolder.ticketToChangeListPerAuthor[ticketId].Values)
            if (changeList.Count > maxChangeCount) maxChangeCount = changeList.Count;
        defaultLabelElement.text = "";

        int index = 0;
        foreach (long authorId in dataHolder.ticketToChangeListPerAuthor[ticketId].Keys.OrderBy(i => i))
        {
            int currChangeCount = dataHolder.ticketToChangeListPerAuthor[ticketId][authorId].Count;
            Vector3 pos = defaultLabelElement.transform.position;
            pos.x = pos.x + (260 / ((personCount - 1 < 1 ? 1 : personCount - 1))) * index;
            TMP_Text newLabel = Instantiate(defaultLabelElement, pos, Quaternion.identity, labelsHolder.transform);
            newLabel.gameObject.SetActive(true);
            if (!dataHolder.verticeData.ContainsKey(authorId)) newLabel.text = "??";
            else newLabel.text = dataHolder.verticeData[authorId].name.ToString();

            Vector2 size = defaultBarElement.rectTransform.sizeDelta;
            size.y = (size.y / maxChangeCount) * currChangeCount;
            pos = defaultBarElement.transform.position;
            pos.x = pos.x + (260 / (personCount - 1)) * index;
            pos.y = pos.y - (250 - size.y) / 2;
            Image newBar = Instantiate(defaultBarElement, pos, Quaternion.identity, barsHolder.transform);
            newBar.gameObject.SetActive(true);
            newBar.rectTransform.sizeDelta = size;

            pos = defaultCountElement.transform.position;
            pos.x = pos.x + (260 / (personCount - 1)) * index;
            pos.y = pos.y - (250 - size.y);
            TMP_Text newCount = Instantiate(defaultCountElement, pos, Quaternion.identity, labelsHolder.transform);
            newCount.text = currChangeCount.ToString();
            newCount.gameObject.SetActive(true);

            index += 1;
        }
    }

    public void clearBarChart()
    {
        foreach (Transform child in labelsHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in barsHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
