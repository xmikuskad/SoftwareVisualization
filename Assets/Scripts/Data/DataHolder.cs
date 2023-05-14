using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Helpers;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class DataHolder
{
    public string projectName;
    public long projectId;
    public Dictionary<long, EdgeData> edgeData;
    public Dictionary<long, VerticeData> verticeData;
    
    public Dictionary<String, long> personIds = new();
    public DateTime startDate;

    // For ticket id returns dict of ids of authors and list of related changes by that author
    public Dictionary<long, Dictionary<long, List<VerticeData>>> ticketToChangeListPerAuthor = new();

    // New data structure
    public Dictionary<long, VerticeWrapper> verticeWrappers = new();
    
    // WARNING: This uses key DateTime.MinValue as a null. REMOVE WHEN USING ONLY THE KEYS
    public Dictionary<DateTime, List<VerticeWrapper>> verticesByDate = new();
    
    // WARNING: This uses value DateTime.MinValue
    private Dictionary<long, List<DateTime>> datesForVertice = new();

    // This is only used for moving in time
    // WARNING: This uses key DateTime.MinValue as a null. REMOVE WHEN USING ONLY THE KEYS
    public Dictionary<DateTime, List<VerticeWrapper>> changesByDate = new();
    // WARNING: This uses key DateTime.MinValue
    public List<DateTime> orderedDates = new();

    public List<VerticeWrapper> spawnAtStart = new();

    public long maxVerticeCount = 0;
    public long maxEdgeCount = 0;

    public DateTime minDate = DateTime.MaxValue.Date;
    public DateTime maxDate = DateTime.MinValue.Date;
    public List<DateTime> dates = new();

    // TODO This function needs a rework. I know that data are only loading for tickets.
    public void LoadData()
    {
        // Create a map so we can get personId from personString
        foreach (var data in this.verticeData.Values.Where(e => e.verticeType == VerticeType.Person))
        {
            personIds[data.name] = data.id;
        }

        personIds["_unknown"] = -1;

        LoadVerticeWrappers();

        fillTicketToChangeListPerAuthor();

        this.projectName = projectName + "-" + projectId;
        Debug.Log("Data holder load finished created");
    }

    private void fillTicketToChangeListPerAuthor()
    {
        
        foreach (VerticeData vertice in verticeData.Values)
        {
            if (vertice.verticeType == VerticeType.Ticket) ticketToChangeListPerAuthor[vertice.id] = new();
        }

        foreach (EdgeData edge in edgeData.Values)
        {
            if (verticeData[edge.from].verticeType != VerticeType.Ticket) continue;
            if (verticeData[edge.to].verticeType != VerticeType.Change) continue;
            VerticeData ticket = verticeData[edge.from];
            VerticeWrapper change = verticeWrappers[edge.to];
            long changeAuthorId = change.GetAuthorId();
            if (!ticketToChangeListPerAuthor[ticket.id].ContainsKey(changeAuthorId))
            {
                ticketToChangeListPerAuthor[ticket.id][changeAuthorId] = new();
            }
            if(!ticketToChangeListPerAuthor[ticket.id][changeAuthorId].Contains(change.verticeData))
                ticketToChangeListPerAuthor[ticket.id][changeAuthorId].Add(change.verticeData);
        }
    }

    // Get person ids based on names
    private List<long> GetPersonIds(string[] person)
    {
        if (person == null)
            return new List<long>();
        return person.Select(x => personIds[x]).Distinct().ToList();
    }

    public int GetTicketCount()
    {
        return verticeData.Count(e => e.Value.verticeType == VerticeType.Ticket);
    }

    private void LoadVerticeWrappers()
    {

        // Load vertices
        foreach (var (key, value) in verticeData)
        {
            verticeWrappers[key] = new VerticeWrapper(projectId);
            verticeWrappers[key].verticeData = value;
        }
        
        // Load edges
        foreach (var (key, value) in edgeData)
        {
            verticeWrappers[value.from].AddData(verticeData[value.to], value);
        }
        
        // Group vertices by type and find the most occurring type
        var mostOccurringType = verticeWrappers.Values.Where(x=>x.verticeData.verticeType!= VerticeType.Change && x.verticeData.verticeType != VerticeType.Commit)
            .GroupBy(v => v.verticeData.verticeType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // Count the most occurring type
        maxVerticeCount = verticeWrappers.Values.Count(v => v.verticeData.verticeType == mostOccurringType);

        foreach (VerticeType t in (VerticeType[]) Enum.GetValues(typeof(VerticeType)))
        {
            foreach (List<Pair<VerticeData,VerticeWrapper>> pairs in GetVerticesForPlatform(t))
            {
                if (pairs.Count > maxEdgeCount)
                    maxEdgeCount = pairs.Count;
            }
        }
        
        
        foreach (var (key, value) in verticeWrappers)
        {
            if (value.verticeData.verticeType == VerticeType.Change ||
                value.verticeData.verticeType == VerticeType.Commit)
            {

                // if (value.GetRelatedVertices().Count > 3)
                // {
                //     Debug.Log("id: "+value.verticeData.id+" = "+value.GetRelatedVertices().Count);
                // }
                
                DateTime dateTime = value.GetTimeWithoutHours();
                if (dateTime != DateTime.MinValue.Date)
                {
                    if (dateTime > maxDate)
                    {
                        maxDate = dateTime;
                    }

                    if (dateTime < minDate)
                    {
                        minDate = dateTime;
                    }
                    dates.Add(dateTime);
                }

                foreach (var related in value.GetRelatedVertices())
                {
                    if (!verticesByDate.ContainsKey(value.GetTimeWithoutHours()))
                    {
                        verticesByDate[value.GetTimeWithoutHours()] = new();
                    }

                    if (!verticesByDate[value.GetTimeWithoutHours()].Contains(verticeWrappers[related.id]))
                        verticesByDate[value.GetTimeWithoutHours()].Add(verticeWrappers[related.id]);
                    
                    verticeWrappers[related.id].AddChangeOrCommit(value);
                }
            }
        }

        dates = dates.Distinct().OrderBy(x => x).ToList();

        startDate = minDate;
    }

    public void UpdateUsageForFilter(DateTime from, DateTime to)
    {
        // TODO
    }


    public List<List<Pair<VerticeData,VerticeWrapper>>> GetVerticesForPlatform(VerticeType type)
    {
        List<List<Pair<VerticeData,VerticeWrapper>>> list = new();
        
        foreach (var val in verticeWrappers.Values.Where(x=>x.verticeData.verticeType == type).OrderBy(x=>x.GetFirstDate()))
        {
            if (type == VerticeType.RepoFile)
            {
                // comparision by commits
                list.Add(val.GetOrderedRelatedVerticesByType(VerticeType.Commit).Select(x=>new Pair<VerticeData,VerticeWrapper>(x,val)).ToList());
            }
            else if (type == VerticeType.Person)
            {
                list.Add(new() { new Pair<VerticeData, VerticeWrapper>(null,val) });
            }
            else
            {
                // compare by changes
                list.Add(val.GetOrderedRelatedVerticesByType(VerticeType.Change).Select(x=>new Pair<VerticeData,VerticeWrapper>(x,val)).ToList());
            }
        }
        
        return list;
    }
}