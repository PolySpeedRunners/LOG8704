using UnityEngine;

public class Respawner : MonoBehaviour
{
    public bool hasBeenGrabbedOnce = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void OnGrab()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (!hasBeenGrabbedOnce)
        {
            hasBeenGrabbedOnce = true;

            GameObject clone = Instantiate(gameObject, originalPosition, originalRotation);
            Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
            if (cloneRb != null)
            {
                cloneRb.useGravity = false;
                cloneRb.isKinematic = true;
            }
            Respawner r = clone.GetComponent<Respawner>();
            r.hasBeenGrabbedOnce = false;
        }
    }
}
