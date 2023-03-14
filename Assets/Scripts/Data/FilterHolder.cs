using System;
using System.Collections.Generic;

public class FilterHolder
{
    public HashSet<EdgeType> allowedEdges;
    public HashSet<VerticeType> allowedVertices;
    public DateTime? fromDate;
    public DateTime? toDate;

    public FilterHolder(HashSet<EdgeType> allowedEdges, HashSet<VerticeType> allowedVertices, DateTime? fromDate, DateTime? toDate)
    {
        this.allowedEdges = allowedEdges;
        this.allowedVertices = allowedVertices;
        this.fromDate = fromDate;
        this.toDate = toDate;
    }
}