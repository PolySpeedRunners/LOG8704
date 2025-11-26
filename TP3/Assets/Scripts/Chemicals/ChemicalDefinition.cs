using UnityEngine;
using LiquidVolumeFX;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
public enum ChemicalType
{
    Water,
    Acid,
    Base,
    Solvent,
    Fuel
}

[System.Serializable]
public class ReactionRequirement
{
    public ChemicalType reactant;
    [Range(0f, 1f)]
    public float requiredRatio; // fraction of total volume needed
}

[CreateAssetMenu(menuName = "Chemistry/Chemical Definition")]
public class ChemicalDefinition : ScriptableObject
{
    public ChemicalType type;

    [Header("Visual")]
    public Color color;

    [Header("Reaction")]
    public List<ReactionRequirement> reactionIngredients; // e.g., A:0.33, B:0.66
    public ChemicalType reactionProduct;
}

public static class ReactionTable
{
    private static readonly Dictionary<(ChemicalType, ChemicalType), ChemicalType> reactions =
        new()
        {
            { (ChemicalType.Acid, ChemicalType.Base), ChemicalType.Water },
            { (ChemicalType.Base, ChemicalType.Acid), ChemicalType.Water },
        };

    public static bool TryGetProduct(ChemicalType r1, ChemicalType r2, out ChemicalType result)
    {
        return reactions.TryGetValue((r1, r2), out result);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ChemicalDefinition))]
public class ChemicalDefinitionAutoNormalizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector
        EditorGUI.BeginChangeCheck();
        DrawPropertiesExcluding(serializedObject, "m_Script");

        if (EditorGUI.EndChangeCheck())
        {
            // Apply inspector changes
            serializedObject.ApplyModifiedProperties();

            ChemicalDefinition def = (ChemicalDefinition)target;
            Normalize(def);
            EditorUtility.SetDirty(def);
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void Normalize(ChemicalDefinition def)
    {
        float sum = def.reactionIngredients.Sum(r => r.requiredRatio);
        if (sum <= 0f) return;

        foreach (var r in def.reactionIngredients)
            r.requiredRatio /= sum;
    }
}
#endif