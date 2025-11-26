using UnityEngine;

public class Heater : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ChemicalContainer container = other.GetComponentInParent<ChemicalContainer>();
        if (container == null)
            return;
        //container.liquidVolume.finalAlphaMultiplier = 0.5f;
        
    }
}
