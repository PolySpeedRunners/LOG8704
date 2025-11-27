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
    public float turbulence1Step = 0.7123f;
    public float turbulence1Start = 0f;
    public float turbulence1End = 1f;

    [Header("Foam Settings")]
    public float foamDensityStart = 0.0f;
    public float foamDensityEnd = 0.022f;

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
    [SerializeField] private float temperatureDecayRate = 1f;
    [SerializeField] private float reactionInterval = 0.1f;

    [Header("Chemical Data")]
    [SerializeField] private ChemicalDatabase chemicalData;
    [SerializeField] private ReactionDatabase reactionData;
    [SerializeField] public List<ChemicalAmount> contents = new();

    [Header("Sparkling Settings")]
    [SerializeField] private SparklingSettings sparkling;

    private Color currentColor;
    private float reactionTimer = 0f;

    public float totalVolume => contents.Sum(c => c.volume);
    public float currentTemperature = 25f;
    public bool hasHeatSource = false;
    public bool isVacuum = false;
    public float heatIncreaseRate = 4f;


    private void Awake()
    {
        currentColor = ComputeBlendedColor();
        if (liquidVolume != null)
        {
            liquidVolume.sparklingAmount = sparkling.baseAmount;
            liquidVolume.sparklingIntensity = sparkling.baseIntensity;
            liquidVolume.speed = sparkling.baseSpeed;

            liquidVolume.turbulence1 = sparkling.turbulence1Start;
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
        if (hasHeatSource)
        {
            currentTemperature += heatIncreaseRate * Time.deltaTime;
        }
        else
        {
            currentTemperature = Mathf.MoveTowards(
                currentTemperature,
                ambientTemperature,
                temperatureDecayRate * Time.deltaTime
            );
        }

        UpdateVisual();
        UpdateSparkling();

        reactionTimer += Time.deltaTime;
        if (reactionTimer >= reactionInterval)
        {
            TryReact();
            reactionTimer = 0f;
        }
    }

    private Color ComputeBlendedColor()
    {
        if (contents.Count == 0) return Color.black;

        float total = totalVolume;
        var dominant = contents.OrderByDescending(c => c.volume).First();
        Color dominantColor = chemicalData.Get(dominant.type).color;

        Color secondaryBlend = Color.black;
        float secondaryWeight = 0f;

        foreach (var c in contents)
        {
            if (c.type == dominant.type) continue;
            float ratio = c.volume / total;
            if (ratio < 0.03f) continue;

            float weight = Mathf.Pow(ratio, 1.6f);
            secondaryBlend += chemicalData.Get(c.type).color * weight;
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
            liquidVolume.liquidColor1 = new Color(1f, 1f, 1f, 0f);
            liquidVolume.liquidColor2 = new Color(1f, 1f, 1f, 0f);
            return;
        }

        Color targetColor = ComputeBlendedColor();
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 3f);

        float brightness = Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(totalVolume / maxVolume));
        Color finalColor = new Color(
            currentColor.r * brightness,
            currentColor.g * brightness,
            currentColor.b * brightness,
            1f
        );

        liquidVolume.liquidColor1 = finalColor;
        liquidVolume.liquidColor2 = finalColor;
    }

    private ChemicalAmount GetChemical(ChemicalType type)
    {
        return contents.FirstOrDefault(c => c.type == type);
    }

    // ---------------- Updated Methods ----------------

    public void AddLiquid(List<ChemicalAmount> addedContents)
    {
        if (addedContents == null || addedContents.Count == 0) return;

        float incomingVolume = addedContents.Sum(c => c.volume);
        float availableSpace = maxVolume - totalVolume;
        if (availableSpace <= 0f) return;

        float scale = incomingVolume > availableSpace ? availableSpace / incomingVolume : 1f;

        foreach (var chem in addedContents)
        {
            var existing = GetChemical(chem.type);
            if (existing == null)
                contents.Add(new ChemicalAmount { type = chem.type, volume = chem.volume * scale });
            else
                existing.volume += chem.volume * scale;
        }

        TryReact();
        UpdateVisual();
    }

    public List<ChemicalAmount> TakeProportional(float amount)
    {
        float total = totalVolume;
        if (total <= 0f) return new List<ChemicalAmount>();

        List<ChemicalAmount> removed = new List<ChemicalAmount>();

        foreach (var c in contents.ToList())
        {
            float ratio = c.volume / total;
            float take = ratio * amount;
            c.volume -= take;

            removed.Add(new ChemicalAmount { type = c.type, volume = take });

            if (c.volume <= 0.0001f)
                contents.Remove(c);
        }

        return removed;
    }

    private float currentReactionProgress = 0f;
    private float totalReactionVolume = 0f;
    private ReactionRecipe currentReaction = null;

    private void TryReact()
    {
        if (reactionData == null || contents.Count == 0) return;
        if (!reactionData.TryFindReaction(this, out ReactionRecipe r)) return;

        // If we started a new reaction, initialize totalReactionVolume
        if (currentReaction != r)
        {
            currentReaction = r;
            currentReactionProgress = 0f;

            // --- Identify limiting reactant ONCE ---
            totalReactionVolume = float.MaxValue;
            foreach (var req in r.reactants)
            {
                var chem = GetChemical(req.type);
                if (chem == null) { totalReactionVolume = 0f; break; }

                float possible = chem.volume / req.ratio;
                if (possible < totalReactionVolume)
                    totalReactionVolume = possible;
            }
            if (totalReactionVolume <= 0f)
            {
                currentReaction = null;
                return;
            }
        }

        // --- Temperature factor ---
        float minTemp = r.targetTemperature - r.temperatureMargin;
        float tempFactor = currentTemperature < minTemp
            ? Mathf.Clamp01(currentTemperature / minTemp)
            : 1f;

        // --- Reaction progress this frame ---
        float fractionThisFrame = (Time.deltaTime / r.reactionDuration) * tempFactor;
        currentReactionProgress += fractionThisFrame;
        if (currentReactionProgress > 1f) fractionThisFrame -= currentReactionProgress - 1f; // clamp final fraction

        float reactionVolume = totalReactionVolume * fractionThisFrame;
        if (reactionVolume <= 0f) return;

        // --- Consume reactants proportionally ---
        foreach (var req in r.reactants)
        {
            var chem = GetChemical(req.type);
            if (chem == null) continue;

            chem.volume -= req.ratio * reactionVolume;
            if (chem.volume <= 0.0001f)
                contents.Remove(chem);
        }

        // --- Produce products proportionally ---
        foreach (var prod in r.products)
        {
            var chem = GetChemical(prod.type);
            float produceAmount = prod.ratio * reactionVolume;

            if (chem == null)
                contents.Add(new ChemicalAmount { type = prod.type, volume = produceAmount });
            else
                chem.volume += produceAmount;
        }

        contents.RemoveAll(c => c.volume < 0f);

        // --- Reaction finished ---
        if (currentReactionProgress >= 1f)
            currentReaction = null;
    }

    private void UpdateSparkling()
    {
        if (liquidVolume == null) return;

        float threshold = sparkling.temperatureStart;
        float maxTemp = sparkling.temperatureEnd;

        if (currentTemperature < threshold)
        {
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

        float t = Mathf.InverseLerp(threshold, maxTemp, currentTemperature);
        t = Mathf.Clamp01(t);

        liquidVolume.sparklingAmount = Mathf.Clamp01(Mathf.Lerp(sparkling.baseAmount, sparkling.maxAmount, t));
        liquidVolume.sparklingIntensity = Mathf.Clamp01(Mathf.Lerp(sparkling.baseIntensity, sparkling.maxIntensity, t));
        liquidVolume.speed = Mathf.Clamp01(Mathf.Lerp(sparkling.baseSpeed, sparkling.maxSpeed, t));

        liquidVolume.foamDensity = Mathf.Clamp01(Mathf.Lerp(Mathf.Max(0f, sparkling.foamDensityStart),
                                                           Mathf.Max(0f, sparkling.foamDensityEnd), t));
        liquidVolume.foamThickness = Mathf.Clamp01(Mathf.Lerp(sparkling.foamThicknessStart, sparkling.foamThicknessEnd, t));
        liquidVolume.foamTurbulence = Mathf.Clamp01(Mathf.Lerp(sparkling.foamTurbulence, 0.7f, t));

        float turbulenceMin = 0.4f;
        float turbulenceMax = 0.7f;
        float oscillationSpeed = Mathf.Lerp(10f, 40f, t);
        liquidVolume.turbulence1 = Mathf.Lerp(turbulenceMin, turbulenceMax, Mathf.PingPong(Time.time * oscillationSpeed, 1f));
        liquidVolume.foamTurbulence = Mathf.Lerp(turbulenceMin, turbulenceMax, Mathf.PingPong(Time.time * oscillationSpeed, 1f));

        liquidVolume.requireBubblesUpdate = true;
    }
}
