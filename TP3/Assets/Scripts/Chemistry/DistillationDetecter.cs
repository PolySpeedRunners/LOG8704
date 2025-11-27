using UnityEngine;

public class DistillationDetector : MonoBehaviour
{
    public DistillationSystem distillationSystem;

    private void OnTriggerEnter(Collider other)
    {
        var container = other.GetComponentInParent<ChemicalContainer>();
        if (container != null)
        {
            distillationSystem.TryPlaceContainer(container);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var container = other.GetComponentInParent<ChemicalContainer>();
        if (container != null)
        {
            distillationSystem.StopDraining(container);
        }
    }
}
