using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
