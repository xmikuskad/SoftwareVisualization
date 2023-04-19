using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

[Serializable]
public class DataHolder
{
    public long projectId;
    public Dictionary<long,EdgeData> edgeData;
    public Dictionary<long,VerticeData> verticeData;
    public Dictionary<String, long> personIds = new();
    public List<EventData> eventData = new ();
    public Dictionary<long, long> edgeCountForTickets = new();
    public DateTime startDate;

    public DataHolder()
    {
    }

    public DataHolder(DataHolder original, FilterHolder holder)
    {
        projectId = original.projectId;
        edgeData =original.edgeData
            .Select(e=>e.Value)
            .Where(e => holder.allowedEdges.Contains(e.type))
            .ToDictionary(i=>i.id);
        verticeData =original.verticeData
            .Select(e=>e.Value)
            .Where(e => holder.allowedVertices.Contains(e.verticeType))
            .ToDictionary(i=>i.id);
    }

    public void CreateEventData()
    {
        this.eventData.Clear();
        // Create a map so we can get personId from personString
        foreach (var data in this.verticeData.Values.Where(e=>e.verticeType == VerticeType.Person))
        {
            personIds[data.name]=data.id;
        }
        
        // Process all tickets and add their creation to events
        foreach (var data in this.verticeData.Values.Where(e=>e.verticeType == VerticeType.Ticket))
        {
            eventData.Add(new EventData(projectId,GetPersonIds(data.author),data.created.Value,data.id,EventActionType.CREATE,0));

            // TODO what to do if start is sooner than creation ???
            DateTime start = data.created.Value >= data.start.Value
                ? data.created.Value.AddMilliseconds(1)
                : data.start.Value;
            eventData.Add(new EventData(projectId,GetPersonIds(data.assignee),start,data.id,EventActionType.UPDATE,0));
        }
        
        // Get all ticket ids for faster comparision
        HashSet<long> ticketIds = this.verticeData.Values.Where(e=>e.verticeType == VerticeType.Ticket).Select(i=>i.id).ToHashSet();
        // Now find all changes which are related to tickets.
        foreach (var data in this.edgeData.Values)
        {
            if (ticketIds.Contains(data.from))
            {
                VerticeData vertice = verticeData[data.to];

                switch (vertice.verticeType)
                {
                    case VerticeType.Change:
                        if (vertice.author == null)
                        {
                            Debug.Log("Null author, skipping");
                            continue;
                        }
                        
                        VerticeData ticket = verticeData[data.from];
                        if (!edgeCountForTickets.ContainsKey(data.from))
                        {
                            edgeCountForTickets[data.from] = 0;
                        }
                        
                        if (vertice.committed != null)
                        {
                            // TODO this happens !?!?!
                            DateTime commitedTime = ticket.created.Value >= vertice.committed.Value
                                ? ticket.created.Value.AddMilliseconds(5)
                                : vertice.committed.Value;
                            eventData.Add(new EventData(projectId,GetPersonIds(vertice.author),commitedTime,data.from,EventActionType.MOVE,data.id));
                            edgeCountForTickets[data.from]++;
                        }
                        // TODO this happens !?!?!
                        DateTime createdTime = ticket.created.Value >= vertice.created.Value
                            ? ticket.created.Value.AddMilliseconds(3)
                            : vertice.created.Value;
                        eventData.Add(new EventData(projectId,GetPersonIds(vertice.author),createdTime,data.from,EventActionType.MOVE,data.id));
                        edgeCountForTickets[data.from]++;
                        break;
                    default:
                        Debug.Log("Ignoring "+vertice.verticeType);
                        break;
                }
            }
        }

        eventData = eventData.OrderBy(x => x.when).ToList();
        startDate = eventData[0].when;

        // eventData.GroupBy(e => e.when.Date).Select(group => new EventDataDate(group.ToList(), group.Key))
        //     .OrderBy(e => e.eventDate);
        
        Debug.Log("Event data created");
    }

    // Get person ids based on names
    private List<long> GetPersonIds(string[] person)
    {
        if (person == null)
            return new List<long>();
        return person.Select(x => personIds[x]).Distinct().ToList();
    }
}