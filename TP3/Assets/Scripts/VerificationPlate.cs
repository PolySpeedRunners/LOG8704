using UnityEngine;

public class VerificationPlate : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ChemicalContainer container = other.GetComponentInParent<ChemicalContainer>();
        if (container == null)
            return;

        QuestObjectiveManager.Instance.CheckContainer(container);
        ResultDisplay.Instance.CheckUpdate(container);
    }
}
