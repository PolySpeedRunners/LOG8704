using UnityEngine;
using LiquidVolumeFX;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
public enum ChemicalType
{
    Water,
    PVA,
    AceticAcid,
    Benzene,
    Acetone,
}

[CreateAssetMenu(menuName = "Chemistry/Chemical Definition")]
public class ChemicalDefinition : ScriptableObject
{
    public ChemicalType type;

    [Header("Visual")]
    public Color color;
}
