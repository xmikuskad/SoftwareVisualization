using System;
using System.Collections.Generic;
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

    // TODO This function needs a rework. I know that data are only loading for tickets.
    public void LoadData()
    {
        // Create a map so we can get personId from personString
        foreach (var data in this.verticeData.Values.Where(e => e.verticeType == VerticeType.Person))
        {
            personIds[data.name] = data.id;
        }

        personIds["_unknown"] = -1;

        fillTicketToChangeListPerAuthor();

        LoadVerticeWrappers();

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

        // Load vertices
        foreach (var (key, value) in verticeData)
        {
            verticeWrappers[key] = new VerticeWrapper();
            verticeWrappers[key].verticeData = value;
        }
        
        // Load edges
        foreach (var (key, value) in edgeData)
        {
            verticeWrappers[value.from].AddData(verticeData[value.to], value);
        }

        Dictionary<long, long> usageHistogram = new();
        foreach (var v in verticeWrappers.Values.Where(x=>x.verticeData.verticeType == VerticeType.Change))
        {
            if (!changesByDate.ContainsKey(v.GetTimeWithoutHours()))
            {
                changesByDate[v.GetTimeWithoutHours()] = new();
            }
            if(!changesByDate[v.GetTimeWithoutHours()].Contains(v))
                changesByDate[v.GetTimeWithoutHours()].Add(v);
            
            foreach (var related in v.GetRelatedVertices())
            {
                if (!verticesByDate.ContainsKey(v.GetTimeWithoutHours()))
                {
                    verticesByDate[v.GetTimeWithoutHours()] = new();
                }
                if(!verticesByDate[v.GetTimeWithoutHours()].Contains(verticeWrappers[related.id]))
                    verticesByDate[v.GetTimeWithoutHours()].Add(verticeWrappers[related.id]);

                if (!datesForVertice.ContainsKey(related.id))
                {
                    datesForVertice[related.id] = new();
                }
                if(!datesForVertice[related.id].Contains(v.GetTimeWithoutHours()))
                    datesForVertice[related.id].Add(v.GetTimeWithoutHours());
                
                if (!usageHistogram.ContainsKey(related.id))
                {
                    usageHistogram[related.id] = 0;
                }
                usageHistogram[related.id]+=1;
            }
        }

        HashSet<long> usedVertices = usageHistogram.Keys.ToHashSet();
        foreach (var l in verticeWrappers.Keys.Where(x=>!usedVertices.Contains(x)))
        {
            if (!changesByDate.ContainsKey(DateTime.MinValue.Date))
            {
                changesByDate[DateTime.MinValue.Date] = new();
            }
            if(!changesByDate[DateTime.MinValue.Date].Contains(verticeWrappers[l]))
                changesByDate[DateTime.MinValue.Date].Add(verticeWrappers[l]);
        }

        this.orderedDates = changesByDate.Keys.Distinct().OrderBy(x => x).ToList();

        this.startDate = verticesByDate.Keys.Where(x => x != DateTime.MinValue.Date).Min();
         foreach (var (key, value) in datesForVertice)
         {
             verticeWrappers[key].SetDates(value);
             verticeWrappers[key].updateCount = Math.Max(usageHistogram[key] -1,1);
         }


    }
}