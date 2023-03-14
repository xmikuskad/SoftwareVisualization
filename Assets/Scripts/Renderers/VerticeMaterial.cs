using UnityEngine;

[CreateAssetMenu(fileName = "VerticeMaterial", menuName = "SO/VerticeMaterial", order = 2)]
public class VerticeMaterial : ScriptableObject
{
    public VerticeType verticeType;
    public Material material;
}