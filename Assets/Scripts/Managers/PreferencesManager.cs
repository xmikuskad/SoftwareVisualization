
using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using UnityEngine;

public class PreferencesManager : MonoBehaviour
{
    private Dictionary<long, ColorMapping> colorMappings = new();
    private Dictionary<long, ShapeMapping> shapeMappings = new();

    public event Action<Dictionary<long, ColorMapping>,Dictionary<long, ShapeMapping>> MappingChangedEvent;


    public void Awake()
    {
        foreach (var colorMapping in ColorMapping.Values)
        {
            colorMappings[colorMapping.id] = colorMapping;
        }
        
        foreach (var shapeMapping in ShapeMapping.Values)
        {
            shapeMappings[shapeMapping.id] = shapeMapping;
        }
    }

    public void SetMappings(List<ColorMapping> newMapping, List<ShapeMapping> newShapes)
    {
        foreach (var colorMapping in newMapping)
        {
            colorMappings[colorMapping.id] = colorMapping;
        }
        
        foreach (var shapeMapping in newShapes)
        {
            shapeMappings[shapeMapping.id] = shapeMapping;
        }

        MappingChangedEvent?.Invoke(colorMappings,shapeMappings);
    }

    public ColorMapping GetColorMapping(ColorMapping c)
    {
        return colorMappings[c.id];
    }
    
    public ShapeMapping GetShapeMapping(ShapeMapping s)
    {
        return shapeMappings[s.id];
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

    public Dictionary<long, ColorMapping> GetColorMappings()
    {
        Dictionary<long, ColorMapping> copy = new();
        foreach (var val in colorMappings.Values)
        {
            copy[val.id] = (ColorMapping)val.Clone();
        }

        return copy;
    }
    
    public Dictionary<long, ShapeMapping> GetShapeMappings()
    {
        Dictionary<long, ShapeMapping> copy = new();
        foreach (var val in shapeMappings.Values)
        {
            copy[val.id] = (ShapeMapping)val.Clone();
        }

        return copy;
    }
}
