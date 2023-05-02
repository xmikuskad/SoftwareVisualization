using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using E2C;

public class KiviatDiagram : MonoBehaviour
{
    public GameObject defaultPersonToggleElement;

    public GameObject personToggleElementHolder;

    public GameObject kiwiatDiagramArea;

    public GameObject kiviatDiagramMain;

    private Dictionary<long, VerticeData> allPersons = new Dictionary<long, VerticeData>();

    private List<VerticeData> shownPersons = new List<VerticeData>();

    private Dictionary<long, Dictionary<String, float>> personIdToMetricIdToValue = new Dictionary<long, Dictionary<String, float>>();

    public E2Chart e2chart;

    private E2ChartData e2ChartData;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void initiateKiviat(DataHolder dataHolder)
    {
        foreach (VerticeData vertice in dataHolder.verticeData.Values)
            if (vertice.verticeType == VerticeType.Person)
            {
                allPersons[vertice.id] = vertice;
                shownPersons.Add(vertice);
            }

        int y_index = 0;
        int offset = 30;
        if (allPersons.Count > 8) offset = 25;
        foreach (KeyValuePair<long, VerticeData> person in allPersons)
        {
            if (person.Key < 0) continue;
            Vector3 pos = defaultPersonToggleElement.transform.position;
            pos.y = pos.y - offset * y_index;
            GameObject newPersonToggleElement = Instantiate(defaultPersonToggleElement, pos, Quaternion.identity, personToggleElementHolder.transform);
            newPersonToggleElement.GetComponentInChildren<Text>().text = person.Value.name;
            VerticeData person2 = person.Value;
            newPersonToggleElement.GetComponentInChildren<Toggle>().onValueChanged.AddListener((bool value) => selectionChanged(value, person2, y_index));
            newPersonToggleElement.gameObject.SetActive(true);
            y_index++;
        }

        personIdToMetricIdToValue = computeMetrics(dataHolder);
        generateKiwiat(dataHolder);
    }

    public void selectionChanged(bool value, VerticeData person, int toggleIndex)
    {
        if (value)
        {
            Debug.Log("adding " + person.name + " with id " + person.id.ToString() + " to my kiwiat!");
            shownPersons.Add(person);
        }
        else
        {
            Debug.Log("removing " + person.name + " with id " + person.id.ToString() + " from my kiwiat!");
            shownPersons.Remove(person);
        }
    }

    // Update is called once per frame
    public void generateKiwiat(DataHolder dataHolder)
    {
        // removeCurrentKiwiat();

    }

    public void removeCurrentKiwiat()
    {

        foreach (Transform child in kiwiatDiagramArea.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public Dictionary<long, Dictionary<String, float>> computeMetrics(DataHolder dataHolder)
    {
        Dictionary<long, Dictionary<String, float>> metrics = new Dictionary<long, Dictionary<String, float>>();
        foreach (KeyValuePair<long, VerticeData> person in allPersons)
        {
            Dictionary<String, float> metricz = new Dictionary<String, float>();
            metricz["0"] = UnityEngine.Random.Range(0.0f, 1.0f);
            metricz["1"] = UnityEngine.Random.Range(0.0f, 1.0f);
            metricz["2"] = UnityEngine.Random.Range(0.0f, 1.0f);
            metricz["3"] = UnityEngine.Random.Range(0.0f, 1.0f);
            metricz["4"] = UnityEngine.Random.Range(0.0f, 1.0f);
            metricz["5"] = UnityEngine.Random.Range(0.0f, 1.0f);
            metrics[person.Value.id] = metricz;
        }
        return metrics;
    }

}
