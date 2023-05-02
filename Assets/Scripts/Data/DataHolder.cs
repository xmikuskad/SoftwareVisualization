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

    public List<VerticeWrapper> spawnAtStart = new();

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
        
        foreach (var (key, value) in ticketToChangeListPerAuthor)
        {
         
            Debug.LogWarning("Ticket "+key+": "+string.Join(", ",
                value
                    .SelectMany(kv => kv.Value) // Flatten the values of the dictionary into a single IEnumerable<VerticeData>
                    .GroupBy(v => v.id) // Group the VerticeData objects by the Id property
                        .Select(g => g.First()) // Select the first VerticeData object from each group
                        .ToList().Select(x=>x.id))
            + " ||| "+string.Join(", ",value
                .SelectMany(kv => kv.Value).ToList().Select(x=>x.id)));  
            
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
            
            if (!usageHistogram.ContainsKey(v.verticeData.id))
            {
                usageHistogram[v.verticeData.id] = 0;
            }
            usageHistogram[v.verticeData.id]+=1;
            
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

                usageHistogram[related.id] += 1;
            }
            // Debug.Log("Change "+v.verticeData.id +": Tickets:"+string.Join(", ",
            //     v.GetRelatedVertices().Where(x=>x.verticeType == VerticeType.Ticket).Select(x=>x.id)));
        }

        HashSet<long> usedVertices = usageHistogram.Keys.ToHashSet();
        foreach (var l in verticeWrappers.Keys.Where(x=>!usedVertices.Contains(x)))
        {
            spawnAtStart.Add(verticeWrappers[l]);
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