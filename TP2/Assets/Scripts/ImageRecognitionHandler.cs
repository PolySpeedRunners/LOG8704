using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ImageRecognitionHandler : MonoBehaviour
{
    [SerializeField] private GameObject modelPrefab;
    private ARTrackedImageManager _trackedImageManager;

    // Keep track of spawned objects for each tracked image
    private Dictionary<string, GameObject> _spawnedPrefabs = new Dictionary<string, GameObject>();

    void Awake() => _trackedImageManager = GetComponent<ARTrackedImageManager>();

    void OnEnable() => _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    void OnDisable() => _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Handle newly added images
        foreach (var trackedImage in args.added)
        {
            SpawnPrefab(trackedImage);
        }

        // Handle updated images
        foreach (var trackedImage in args.updated)
        {
            UpdatePrefab(trackedImage);
        }

        // Optionally handle removed images
        foreach (var trackedImage in args.removed)
        {
            if (_spawnedPrefabs.TryGetValue(trackedImage.referenceImage.name, out var obj))
            {
                Destroy(obj);
                _spawnedPrefabs.Remove(trackedImage.referenceImage.name);
            }
        }
    }

    private void SpawnPrefab(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking && !_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject obj = Instantiate(modelPrefab, trackedImage.transform.position, trackedImage.transform.rotation);

            // Make it stay on the marker if it has Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            obj.transform.SetParent(trackedImage.transform);
            _spawnedPrefabs.Add(trackedImage.referenceImage.name, obj);
        }
    }

    private void UpdatePrefab(ARTrackedImage trackedImage)
    {
        if (_spawnedPrefabs.TryGetValue(trackedImage.referenceImage.name, out var obj))
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                obj.SetActive(true);
                obj.transform.position = trackedImage.transform.position;
                obj.transform.rotation = trackedImage.transform.rotation;
                obj.transform.localScale = Vector3.one * trackedImage.size.x;
            }
            else
            {
                obj.SetActive(false); // hide if tracking lost
            }
        }
    }
}
