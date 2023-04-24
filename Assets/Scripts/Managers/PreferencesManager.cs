
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PreferencesManager: MonoBehaviour
{
    private Dictionary<long,ColorMapping> colorMappings = new();

    public event Action<Dictionary<long,ColorMapping>> MappingChangedEvent;
    
    
    public void Awake()
    {
        foreach (var colorMapping in ColorMapping.Values)
        {
            colorMappings[colorMapping.id]=colorMapping;
        }
    }

    public void SetColorMappings(List<ColorMapping> newMapping)
    {
        foreach (var colorMapping in newMapping)
        {
            colorMappings[colorMapping.id] = colorMapping;
        }
        
        MappingChangedEvent?.Invoke(colorMappings);
    }

    public ColorMapping GetColorMapping(ColorMapping c)
    {
        return colorMappings[c.id];
    }

    public Dictionary<long,ColorMapping> GetMappings()
    {
        Dictionary<long, ColorMapping> copy = new();
        foreach (var val in colorMappings.Values)
        {
            copy[val.id] = (ColorMapping)val.Clone();
        }

        return copy;
    }
}
