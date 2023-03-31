using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;

public class FilterController : MonoBehaviour
{
    private Dictionary<EdgeType, Toggle> edgeFilter = new ();
    private Dictionary<VerticeType, Toggle> verticeFilter = new();

    [Header("References")] 
    public GameObject verticeSpawn;
    public GameObject edgeSpawn;
    public GameObject checkboxPrefab;
    public GameObject dialogObj;

    // Start is called before the first frame update
    void Start()
    {
        foreach (EdgeType type in (EdgeType[]) Enum.GetValues(typeof(EdgeType)))
        {
            GameObject edgeCheckbox = Instantiate(checkboxPrefab, Vector3.zero, Quaternion.identity, edgeSpawn.transform);
            edgeCheckbox.GetComponentInChildren<TMP_Text>().text = type.ToString();
            Toggle t = edgeCheckbox.GetComponent<Toggle>();
            t.isOn = true;
            edgeFilter.Add(type,t);
        }
        
        foreach (VerticeType type in (VerticeType[]) Enum.GetValues(typeof(VerticeType)))
        {
            GameObject verticeCheckbox = Instantiate(checkboxPrefab, Vector3.zero, Quaternion.identity, verticeSpawn.transform);
            verticeCheckbox.GetComponentInChildren<TMP_Text>().text = type.ToString();
            Toggle t = verticeCheckbox.GetComponent<Toggle>();
            t.isOn = true;
            verticeFilter.Add(type,t);
        }
    }

    public void SaveFilter()
    {
        SingletonManager.Instance.dataManager.SetFilter(new FilterHolder(
            edgeFilter.Where(i=>i.Value.isOn).Select(pair=>pair.Key).ToHashSet(),
            verticeFilter.Where(i=>i.Value.isOn).Select(pair=>pair.Key).ToHashSet(),
            null,null));
        CloseDialog();
    }

    public void CloseDialog()
    {
        dialogObj.SetActive(false);
        SingletonManager.Instance.pauseManager.SetPaused(false);
    }
    
    public void OpenDialog()
    {
        dialogObj.SetActive(true);
        SingletonManager.Instance.pauseManager.SetPaused(true);
    }
    
    public void SetSpeed(float speed)
    {
        SingletonManager.Instance.animationManager.SetSpeed(speed);
    }
}
