using System;
using System.Collections.Generic;
using UnityEngine;

// Java enum implementation from https://stackoverflow.com/a/469315
public class ColorMapping : ICloneable
{
    public static readonly ColorMapping HIGHLIGHTED = new(1, new Color32(226, 255, 0, 255), "Highlighted color");
    public static readonly ColorMapping HIDDEN = new(2, new Color32(118, 118, 118, 255), "Hidden color");
    public static readonly ColorMapping TILEMAPHIGHLIGHT = new(3, new Color32(0, 180, 0, 255), "Contributions Calendar Color");
    public static readonly ColorMapping TICKET = new(20, new Color32(230, 30, 30, 255), "Ticket vertice");
    public static readonly ColorMapping PERSON = new(21, new Color32(69, 229, 234, 255), "Person vertice");
    public static readonly ColorMapping FILE = new(22, new Color32(230, 100, 30, 255), "File vertice");
    public static readonly ColorMapping WIKI = new(23, new Color32(30, 230, 30, 255), "Wiki vertice");
    public static readonly ColorMapping REPOFILE = new(25, new Color32(30, 30, 230, 255), "RepoFile vertice");

    public static IEnumerable<ColorMapping> Values
    {
        get
        {
            yield return HIGHLIGHTED;
            yield return HIDDEN;
            yield return TILEMAPHIGHLIGHT;
            yield return TICKET;
            yield return PERSON;
            yield return FILE;
            yield return WIKI;
            yield return REPOFILE;
        }
    }

    public long id;
    public Color32 color;
    public string name;

    public ColorMapping(long _id, Color32 _color, string _name) =>
        (id, color, name) = (_id, _color, _name);

    // public double SurfaceWeight(double other) => other * SurfaceGravity(); Keeping example how to add method
    public override string ToString() => name;

    public object Clone()
    {
        return new ColorMapping(

            id,
            color,
            name
        );
    }
}