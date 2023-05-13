
using System;
using System.Collections.Generic;
using UnityEngine;

public class PreferencesManager : MonoBehaviour
{
    private Dictionary<long, ColorMapping> colorMappings = new();

    public event Action<Dictionary<long, ColorMapping>> MappingChangedEvent;


    public void Awake()
    {
        foreach (var colorMapping in ColorMapping.Values)
        {
            colorMappings[colorMapping.id] = colorMapping;
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

    public ColorMapping GetColorMappingByType(VerticeType verticeType)
    {
        switch (verticeType)
        {
            case VerticeType.Ticket:
                return colorMappings[ColorMapping.TICKET_PLATFORM.id];
            case VerticeType.Person:
                return colorMappings[ColorMapping.PERSON_PLATFORM.id];
            case VerticeType.File:
                return colorMappings[ColorMapping.FILE_PLATFORM.id];
            case VerticeType.RepoFile:
                return colorMappings[ColorMapping.REPOFILE_PLATFORM.id];
            case VerticeType.Wiki:
                return colorMappings[ColorMapping.WIKI_PLATFORM.id];
        }

        return ColorMapping.PERSON;
    }

    public Dictionary<long, ColorMapping> GetMappings()
    {
        Dictionary<long, ColorMapping> copy = new();
        foreach (var val in colorMappings.Values)
        {
            copy[val.id] = (ColorMapping)val.Clone();
        }

        return copy;
    }
}
