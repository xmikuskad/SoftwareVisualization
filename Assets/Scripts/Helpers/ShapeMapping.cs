using System;
using System.Collections.Generic;
using Helpers;
using UnityEngine;

// Java enum implementation from https://stackoverflow.com/a/469315
public class ShapeMapping : ICloneable
{
    
    public static readonly ShapeMapping TICKET = new(20, VerticeShape.CUBE, "Ticket shape");
    public static readonly ShapeMapping PERSON = new(21, VerticeShape.CUBE, "Person shape");
    public static readonly ShapeMapping FILE = new(22, VerticeShape.CUBE, "File shape");
    public static readonly ShapeMapping WIKI = new(23, VerticeShape.CUBE, "Wiki shape");
    public static readonly ShapeMapping REPOFILE = new(25, VerticeShape.CUBE, "RepoFile shape");

    public static IEnumerable<ShapeMapping> Values
    {
        get
        {
            yield return TICKET;
            yield return PERSON;
            yield return FILE;
            yield return WIKI;
            yield return REPOFILE;
        }
    }

    public long id;
    public VerticeShape shape;
    public string name;

    public ShapeMapping(long _id, VerticeShape _shape, string _name) =>
        (id, shape, name) = (_id, _shape, _name);

    public override string ToString() => name;

    public object Clone()
    {
        return new ShapeMapping(

            id,
            shape,
            name
        );
    }
}