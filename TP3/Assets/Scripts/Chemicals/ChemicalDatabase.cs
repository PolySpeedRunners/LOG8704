using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Chemical Database", menuName = "Chemistry/Chemical Database")]
public class ChemicalDatabase : ScriptableObject
{
    public ChemicalDefinition[] definitions;

    public ChemicalDefinition Get(ChemicalType type)
    {
        return definitions.FirstOrDefault(d => d.type == type);
    }
}