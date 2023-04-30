using System;
using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace Data
{
    public class VerticeWrapper
    {
        public VerticeData verticeData;
        private Dictionary<VerticeType, List<VerticeData>> relatedVertices = new();
        private Dictionary<EdgeType, List<EdgeData>> relatedEdges= new();
        
        private Dictionary<long, VerticeData> relatedVerticesById = new();
        private Dictionary<long, EdgeData> relatedEdgesById= new();

        public void AddData(VerticeData vertice, EdgeData edge)
        {
            // Set up vertice dictionary by type
            if (!relatedVertices.ContainsKey(vertice.verticeType))
            {
                relatedVertices[vertice.verticeType] = new ();
            }
            if (!relatedVertices[vertice.verticeType].Contains(vertice))
            {
                relatedVertices[vertice.verticeType].Add(vertice);   
            }

            // Set up vertice dictionary by ID
            if (!relatedVerticesById.ContainsKey(vertice.id))
            {
                relatedVerticesById[vertice.id] = vertice;
            }
            
            // Set up edge dictionary by type
            if (!relatedEdges.ContainsKey(edge.type))
            {
                relatedEdges[edge.type] = new ();
            }
            relatedEdges[edge.type].Add(edge);
            if (!relatedEdges[edge.type].Contains(edge))
            {
                relatedEdges[edge.type].Add(edge);  
            }
            
            // Set up edge dictionary by ID
            if (!relatedEdgesById.ContainsKey(edge.id))
            {
                relatedEdgesById[edge.id] = edge;
            }
        }

        // Change, RepoFile, Wiki can have null author!
        public void GetAuthor()
        {
            if (!relatedEdges.ContainsKey(EdgeType.Authorship))
            {
                Debug.Log(this.verticeData.verticeType+" - Authorship not found");
                return;
            }
            if (relatedEdges[EdgeType.Authorship].Count > 1)
            {
                Debug.Log(this.verticeData.verticeType+" - Multiple authorship found: "+relatedEdges[EdgeType.Authorship].Count);
                return;
            }
            Debug.Log("Author OK");
        }
    }
}