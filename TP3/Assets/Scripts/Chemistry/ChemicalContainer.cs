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
    [SerializeField] public ChemicalDatabase chemicalData;
    [SerializeField] public ReactionDatabase reactionData;

    [SerializeField] public List<ChemicalAmount> defaultContents = new();

    public Dictionary<ChemicalType, float> contents = new();
    public float totalVolume => contents.Values.Sum();

    private void Awake()
    {
        LoadDefaultContents();

        currentColor = ComputeBlendedColor(); // now uses the stable blending

        UpdateVisual();
    }


    private Color ComputeBlendedColor()
    {
        if (contents.Count == 0)
            return Color.black;

        float total = totalVolume;

        // 1. Find dominant chemical by volume
        var dominant = contents
            .OrderByDescending(kvp => kvp.Value)
            .First();

        Color dominantColor = chemicalData.Get(dominant.Key).color;

        // 2. Collect meaningful secondary colors
        Color secondaryBlend = Color.black;
        float secondaryWeight = 0f;

        foreach (var kvp in contents)
        {
            if (kvp.Key == dominant.Key) continue;

            float ratio = kvp.Value / total;

            // Ignore tiny contaminants (<3% total volume)
            if (ratio < 0.03f)
                continue;

            // Moderate nonlinear influence
            float weight = Mathf.Pow(ratio, 1.6f); // makes big components dominate strongly

            secondaryBlend += chemicalData.Get(kvp.Key).color * weight;
            secondaryWeight += weight;
        }

        // If no secondaries, return dominant
        if (secondaryWeight <= 0f)
            return dominantColor;

        secondaryBlend /= secondaryWeight;

        // 3. Final mix: dominant has heavier weight
        return Color.Lerp(dominantColor, secondaryBlend, 0.25f);
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

    [SerializeField] private float colorLerpSpeed = 3f;
    private Color currentColor;

    private void UpdateVisual()
    {
        if (liquidVolume == null)
            return;

        // Update liquid height
        liquidVolume.level = totalVolume / maxVolume;

        if (contents.Count == 0)
            return;

        // 1. Compute target color using stable blending
        Color targetColor = ComputeBlendedColor();

        // 2. Smooth transition
        currentColor = Color.Lerp(
            currentColor,
            targetColor,
            Time.deltaTime * colorLerpSpeed
        );

        // 3. Apply visual intensity based on fill amount
        float intensity = Mathf.Clamp01(totalVolume / maxVolume);
        Color finalColor = currentColor * Mathf.Lerp(0.6f, 1f, intensity);

        // 4. Assign to LVFX
        liquidVolume.liquidColor1 = finalColor;
        liquidVolume.liquidColor2 = finalColor;
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

    private void TryReact()
    {
        if (reactionData == null || contents.Count == 0)
            return;

        if (!reactionData.TryFindReaction(contents, out ReactionRecipe r))
            return;

        float total = totalVolume;
        if (total <= 0f)
            return;

        // ---------------------------------------------------------
        // 1. Find limiting reactant (how far can the reaction go?)
        // ---------------------------------------------------------
        float limitingFactor = float.MaxValue;

        foreach (var req in r.reactants)
        {
            if (!contents.TryGetValue(req.type, out float have))
                return; // missing reactant => can't react

            float requiredAmount = total * req.ratio;
            if (requiredAmount <= 0f)
                return;

            float possible = have / requiredAmount;
            limitingFactor = Mathf.Min(limitingFactor, possible);
        }

        if (limitingFactor <= 0f)
            return;

        // ---------------------------------------------------------
        // 2. Apply reaction rate (Time.deltaTime safe)
        // ---------------------------------------------------------
        float reactionStep = r.reactionRate * Time.deltaTime;
        float actualProgress = Mathf.Min(reactionStep, limitingFactor);

        if (actualProgress <= 0f)
            return;

        // ---------------------------------------------------------
        // 3. Consume reactants
        // ---------------------------------------------------------
        foreach (var req in r.reactants)
        {
            float amountToRemove = total * req.ratio * actualProgress;

            contents[req.type] -= amountToRemove;
            if (contents[req.type] <= 0.0001f)
                contents.Remove(req.type);
        }

        // ---------------------------------------------------------
        // 4. Add products
        // ---------------------------------------------------------
        foreach (var prod in r.products)
        {
            float amountToAdd = total * prod.ratio * actualProgress;

            if (!contents.ContainsKey(prod.type))
                contents[prod.type] = 0f;

            contents[prod.type] += amountToAdd;
        }

        // ---------------------------------------------------------
        // 5. Ensure no floating point garbage
        // ---------------------------------------------------------
        CleanSmallEntries();
    }

    private void CleanSmallEntries()
    {
        foreach (var key in contents.Keys.ToList())
        {
            if (contents[key] <= 0.0001f)
                contents.Remove(key);
        }
    }

}
