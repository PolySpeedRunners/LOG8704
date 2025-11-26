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
    public float turbulence1Step = 0.7123f; // max change per update for turbulence1
    public float turbulence1Start = 0f;  // min value for turbulence1
    public float turbulence1End = 1f;    // max value for turbulence1

    [Header("Foam Settings")]
    public float foamDensityStart = 0.0f; // max change per update for turbulence1
    public float foamDensityEnd = 0.022f; // max change per update for turbulence1

    public float foamThicknessStart = 0f;
    public float foamThicknessEnd = 0.3f;
    public float foamScale = 0.714f;
    public float foamTurbulence = 0.5f;
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
        if (liquidVolume != null)
        {

            // Sparkling defaults
            liquidVolume.sparklingAmount = sparkling.baseAmount;
            liquidVolume.sparklingIntensity = sparkling.baseIntensity;
            liquidVolume.speed = sparkling.baseSpeed;

            // Turbulence defaults
            liquidVolume.turbulence1 = sparkling.turbulence1Start;

            // Foam defaults
            liquidVolume.foamDensity = sparkling.foamDensityStart;
            liquidVolume.foamScale = sparkling.foamScale;
            liquidVolume.foamThickness = sparkling.foamThicknessStart;
            liquidVolume.foamTurbulence = sparkling.foamTurbulence;

            liquidVolume.requireBubblesUpdate = true;
        }

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

        QuestObjectiveManager.Instance.NotifyReaction("@Coronaxus help me out here what do i display");
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


    private void UpdateSparkling()
    {
        if (liquidVolume == null) return;

        float threshold = sparkling.temperatureStart;
        float maxTemp = sparkling.temperatureEnd;

        if (currentTemperature < threshold)
        {
            // Reset all to base values
            liquidVolume.sparklingAmount = Mathf.Clamp01(sparkling.baseAmount);
            liquidVolume.sparklingIntensity = Mathf.Clamp01(sparkling.baseIntensity);
            liquidVolume.speed = Mathf.Clamp01(sparkling.baseSpeed);

            liquidVolume.turbulence1 = 0.4f;

            liquidVolume.foamDensity = Mathf.Clamp01(sparkling.foamDensityStart);
            liquidVolume.foamScale = Mathf.Clamp01(sparkling.foamScale);
            liquidVolume.foamThickness = Mathf.Clamp01(sparkling.foamThicknessStart);
            liquidVolume.foamTurbulence = Mathf.Clamp01(sparkling.foamTurbulence);

            liquidVolume.requireBubblesUpdate = true;
            return;
        }

        // Normalized temperature factor
        float t = Mathf.InverseLerp(threshold, maxTemp, currentTemperature);
        t = Mathf.Clamp01(t);

        // Sparkling properties (clamped)
        liquidVolume.sparklingAmount = Mathf.Clamp01(Mathf.Lerp(sparkling.baseAmount, sparkling.maxAmount, t));
        liquidVolume.sparklingIntensity = Mathf.Clamp01(Mathf.Lerp(sparkling.baseIntensity, sparkling.maxIntensity, t));
        liquidVolume.speed = Mathf.Clamp01(Mathf.Lerp(sparkling.baseSpeed, sparkling.maxSpeed, t));

        // Foam properties (clamped)
        liquidVolume.foamDensity = Mathf.Clamp01(Mathf.Lerp(Mathf.Max(0f, sparkling.foamDensityStart),
                                                           Mathf.Max(0f, sparkling.foamDensityEnd), t));
        liquidVolume.foamThickness = Mathf.Clamp01(Mathf.Lerp(sparkling.foamThicknessStart, sparkling.foamThicknessEnd, t));
        liquidVolume.foamTurbulence = Mathf.Clamp01(Mathf.Lerp(sparkling.foamTurbulence, 0.7f, t));

        // Turbulence oscillation between 0.4 and 0.7 (safe)
        float turbulenceMin = 0.4f;
        float turbulenceMax = 0.7f;
        float oscillationSpeed = Mathf.Lerp(10f, 40f, t);
        liquidVolume.turbulence1 = Mathf.Lerp(turbulenceMin, turbulenceMax, Mathf.PingPong(Time.time * oscillationSpeed, 1f));
        liquidVolume.foamTurbulence = Mathf.Lerp(turbulenceMin, turbulenceMax, Mathf.PingPong(Time.time * oscillationSpeed, 1f));

        liquidVolume.requireBubblesUpdate = true;
    }




}
