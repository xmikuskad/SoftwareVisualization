﻿using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using UnityEngine;

namespace Data
{
    public class VerticeWrapper
    {
        public VerticeData verticeData;
        private Dictionary<VerticeType, List<VerticeData>> relatedVertices = new();
        private Dictionary<EdgeType, List<EdgeData>> relatedEdges = new();

        private Dictionary<long, VerticeData> relatedVerticesById = new();
        private Dictionary<long, EdgeData> relatedEdgesById = new();

        public Dictionary<long, VerticeWrapper> relatedChangesOrCommits = new();

        private List<DateTime> dates = new();
        public long updateCount = 1;

        public void AddData(VerticeData vertice, EdgeData edge)
        {
            // Set up vertice dictionary by type
            if (!relatedVertices.ContainsKey(vertice.verticeType))
            {
                relatedVertices[vertice.verticeType] = new();
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
            if (edge != null)
            {
                // Set up edge dictionary by type
                if (!relatedEdges.ContainsKey(edge.type))
                {
                    relatedEdges[edge.type] = new();
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

            this.dates = new();
            if (this.relatedVertices.ContainsKey(VerticeType.Change))
            {
                this.dates = this.relatedVertices[VerticeType.Change]
                    .Select(x => x.created ?? x.begin ?? DateTime.MinValue)
                    .Select(x => x.Date).Distinct().Where(x => x > DateTime.MinValue.Date).OrderBy(x=>x).ToList();
            }
            else if (this.relatedVertices.ContainsKey(VerticeType.Commit))
            {
                this.dates = this.relatedVertices[VerticeType.Commit]
                    .Select(x => x.created ?? x.begin ?? DateTime.MinValue)
                    .Select(x => x.Date).Distinct().Where(x => x > DateTime.MinValue.Date).OrderBy(x=>x).ToList();
            }
            else
            {
                this.dates.Add(DateTime.MinValue.Date);
            }

        }

        public void AddChangeOrCommit(VerticeWrapper v)
        {
            this.relatedChangesOrCommits[v.verticeData.id] = v;
        }
        
        // Change, RepoFile, Wiki can have null author!
        public long GetAuthorId()
        {
            if (!relatedEdges.ContainsKey(EdgeType.Authorship))
            {
                //Debug.Log(this.verticeData.verticeType+" - Authorship not found");
                return -1L;
            }
            if (relatedEdges[EdgeType.Authorship].Count > 1)
            {
                //Debug.Log(this.verticeData.verticeType+" - Multiple authorship found: "+relatedEdges[EdgeType.Authorship].Count);
                return -1L;
            }
            //Debug.Log("Author OK");

            return relatedEdges[EdgeType.Authorship][0].to;
        }

        public DateTime TmpGetDate()
        {
            return verticeData.created ?? verticeData.begin ?? DateTime.MinValue;
        }
        
        public DateTime TmpGetDateNoHours()
        {
            return TmpGetDate().Date;
        }
        // Returns DateTime.MinValue on fail.
        public DateTime GetTime()
        {

            if (this.verticeData.verticeType == VerticeType.Change)
            {
                return verticeData.created ?? verticeData.begin ?? DateTime.MinValue;
            }

            if (this.verticeData.verticeType == VerticeType.Commit)
            {
                return verticeData.created ?? verticeData.begin ?? DateTime.MinValue;
            }
           
            return DateTime.MinValue;
        }

        public DateTime GetTimeWithoutHours()
        {
            return GetTime().Date;
        }

        public bool IsConnectedWithVertices(HashSet<long> verticeId)
        {
            if (verticeId.Count == 0) return false;
            return relatedEdgesById.Values.Where(x => verticeId.Contains(x.to)).ToList().Count() > 0 || verticeId.Contains(this.verticeData.id);
        }

        public List<VerticeData> GetRelatedVertices()
        {
            return relatedVerticesById.Values.ToList();
        }
        
        public Dictionary<VerticeType,List<VerticeData>> GetRelatedVerticesDict()
        {
            return relatedVertices;
        }
        public List<VerticeData> GetOrderedRelatedVerticesByType(VerticeType type)
        {
            if (relatedVertices.ContainsKey(type))
            {
                return relatedVertices[type].OrderBy(x=>x.created ?? x.begin ?? DateTime.MinValue).ToList();
            }

            return new();
        }

        public bool ContainsDate(DateTime date)
        {
            return this.dates.Contains(date);
        }

        public bool ContainsDate(List<DateTime> dates)
        {
            foreach (var dateTime in dates)
            {
                if (this.dates.Contains(dateTime))
                    return true;
            }

            return false;
        }

        public bool IsDateBetween(DateTime from, DateTime to)
        {
            foreach (var dateTime in dates)
            {
                if (this.dates.Any(x => x >= from && x <= to))
                    return true;
            }

            return false;
        }

        public DateTime GetFirstDate()
        {
            if (this.dates.Count > 0)
            {
                return this.dates[0];
            }
            return DateTime.MinValue;
        }

        public bool IsConnected(Pair<VerticeData, VerticeWrapper> pair, VerticeData commitOrChange)
        {
            // This is only person
            if ((commitOrChange?.id ?? -1) < 0)
            {
                if (pair.Left?.id == null)
                {
                    if (pair.Right.verticeData.verticeType == VerticeType.Person)
                    {
                        // Do not show other person when clicking on person
                        return pair.Right.verticeData.id == this.verticeData.id;
                    }
                    else
                    {
                        // If the ticket contains reference to this then show it
                        return pair.Right.relatedVertices.ContainsKey(VerticeType.Person) && pair.Right
                            .relatedVertices[VerticeType.Person].Any(x => x.id == this.verticeData.id);
                    }
                }
                else
                {
                    // Check if person is mentioned in a commit
                    return this.relatedChangesOrCommits.ContainsKey(pair.Left.id) &&
                           this.relatedChangesOrCommits[pair.Left.id].relatedVerticesById
                               .ContainsKey(this.verticeData.id);
                }
                
            }

            if (pair.Left?.id != null)
            {
                return commitOrChange.id == pair.Left.id;
            }
            else
            {
                return pair.Right.relatedVerticesById.ContainsKey(this.verticeData.id);
            }



            // Ehm this will never happen
            if (this.verticeData.verticeType == VerticeType.Change ||
                this.verticeData.verticeType == VerticeType.Commit)
            {
                Debug.Log("Happened?");
                return IsDirectlyConnected(pair.Left ?? pair.Right.verticeData);
            }

            if (pair.Left != null)
            {
                return pair.Left.id == commitOrChange?.id || (this.verticeData.verticeType == VerticeType.Person && (this.relatedChangesOrCommits.ContainsKey(pair.Left.id)));
            }
            else
            {
                return IsConnectedByChangeOrCommit(pair.Right.verticeData, (commitOrChange?.id??-1));
            }
        }

        public bool IsConnectedByChangeOrCommit(VerticeData v, long commitOrChangeId)
        {
            if (this.verticeData.id == v.id) return true;
            if (this.verticeData.verticeType == VerticeType.Person) return false;
            
            if(commitOrChangeId <0)
                return this.relatedChangesOrCommits.Values.Any(x => x.IsDirectlyConnected(v));
            return (this.relatedChangesOrCommits.ContainsKey(commitOrChangeId) && this.relatedChangesOrCommits[commitOrChangeId].IsDirectlyConnected(v));
        }

        // public bool ContainsChangeOrCommit(long verticeId, long commitOrChangeIdFrom,long commitOrChangeIdTo)
        // {
        //     if(commitOrChangeIdTo <0)
        //         return this.verticeData.id == verticeId || this.relatedChangesOrCommits.ContainsKey(commitOrChangeIdFrom);
        //     if (this.verticeData.id == verticeId && commitOrChangeIdFrom == commitOrChangeIdTo)
        //         return true;
        //     return this.relatedChangesOrCommits.ContainsKey(commitOrChangeIdFrom);
        // }

        public bool IsDirectlyConnected(VerticeData v)
        {
            if (this.verticeData.id == v.id) return true;
            if (this.verticeData.verticeType == VerticeType.Person)
            {
                return false;
                // return this.relatedVerticesById.Values.Where(x => x.verticeType != VerticeType.Person).Select(x => x.id)
                //     .Any(x => x == v.id);
            }
            return this.verticeData.id == v.id || this.relatedVerticesById.Values.Select(x=>x.id).Any(x => x==v.id);
        }
    }
}