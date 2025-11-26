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
            // --- Check reactants exist in container ---
            bool allReactantsExist = r.reactants.All(req =>
                container.contents.Any(c => c.type == req.type && c.volume > 0f));
            if (!allReactantsExist) continue;

            // --- Check prerequisites ---
            if (r.requireHeatSource && !container.hasHeatSource) continue;
            if (r.requireVacuum && !container.isVacuum) continue;

            // --- Check temperature (only minimum required) ---
            if (container.currentTemperature < r.targetTemperature - r.temperatureMargin)
                continue;

            recipe = r;
            return true;
        }

        return false;
    }

}
