using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "EdgeMaterial", menuName = "SO/EdgeMaterial", order = 1)]
public class EdgeMaterial : ScriptableObject
{
    public EdgeType edgeType;
    public Material material;
}