using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager: MonoBehaviour
{
    private long projectIdCounter = 0;
    private Dictionary<long, String> projectNames = new();
    private Dictionary<long, DataHolder> dataHolders = new();
    
    public void LoadData(DataHolder holder)
    {
        projectIdCounter++;
        // Template name
        projectNames.Add(projectIdCounter,"SomeName"+projectIdCounter);
        holder.projectId = projectIdCounter;
        dataHolders.Add(projectIdCounter,holder);
        
        Debug.Log(string.Join("\n", holder.edgeData.Values.Select(x => x.ToString()).ToArray()));
        Debug.Log(string.Join("\n", holder.verticeData.Values.Select(x => x.ToString()).ToArray()));
    }

}
