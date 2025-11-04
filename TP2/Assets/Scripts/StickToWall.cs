using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class StickToWall : MonoBehaviour
{
    ARRaycastManager raycastManager;
    Camera cam;
    Rigidbody rb;
    [SerializeField]
    bool isGlued = false;

    void Start()
    {
        raycastManager = FindAnyObjectByType<ARRaycastManager>();
        cam = FindAnyObjectByType<Camera>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isGlued) return; 

        if (raycastManager == null) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        // Ray depuis la caméra, vers l'avant
        if (raycastManager.Raycast(new Ray(cam.transform.position, transform.position - cam.transform.position), hits, TrackableType.Planes))
        {
            var plane = hits[0];

            Vector3 normal = plane.pose.up;

            bool isWall = Mathf.Abs(normal.y) < 0.3f;

            // Si c'est un mur, reste là
            if (isWall)
            {
                Vector3 wallNormal = normal;
                Quaternion look = Quaternion.LookRotation(-wallNormal, Vector3.up);
                transform.rotation = look * Quaternion.Euler(0, 90, 0);

                rb.useGravity = false;
                rb.isKinematic = true;
                isGlued = true;
            }
        }
    }
}

