using UnityEngine;


public class DropletMetadata : MonoBehaviour
{
    [SerializeField]
    private ChemicalType chemical_type = ChemicalType.Water;
    public ChemicalType Chemical
    {
        get { return chemical_type; }
        set { chemical_type = value; }
    }
}
