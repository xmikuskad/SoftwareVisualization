using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class DataHolder
{
    public long projectId;
    public Dictionary<long,EdgeData> edgeData;
    public Dictionary<long,VerticeData> verticeData;

    public DataHolder()
    {
    }

    public DataHolder(DataHolder original, HashSet<EdgeType> edgeFilter, HashSet<VerticeType> verticeFilter)
    {
        projectId = original.projectId;
        edgeData =original.edgeData
            .Select(e=>e.Value)
            .Where(e => !edgeFilter.Contains(e.type))
            .ToDictionary(i=>i.id);
        verticeData =original.verticeData
            .Select(e=>e.Value)
            .Where(e => !verticeFilter.Contains(e.verticeType))
            .ToDictionary(i=>i.id);
    }
}