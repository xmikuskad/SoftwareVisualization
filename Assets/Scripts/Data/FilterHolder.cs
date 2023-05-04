using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;

public class FilterHolder
{
    public HashSet<EdgeType> disabledEdges;
    public HashSet<VerticeType> disabledVertices;

    public FilterHolder(HashSet<EdgeType> disabledEdges, HashSet<VerticeType> disabledVertices)
    {
        this.disabledEdges = disabledEdges;
        this.disabledVertices = disabledVertices;
    }

    public FilterHolder()
    {
        this.disabledEdges = new ();
        this.disabledVertices = new ();

    }
}