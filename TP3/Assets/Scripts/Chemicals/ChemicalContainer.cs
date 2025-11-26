using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LiquidVolumeFX;

[System.Serializable]
public class ChemicalAmount
{
    public ChemicalType type;
    public float volume; // mL
}

public class ChemicalContainer : MonoBehaviour
{
    [SerializeField] public float maxVolume = 250f;
    [SerializeField] public LiquidVolume liquidVolume;
    [SerializeField] public float fillSpeed = 0.01f;
    [SerializeField] public ChemicalDatabase database;
    [SerializeField] public List<ChemicalAmount> defaultContents = new();

    public Dictionary<ChemicalType, float> contents = new();
    public float totalVolume => contents.Values.Sum();

    private void Awake()
    {
        LoadDefaultContents();

        // Initialize the current color to match the initial contents
        currentColor = ComputeBlendedColor();

        UpdateVisual();
    }

    private Color ComputeBlendedColor()
    {
        if (contents.Count == 0) return Color.black;

        Color blended = Color.black;
        foreach (var kvp in contents)
        {
            var color = database.Get(kvp.Key).color;
            float ratio = kvp.Value / totalVolume;
            blended += color * ratio;
        }
        return blended;
    }

    private void LoadDefaultContents()
    {
        // Convert list → dictionary
        contents = defaultContents.ToDictionary(c => c.type, c => c.volume);

        // Clamp if default exceeds maxVolume
        float total = totalVolume;
        if (total > maxVolume)
        {
            float scale = maxVolume / total;
            foreach (var key in contents.Keys.ToList())
                contents[key] *= scale;
        }
    }

    private Color currentColor;
    [SerializeField] private float colorLerpSpeed = 3f;

    private void UpdateVisual()
    {
        if (liquidVolume != null)
        {
            // Update fill level
            liquidVolume.level = totalVolume / maxVolume;

            if (contents.Count > 0)
            {
                // Compute target blended color
                Color targetColor = ComputeBlendedColor();

                // Smoothly lerp from currentColor to targetColor
                currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorLerpSpeed);

                // Apply to LiquidVolumeFX
                liquidVolume.liquidColor1 = currentColor;
                liquidVolume.liquidColor2 = currentColor;
            }
        }
    }


    // Add liquids from another source (spill, another container)
    public void AddLiquid(Dictionary<ChemicalType, float> addedContents)
    {
        if (addedContents == null || addedContents.Count == 0)
            return;

        float incomingVolume = addedContents.Values.Sum();
        float availableSpace = maxVolume - totalVolume;
        if (availableSpace <= 0f)
            return; // Beaker full

        // Scale if over maxVolume
        float scale = 1f;
        if (incomingVolume > availableSpace)
            scale = availableSpace / incomingVolume;

        // Merge contents
        foreach (var kvp in addedContents)
        {
            var type = kvp.Key;
            var amount = kvp.Value * scale;
            if (!contents.ContainsKey(type))
                contents[type] = 0f;
            contents[type] += amount;
        }

        TryReact();
        UpdateVisual();
    }

    // Remove a proportional amount of liquid
    public Dictionary<ChemicalType, float> TakeProportional(float amount)
    {
        float total = totalVolume;
        if (total <= 0f) return new Dictionary<ChemicalType, float>();

        Dictionary<ChemicalType, float> removed = new();
        foreach (var key in contents.Keys.ToList())
        {
            float ratio = contents[key] / total;
            float take = ratio * amount;
            contents[key] -= take;
            removed[key] = take;

            if (contents[key] <= 0.0001f)
                contents.Remove(key);
        }

        UpdateVisual();
        return removed;
    }

    // Reaction logic
    private void TryReact()
    {
        if (totalVolume <= 0f) return;

        foreach (var kvp in contents.ToList())
        {
            var def = database.Get(kvp.Key);
            if (def == null || def.reactionIngredients.Count == 0)
                continue;

            // Compute reaction progress (min ratio of ingredients)
            float reactionProgress = 1f;
            foreach (var req in def.reactionIngredients)
            {
                float have = contents.ContainsKey(req.reactant) ? contents[req.reactant] : 0f;
                float ratio = have / totalVolume;
                float progress = Mathf.Clamp01(ratio / req.requiredRatio);
                reactionProgress = Mathf.Min(reactionProgress, progress);
            }

            if (reactionProgress <= 0f) continue;

            // Remove reactants proportionally
            foreach (var req in def.reactionIngredients)
            {
                if (contents.ContainsKey(req.reactant))
                    contents[req.reactant] -= req.requiredRatio * totalVolume * reactionProgress;

                if (contents.ContainsKey(req.reactant) && contents[req.reactant] <= 0.0001f)
                    contents.Remove(req.reactant);
            }

            // Add product
            if (!contents.ContainsKey(def.reactionProduct))
                contents[def.reactionProduct] = 0f;
            contents[def.reactionProduct] += reactionProgress * totalVolume;
        }
    }
}
