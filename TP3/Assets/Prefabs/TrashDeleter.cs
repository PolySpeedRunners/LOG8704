using UnityEngine;

public class TrashDeleter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Transform root = rb.transform;

        ChemicalContainer container = root.GetComponent<ChemicalContainer>();
        if (container == null)
        {
            return;
        }

        Destroy(root.gameObject);
    }
}