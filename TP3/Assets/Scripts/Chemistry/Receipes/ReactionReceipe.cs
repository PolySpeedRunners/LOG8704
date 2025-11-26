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

    [Tooltip("Time in seconds for full conversion if ratios allow")]
    [Range(0.01f, 10f)]
    public float reactionDuration = 1f; // new: full reaction duration

    // Optional prerequisites
    public bool requireHeatSource = false;
    public bool requireVacuum = false;

    [Tooltip("Temperature margin around target temperature (±degrees Celsius)")]
    public float temperatureMargin = 5f; 
    public float targetTemperature = 25f; // ideal temp for this reaction
}
