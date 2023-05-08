using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        // foreach (EdgeType type in (EdgeType[]) Enum.GetValues(typeof(EdgeType)))
        // {
        //     GameObject edgeCheckbox = Instantiate(checkboxPrefab, Vector3.zero, Quaternion.identity, edgeSpawn.transform);
        //     edgeCheckbox.GetComponentInChildren<TMP_Text>().text = type.ToString();
        //     Toggle t = edgeCheckbox.GetComponent<Toggle>();
        //     t.isOn = true;
        //     edgeFilter.Add(type,t);
        // }
        
        foreach (VerticeType type in (VerticeType[]) Enum.GetValues(typeof(VerticeType)))
        {
            if(type == VerticeType.Change) continue;
            if(type == VerticeType.Commit) continue;
            GameObject verticeCheckbox = Instantiate(checkboxPrefab, Vector3.zero, Quaternion.identity, verticeSpawn.transform);
            verticeCheckbox.GetComponentInChildren<TMP_Text>().text = type.ToString();
            Toggle t = verticeCheckbox.GetComponent<Toggle>();
            t.isOn = true;
            verticeFilter.Add(type,t);
        }
    }

    public void SaveFilter()
    {
        SingletonManager.Instance.dataManager.InvokeDataFilterEvent(new FilterHolder(
            edgeFilter.Where(i=>!i.Value.isOn).Select(pair=>pair.Key).ToHashSet(),
            verticeFilter.Where(i=>!i.Value.isOn).Select(pair=>pair.Key).ToHashSet()));
        CloseDialog();
    }

    public void CloseDialog()
    {
        dialogObj.SetActive(false);
        SingletonManager.Instance.pauseManager.SetEverythingPaused(false);
    }
    
    public void OpenDialog()
    {
        // This has to be 2 times
        dialogObj.SetActive(true);
        dialogObj.SetActive(true);
        SingletonManager.Instance.pauseManager.SetEverythingPaused(true);
    }
    
    public void SetSpeed(float speed)
    {
        SingletonManager.Instance.animationManager.SetSpeed(speed);
    }
}
