using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Chemistry/Reaction Database")]
public class ReactionDatabase : ScriptableObject
{
    public List<ReactionRecipe> reactions = new();

    public bool TryFindReaction(ChemicalContainer container, out ReactionRecipe recipe)
    {
        recipe = null;
        if (container.contents.Count == 0) return false;

        foreach (var r in reactions)
        {
            // --- Check reactants exist ---
            if (r.reactants.Any(req => !container.contents.ContainsKey(req.type)))
                continue;

            // --- Check prerequisites ---
            if (r.requireHeatSource && !container.hasHeatSource)
                continue;

            if (r.requireVacuum && !container.isVacuum)
                continue;

            // --- Check temperature within margin ---
            float tempDiff = Mathf.Abs(container.currentTemperature - r.targetTemperature);
            if (tempDiff > r.temperatureMargin)
                continue;

            recipe = r;
            return true;
        }

        return false;
    }
}
