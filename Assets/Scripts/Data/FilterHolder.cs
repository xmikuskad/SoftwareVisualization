using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;

public class FilterHolder
{
    public HashSet<EdgeType> allowedEdges;
    public HashSet<VerticeType> allowedVertices;

    public FilterHolder(HashSet<EdgeType> allowedEdges, HashSet<VerticeType> allowedVertices)
    {
        this.allowedEdges = allowedEdges;
        this.allowedVertices = allowedVertices;
    }

    public FilterHolder()
    {
        this.allowedEdges = new HashSet<EdgeType>(Enum.GetValues(typeof(EdgeType)).Cast<EdgeType>());
        this.allowedVertices = new HashSet<VerticeType>(Enum.GetValues(typeof(VerticeType)).Cast<VerticeType>());

    }
}