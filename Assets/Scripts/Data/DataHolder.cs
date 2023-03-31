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
    public Dictionary<String, long> personData = new();

    public List<EventData> eventData = new ();
    public Dictionary<long, long> edgeCountForTickets = new();

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
        foreach (var data in this.verticeData.Values.Where(e=>e.verticeType == VerticeType.Person))
        {
            personData[data.name]=data.id;
        }
        foreach (var data in this.verticeData.Values.Where(e=>e.verticeType == VerticeType.Ticket))
        {
            eventData.Add(new EventData(projectId,personData[data.author[0]],data.created.Value,data.id,EventActionType.CREATE,0));

            // TODO what to do if start is sooner than creation ???
            DateTime start = data.created.Value >= data.start.Value
                ? data.created.Value.AddMilliseconds(1)
                : data.start.Value;
            eventData.Add(new EventData(projectId,personData[data.assignee[0]],start,data.id,EventActionType.UPDATE,0));
        }
        
        HashSet<long> ticketIds = this.verticeData.Values.Where(e=>e.verticeType == VerticeType.Ticket).Select(i=>i.id).ToHashSet();
        foreach (var data in this.edgeData.Values)
        {
            if (ticketIds.Contains(data.from))
            {
                VerticeData vertice = verticeData[data.to];
                if (vertice.author == null)
                {
                    Debug.Log("Null author, skipping");
                    continue;
                }

                switch (vertice.verticeType)
                {
                    case VerticeType.Change:
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
                            eventData.Add(new EventData(projectId,personData[vertice.author[0]],commitedTime,data.from,EventActionType.MOVE,data.id));
                            edgeCountForTickets[data.from]++;
                        }
                        // TODO this happens !?!?!
                        DateTime createdTime = ticket.created.Value >= vertice.created.Value
                            ? ticket.created.Value.AddMilliseconds(3)
                            : vertice.created.Value;
                        eventData.Add(new EventData(projectId,personData[vertice.author[0]],createdTime,data.from,EventActionType.MOVE,data.id));
                        edgeCountForTickets[data.from]++;
                        break;
                    default:
                        Debug.Log("Ignoring "+vertice.verticeType);
                        break;
                }
            }
        }

        eventData = eventData.OrderBy(x => x.when).ToList();
        Debug.Log("DONE?");
    }
}