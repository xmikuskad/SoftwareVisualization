using System;
using System.Collections.Generic;

// Parsed from json
[Serializable]
public class RawEdgeData
{
    public long id;
    public long to;
    public long from;
    public string text;
    public string archetype;
    public Dictionary<String, System.Object> attributes;
}
