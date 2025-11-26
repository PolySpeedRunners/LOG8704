using UnityEngine;

public class Heater : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var container = other.GetComponentInParent<ChemicalContainer>();
        if (container != null)
            container.hasHeatSource = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var container = other.GetComponentInParent<ChemicalContainer>();
        if (container != null)
            container.hasHeatSource = false;
    }
}
