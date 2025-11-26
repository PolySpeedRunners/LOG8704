using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Chemistry/Reaction Database")]
public class ReactionDatabase : ScriptableObject
{
    public List<ReactionRecipe> reactions = new();

    public bool TryFindReaction(
        Dictionary<ChemicalType, float> contents,
        out ReactionRecipe recipe)
    {
        recipe = null;

        // Cannot react with no chemicals
        if (contents.Count == 0)
            return false;

        float total = contents.Values.Sum();
        if (total <= 0f)
            return false;

        foreach (var r in reactions)
        {
            // === RULE 1: All required reactants must exist ===
            bool missingIngredient = r.reactants.Any(req => !contents.ContainsKey(req.type));
            if (missingIngredient)
                continue;

            // === RULE 2: Each reactant must meet ratio requirement ===
            bool insufficientRatio = r.reactants.Any(req =>
            {
                float ratio = contents[req.type] / total;
                return ratio < req.ratio;
            });

            if (insufficientRatio)
                continue;

            // === RULE 3: No extra reactants that are not part of this recipe ===
            bool hasExtraReactants = contents.Keys.Any(k => !r.reactants.Any(i => i.type == k));
            if (hasExtraReactants)
                continue;

            // Reaction found!
            recipe = r;
            return true;
        }

        return false;
    }
}
