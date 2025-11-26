using System.Collections.Generic;

public enum ChemicalType
{
    Water,
    Acid,
    Base,
    Solvent,
    Fuel
}

public static class ReactionTable
{
    private static readonly Dictionary<(ChemicalType, ChemicalType), ChemicalType> reactions =
        new()
        {
            { (ChemicalType.Acid, ChemicalType.Base), ChemicalType.Water },
        };

    public static bool TryGetProduct(ChemicalType r1, ChemicalType r2, out ChemicalType result)
    {
        return reactions.TryGetValue((r1, r2), out result);
    }
}