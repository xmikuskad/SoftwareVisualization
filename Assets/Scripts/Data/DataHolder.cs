using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Data;
using UnityEngine;

[Serializable]
public class DataHolder
{
    public long projectId;
    public Dictionary<long, EdgeData> edgeData;
    public Dictionary<long, VerticeData> verticeData;
    public Dictionary<String, long> personIds = new();
    public List<EventData> eventData = new();
    public Dictionary<long, long> edgeCountForTickets = new();
    public DateTime startDate;
    public Dictionary<DateTime, long> eventCountByDate = new();

    // Contains all data for specific dateTime
    public Dictionary<DateTime, List<EventData>> eventsByDate = new();

    // Contains all dates for a verticeId
    public Dictionary<long, List<DateTime>> datesForVertice = new();

    // Contains all dates for a verticeId - without time, only date
    public Dictionary<long, List<DateTime>> rawDatesForVertice = new();

    // For ticket id returns dict of ids of authors and list of related changes by that author
    public Dictionary<long, Dictionary<long, List<VerticeData>>> ticketToChangeListPerAuthor = new();
    
    // TESTING
    public Dictionary<long, VerticeWrapper> verticeWrappers = new();

    public DataHolder()
    {
    }

    public DataHolder(DataHolder original, FilterHolder holder)
    {
        projectId = original.projectId;
        edgeData = original.edgeData
            .Select(e => e.Value)
            .Where(e => holder.allowedEdges.Contains(e.type))
            .ToDictionary(i => i.id);
        verticeData = original.verticeData
            .Select(e => e.Value)
            .Where(e => holder.allowedVertices.Contains(e.verticeType))
            .ToDictionary(i => i.id);
    }


    // TODO This function needs a rework. I know that data are only loading for tickets.
    public void LoadData()
    {
        this.eventData.Clear();
        // Create a map so we can get personId from personString
        foreach (var data in this.verticeData.Values.Where(e => e.verticeType == VerticeType.Person))
        {
            personIds[data.name] = data.id;
        }
        personIds["_unknown"] = -1;

        // Process all tickets and add their creation to events
        foreach (var data in this.verticeData.Values.Where(e => e.verticeType == VerticeType.Ticket))
        {
            DateTime createdTime = data.created.HasValue ? data.created.Value : data.begin.Value;
            EventData created = new EventData(projectId, GetPersonIds(data.author), createdTime, data.id,
                EventActionType.CREATE, 0);
            eventData.Add(created);
            AddEventToDate(created);

            // TODO what to do if start is sooner than creation ???
            DateTime start = createdTime >= data.start.Value
                ? createdTime.AddMilliseconds(1)
                : data.start.Value;
            EventData started = new EventData(projectId, GetPersonIds(data.assignee), start, data.id,
                EventActionType.UPDATE, 0);
            eventData.Add(started);
            AddEventToDate(started);
        }

        // Get all ticket ids for faster comparision
        HashSet<long> ticketIds = this.verticeData.Values.Where(e => e.verticeType == VerticeType.Ticket).Select(i => i.id).ToHashSet();
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
                        
                        DateTime createdTimeForTicket = ticket.created.HasValue ? ticket.created.Value : ticket.begin.Value;
                        DateTime createdTimeForVertice = vertice.created.HasValue ? vertice.created.Value : vertice.begin.Value;

                        if (vertice.committed != null)
                        {
                            // TODO this happens !?!?!
                            DateTime commitedTime = createdTimeForTicket >= vertice.committed.Value
                                ? createdTimeForTicket.AddMilliseconds(5)
                                : vertice.committed.Value;
                            EventData commited = new EventData(projectId, GetPersonIds(vertice.author), commitedTime,
                                data.from, EventActionType.MOVE, data.id);
                            eventData.Add(commited);
                            AddEventToDate(commited);
                            edgeCountForTickets[data.from]++;
                        }
                        // TODO this happens !?!?!
                        DateTime createdTime = createdTimeForTicket >= createdTimeForVertice
                            ? createdTimeForTicket.AddMilliseconds(3)
                            : createdTimeForVertice;
                        EventData created = new EventData(projectId, GetPersonIds(vertice.author), createdTime,
                            data.from, EventActionType.MOVE, data.id);
                        eventData.Add(created);
                        AddEventToDate(created);
                        edgeCountForTickets[data.from]++;
                        break;
                    default:
                        Debug.Log("Ignoring " + vertice.verticeType);
                        break;
                }
            }
        }

        eventData = eventData.OrderBy(x => x.when).ToList();
        startDate = eventData[0].when;

        datesForVertice = eventsByDate
            .SelectMany(kvp => kvp.Value.Select(eventData => new { Id = eventData.verticeId, Date = kvp.Key }))
            .GroupBy(obj => obj.Id)
            .ToDictionary(g => g.Key, g => g.Select(obj => obj.Date).ToList());
        rawDatesForVertice = eventsByDate
            .SelectMany(kvp => kvp.Value.Select(eventData => new { Id = eventData.verticeId, Date = kvp.Key }))
            .GroupBy(obj => obj.Id)
            .ToDictionary(g => g.Key, g => g.Select(obj => obj.Date.Date).ToList());


        // SORT EVENTS BY DATE
        var keys = eventsByDate.Keys.ToList(); // create a list of keys to iterate over

        for (int i = 0; i < keys.Count; i++)
        {
            DateTime key = keys[i];

            // sort the list of EventData objects by the date property
            List<EventData> sortedList = eventsByDate[key].OrderBy(e => e.when).ToList();

            // replace the unsorted list with the sorted list in the dictionary
            eventsByDate[key] = sortedList;
        }

        fillTicketToChangeListPerAuthor();

        LoadVerticeWrappers();
        
        Debug.Log("Event data created");
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
            VerticeData change = verticeData[edge.to];
            string changeAuthorInitials = "_unknown";
            if (!(change.author == null || change.author[0] == null)) changeAuthorInitials = change.author[0];
            long changeAuthorId = personIds[changeAuthorInitials];
            // Debug.Log("found a ticket id " + ticket.id.ToString() + ", change id " + change.id.ToString() + ", author " + changeAuthorInitials + " with id " + changeAuthorId.ToString());
            if (!ticketToChangeListPerAuthor[ticket.id].ContainsKey(changeAuthorId))
            {
                ticketToChangeListPerAuthor[ticket.id][changeAuthorId] = new();
            }
            ticketToChangeListPerAuthor[ticket.id][changeAuthorId].Add(change);
        }
    }

    private void AddEventCount(DateTime date)
    {
        if (eventCountByDate.ContainsKey(date.Date))
        {
            eventCountByDate[date.Date]++;
        }
        else
        {
            eventCountByDate[date.Date] = 1;
        }
    }

    private void AddEventToDate(EventData data)
    {
        if (!eventsByDate.ContainsKey(data.when.Date))
        {
            eventsByDate[data.when.Date] = new();
        }
        eventsByDate[data.when.Date].Add(data);
        AddEventCount(data.when.Date);
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

    // TODO FINISH
    private void LoadVerticeWrappers()
    {
        
        foreach (var (key, value) in verticeData)
        {
            verticeWrappers[key] = new VerticeWrapper();
            verticeWrappers[key].verticeData = value;
        }
        
        foreach (var (key, value) in edgeData)
        {
            verticeWrappers[value.from].AddData(verticeData[value.from],value);
        }
        
        foreach (var val in verticeWrappers.Values)
        {
            // if(val.verticeData.verticeType != VerticeType.Change)
            //     continue;
            // if(!val.relatedEdges.Values.SelectMany(x=>x).Where(x=> String.Equals("created by",x.relation)).Any())
            // {
            //     Debug.LogWarning("Change doesnt have creation by person; "+ val.verticeData.id);
            // }
            
            val.GetAuthor();
        }
        
        

        // TODO testing
        // Dictionary<long, List<VerticeData>> dict = new();
        // foreach (var (key, value) in edgeData)
        // {
        //     if (verticeData[value.from].verticeType == VerticeType.Commit)
        //     {
        //         if (!dict.ContainsKey(value.from))
        //         {
        //             dict[value.from] = new();
        //         }
        //         dict[value.from].Add(verticeData[value.to]);
        //         // Debug.LogWarning(verticeData[value.from].verticeType +" --- "+verticeData[value.to].verticeType);    
        //     }
        //     
        //     // Debug.LogWarning("["+key+", "+value.type+"]"+verticeData[value.from].verticeType +" --- "+verticeData[value.to].verticeType);
        //     // if(verticeData[value.from].verticeType == VerticeType.Change || verticeData[value.from].verticeType == VerticeType.Person)
        //     //     continue;
        //     // Debug.Lo
        //     // Debug.LogWarning((verticeData[value.from].verticeType == VerticeType.Change || verticeData[value.from].verticeType == VerticeType.Person) ? "Change/Person" : verticeData[value.from].verticeType);
        // }
        //
        // foreach (var (key, value) in dict)
        // {
        //     // if(value.Where(x=>x.verticeType == VerticeType.Ticket).Count() > 1)
        //         // Debug.LogWarning("CHANGE "+key+" :"+String.Join(",",value.Select(x=>x.verticeType).ToList()));
        //         // Debug.LogWarning("CHANGE :"+String.Join(",",value.Select(x=>x.verticeType).ToList()));
        //
        // }

    }
}