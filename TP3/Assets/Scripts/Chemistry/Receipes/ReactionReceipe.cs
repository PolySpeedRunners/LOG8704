using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ChemicalRatio
{
    public ChemicalType type;
    [Range(0f, 1f)]
    public float ratio;
}

[CreateAssetMenu(menuName = "Chemistry/Reaction Recipe")]
public class ReactionRecipe : ScriptableObject
{
    public List<ChemicalRatio> reactants = new();
    public List<ChemicalRatio> products = new();

    [Range(0.01f, 10f)]
    public float reactionRate = 1f;
    // 1f = full conversion in 1 second if ratios allow

#if UNITY_EDITOR
    private void OnValidate()
    {
        NormalizeList(reactants);
        NormalizeList(products);
    }

    private void NormalizeList(List<ChemicalRatio> list)
    {
        float sum = list.Sum(c => c.ratio);
        if (sum <= 0f) return;
        for (int i = 0; i < list.Count; i++)
            list[i].ratio /= sum;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ReactionRecipe))]
public class ReactionRecipeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script");

        if (GUILayout.Button("Normalize Ratios"))
        {
            ReactionRecipe recipe = (ReactionRecipe)target;

            NormalizeList(recipe.reactants);
            NormalizeList(recipe.products);

            EditorUtility.SetDirty(recipe);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void NormalizeList(System.Collections.Generic.List<ChemicalRatio> list)
    {
        float sum = list.Sum(c => c.ratio);
        if (sum <= 0f) return;

        for (int i = 0; i < list.Count; i++)
        {
            list[i].ratio /= sum;
        }
    }
}
#endif


