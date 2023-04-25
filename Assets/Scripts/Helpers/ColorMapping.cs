using System;
using System.Collections.Generic;
using UnityEngine;

// Java enum implementation from https://stackoverflow.com/a/469315
public class ColorMapping : ICloneable
{
    public static readonly ColorMapping HIGHLIGHTED = new(1, new Color32(226, 255, 0, 255), "Highlighted color");
    public static readonly ColorMapping HIDDEN = new(2, new Color32(118, 118, 118, 255), "Hidden color");
    public static readonly ColorMapping TILEMAPHIGHLIGHT = new(3, new Color32(0, 180, 0, 255), "Contributions Calendar Color");
    public static readonly ColorMapping UNHIGHLIGHTED2 = new(20, new Color32(255, 0, 0, 255), "TODO");
    public static readonly ColorMapping UNHIGHLIGHTED3 = new(21, new Color32(255, 0, 0, 255), "TODO");
    public static readonly ColorMapping UNHIGHLIGHTED4 = new(22, new Color32(255, 0, 0, 255), "TODO");
    public static readonly ColorMapping UNHIGHLIGHTED5 = new(23, new Color32(255, 0, 0, 255), "TODO");
    public static readonly ColorMapping UNHIGHLIGHTED6 = new(24, new Color32(255, 0, 0, 255), "TODO");
    public static readonly ColorMapping UNHIGHLIGHTED7 = new(25, new Color32(255, 0, 0, 255), "TODO");

    public static IEnumerable<ColorMapping> Values
    {
        get
        {
            yield return HIGHLIGHTED;
            yield return HIDDEN;
            yield return TILEMAPHIGHLIGHT;
            yield return UNHIGHLIGHTED2;
            yield return UNHIGHLIGHTED3;
            yield return UNHIGHLIGHTED4;
            yield return UNHIGHLIGHTED5;
            yield return UNHIGHLIGHTED6;
            yield return UNHIGHLIGHTED7;
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