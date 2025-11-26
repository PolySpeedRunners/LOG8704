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

[System.Serializable]
public class SparklingSettings
{
    [Header("Basic Sparkling Properties")]
    public float baseSpeed = 0.05f;
    public float baseIntensity = 1.785f;
    public float baseAmount = 0f;

    public float maxSpeed = 0.193f;
    public float maxIntensity = 0.65f;
    public float maxAmount = 0.193f;

    [Header("Temperature Range")]
    public float temperatureStart = 75f;
    public float temperatureEnd = 100f;

    [Header("Turbulence Settings")]
    public float turbulence1Step = 2.7f; // max change per update for turbulence1
    public float turbulence1Start = 0f;  // min value for turbulence1
    public float turbulence1End = 1f;    // max value for turbulence1

    public float turbulence2Step = 0.01f; // max change per update for turbulence2
    public float turbulence2Start = 0.0f;  // min value for turbulence2
    public float turbulence2End = 0.1f;      // max value for turbulence2
}

public class ChemicalContainer : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] public float maxVolume = 250f;
    [SerializeField] private LiquidVolume liquidVolume;
    [SerializeField] private float fillSpeed = 0.01f;
    [SerializeField] private float ambientTemperature = 25f;
    [SerializeField] private float temperatureDecayRate = 1f; // °C per second
    [SerializeField] private float reactionInterval = 0.1f;

    [Header("Chemical Data")]
    [SerializeField] private ChemicalDatabase chemicalData;
    [SerializeField] private ReactionDatabase reactionData;
    [SerializeField] private List<ChemicalAmount> defaultContents = new();

    [Header("Sparkling Settings")]
    [SerializeField] private SparklingSettings sparkling;

    public Dictionary<ChemicalType, float> contents = new();
    private Color currentColor;
    private float reactionTimer = 0f;

    public float totalVolume => contents.Values.Sum();
    public float currentTemperature = 25f;
    public bool hasHeatSource = false;
    public bool isVacuum = false;

    private void Awake()
    {
        LoadDefaultContents();
        currentColor = ComputeBlendedColor();
        turbulence2Value = sparkling.turbulence2Start;
        UpdateVisual();
    }

    private void Update()
    {
        // Temperature decay
        if (!hasHeatSource)
            currentTemperature = Mathf.MoveTowards(currentTemperature, ambientTemperature, temperatureDecayRate * Time.deltaTime);

        // Visual
        UpdateVisual();

        // Sparkling effect
        UpdateSparkling();

        // Reactions at fixed intervals
        reactionTimer += Time.deltaTime;
        if (reactionTimer >= reactionInterval)
        {
            TryReact();
            reactionTimer = 0f;
        }
    }

    private void LoadDefaultContents()
    {
        contents = defaultContents.ToDictionary(c => c.type, c => c.volume);

        float total = totalVolume;
        if (total > maxVolume)
        {
            float scale = maxVolume / total;
            foreach (var key in contents.Keys.ToList())
                contents[key] *= scale;
        }
    }

    private Color ComputeBlendedColor()
    {
        if (contents.Count == 0) return Color.black;

        float total = totalVolume;
        var dominant = contents.OrderByDescending(kvp => kvp.Value).First();
        Color dominantColor = chemicalData.Get(dominant.Key).color;

        Color secondaryBlend = Color.black;
        float secondaryWeight = 0f;

        foreach (var kvp in contents)
        {
            if (kvp.Key == dominant.Key) continue;
            float ratio = kvp.Value / total;
            if (ratio < 0.03f) continue;

            float weight = Mathf.Pow(ratio, 1.6f);
            secondaryBlend += chemicalData.Get(kvp.Key).color * weight;
            secondaryWeight += weight;
        }

        if (secondaryWeight <= 0f) return dominantColor;

        secondaryBlend /= secondaryWeight;
        return Color.Lerp(dominantColor, secondaryBlend, 0.25f);
    }

    private void UpdateVisual()
    {
        if (liquidVolume == null) return;

        liquidVolume.level = totalVolume / maxVolume;

        if (contents.Count == 0)
        {
            // Optional: show transparent liquid instead of black
            liquidVolume.liquidColor1 = new Color(1f, 1f, 1f, 0f);
            liquidVolume.liquidColor2 = new Color(1f, 1f, 1f, 0f);
            return;
        }

        Color targetColor = ComputeBlendedColor();

        // Smooth transition
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 3f);

        // Keep alpha = 1, adjust brightness with intensity
        float brightness = Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(totalVolume / maxVolume));
        Color finalColor = new Color(
            currentColor.r * brightness,
            currentColor.g * brightness,
            currentColor.b * brightness,
            1f // alpha always 1 so LVFX shows the color
        );

        liquidVolume.liquidColor1 = finalColor;
        liquidVolume.liquidColor2 = finalColor;
    }


    public void AddLiquid(Dictionary<ChemicalType, float> addedContents)
    {
        if (addedContents == null || addedContents.Count == 0) return;

        float incomingVolume = addedContents.Values.Sum();
        float availableSpace = maxVolume - totalVolume;
        if (availableSpace <= 0f) return;

        float scale = 1f;
        if (incomingVolume > availableSpace)
            scale = availableSpace / incomingVolume;

        foreach (var kvp in addedContents)
        {
            if (!contents.ContainsKey(kvp.Key))
                contents[kvp.Key] = 0f;
            contents[kvp.Key] += kvp.Value * scale;
        }

        TryReact();
        UpdateVisual();
    }

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

        return removed;
    }

    private void TryReact()
    {
        if (reactionData == null || contents.Count == 0) return;
        if (!reactionData.TryFindReaction(this, out ReactionRecipe r)) return;

        float total = totalVolume;
        if (total <= 0f) return;

        float limitingFactor = float.MaxValue;
        foreach (var req in r.reactants)
        {
            float have = contents[req.type];
            float requiredAmount = total * req.ratio;
            limitingFactor = Mathf.Min(limitingFactor, have / requiredAmount);
        }
        if (limitingFactor <= 0f) return;

        float tempDiff = Mathf.Abs(currentTemperature - r.targetTemperature);
        float tempFactor = Mathf.Clamp01(1f - tempDiff / r.temperatureMargin);

        float reactionStep = r.reactionRate * Time.deltaTime * tempFactor;
        float actualProgress = Mathf.Min(reactionStep, limitingFactor);
        if (actualProgress <= 0f) return;

        foreach (var req in r.reactants)
        {
            float amountToRemove = total * req.ratio * actualProgress;
            contents[req.type] -= amountToRemove;
            if (contents[req.type] <= 0.0001f)
                contents.Remove(req.type);
        }

        foreach (var prod in r.products)
        {
            float amountToAdd = total * prod.ratio * actualProgress;
            if (!contents.ContainsKey(prod.type)) contents[prod.type] = 0f;
            contents[prod.type] += amountToAdd;
        }

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

    private float turbulence1Value = 0f;
    private float turbulence2Value = 0.5f; // starts at 0.5
    [SerializeField] private float turbulence1Step = 0.1f; // max change per update
    [SerializeField] private float turbulence2Step = 0.01f; // smaller change per update

    private void UpdateSparkling()
    {
        if (liquidVolume == null) return;

        // Only start effects if temperature is above threshold
        float threshold = 60f; // start effects at 60°C
        if (currentTemperature < threshold)
        {
            // Optionally reset turbulences to base/start values
            liquidVolume.turbulence1 = sparkling.turbulence1Start;
            liquidVolume.turbulence2 = sparkling.turbulence2Start;
            liquidVolume.sparklingAmount = sparkling.baseAmount;
            liquidVolume.sparklingIntensity = sparkling.baseIntensity;
            liquidVolume.speed = sparkling.baseSpeed;
            liquidVolume.requireBubblesUpdate = true;
            return;
        }

        // Normalize temperature between threshold and temperatureEnd
        float t = Mathf.InverseLerp(threshold, sparkling.temperatureEnd, currentTemperature);
        t = Mathf.Clamp01(t);

        // LVFX sparkling properties
        liquidVolume.sparklingAmount = Mathf.Lerp(sparkling.baseAmount, sparkling.maxAmount, t);
        liquidVolume.sparklingIntensity = Mathf.Lerp(sparkling.baseIntensity, sparkling.maxIntensity, t);
        liquidVolume.speed = Mathf.Lerp(sparkling.baseSpeed, sparkling.maxSpeed, t);

        // Turbulence 1 - gradually change within defined range
        float dir1 = Random.value > 0.5f ? 1f : -1f;
        turbulence1Value += dir1 * sparkling.turbulence1Step * t; // scale by temperature factor
        turbulence1Value = Mathf.Clamp(turbulence1Value, sparkling.turbulence1Start, sparkling.turbulence1End);
        liquidVolume.turbulence1 = turbulence1Value;

        // Turbulence 2 - gradually change within defined range
        float dir2 = Random.value > 0.5f ? 1f : -1f;
        turbulence2Value += dir2 * sparkling.turbulence2Step * t; // scale by temperature factor
        turbulence2Value = Mathf.Clamp(turbulence2Value, sparkling.turbulence2Start, sparkling.turbulence2End);
        liquidVolume.turbulence2 = turbulence2Value;

        liquidVolume.requireBubblesUpdate = true;
    }



}
