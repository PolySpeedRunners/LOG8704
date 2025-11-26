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

    // Cette méthode sera appelée par l'InteractableUnityEventWrapper -> OnSelectEnter
    public void OnGrab()
    {
        UnityEngine.Debug.Log("ADJIDOUBAYYEEEEE");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.useGravity = true;

        if (!hasBeenGrabbedOnce)
        {
            GameObject clone = Instantiate(gameObject, originalPosition, originalRotation);

            // Réinitialiser le clone pour qu'il ne soit pas "grabbed"
            Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
            if (cloneRb != null)
                cloneRb.useGravity = false;

            Respawner r = clone.GetComponent<Respawner>();
            r.hasBeenGrabbedOnce = false;

            hasBeenGrabbedOnce = true;
        }
    }
}



//using UnityEngine;

//public class Respawner : MonoBehaviour
//{
//    public bool hasBeenGrabbedOnce = false;
//    private Vector3 originalPosition;
//    private Quaternion originalRotation;

//    private void Start()
//    {
//        originalPosition = transform.position;
//        originalRotation = transform.rotation;
//    }

//    public void OnGrab()
//    {
//        Rigidbody rb = GetComponent<Rigidbody>();
//        if (rb != null)
//            rb.useGravity = true;

//        if (!hasBeenGrabbedOnce)
//        {
//            GameObject clone = Instantiate(gameObject, originalPosition, originalRotation);

//            // Attention, le clone ne doit pas être en état "grabbed" TODO

//            hasBeenGrabbedOnce = true;
//        }
//    }
//}