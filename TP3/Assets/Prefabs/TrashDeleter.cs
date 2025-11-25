using UnityEngine;

public class TrashDeleter : MonoBehaviour
{
    public string requiredTag = "Trashable";

    private void OnTriggerEnter(Collider other)
    {

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Transform root = rb.transform;

        if (!root.CompareTag(requiredTag)) return;

        Debug.Log($"[TrashDeleter] DELETING: {root.name}");
        Destroy(root.gameObject);
    }
}