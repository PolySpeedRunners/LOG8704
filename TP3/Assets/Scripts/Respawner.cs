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
        UnityEngine.Debug.Log("adj has been grabbed");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (!hasBeenGrabbedOnce)
        {
            GameObject clone = Instantiate(gameObject, originalPosition, originalRotation);

            // Réinitialiser le clone pour qu'il ne soit pas "grabbed"
            Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
            if (cloneRb != null)
            {
                cloneRb.useGravity = false;
                cloneRb.isKinematic = true;
            }

            Respawner r = clone.GetComponent<Respawner>();
            r.hasBeenGrabbedOnce = false;

            hasBeenGrabbedOnce = true;
        }
    }
}
