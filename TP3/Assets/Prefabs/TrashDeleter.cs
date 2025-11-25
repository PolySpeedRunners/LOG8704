using UnityEngine;

public class TrashDeleter : MonoBehaviour
{
    [Tooltip("Only objects with this tag will be deleted.")]
    public string requiredTag = "Trashable";  

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a Rigidbody (needed for physics interactions)
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // Only proceed if the object has the correct tag
        if (!other.CompareTag(requiredTag)) return;

        // Destroy the root object (important for VR objects with nested hierarchy)
        GameObject root = rb.transform.root.gameObject;
        Destroy(root);
    }
}