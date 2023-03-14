using System;
using System.Reflection;

[Serializable]
public class EdgeData
{
    public long id { get; set; }
    public long to { get; set; }
    public long from { get; set; }
    public EdgeType type { get; set; }
    
    
    // attributes
    public string relation { get; set; }


    public EdgeData(RawEdgeData rawEdgeData)
    {
        this.id = rawEdgeData.id;
        this.to = rawEdgeData.to;
        this.from = rawEdgeData.from;
        this.type = (EdgeType)System.Enum.Parse(typeof(EdgeType), rawEdgeData.archetype);
            
        foreach (string key in rawEdgeData.attributes.Keys)
        {
            PropertyInfo info = typeof(EdgeData).GetProperty(DataUtils.GetFormattedPropertyName(key));
            info.SetValue(this, rawEdgeData.attributes[key],null);
        }
    }
    
    
    // for debugging
    public override string ToString()
    {
        return $"EdgeData {{ id={id}, to={to}, from={from}, type=<b>{type}</b>,relation={relation} }}";
    }
}