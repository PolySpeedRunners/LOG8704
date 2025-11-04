using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageRecognitionHandler : MonoBehaviour
{
    [SerializeField] private GameObject modelPrefab;
    private ARTrackedImageManager _manager;

    void Awake()
    {
        _manager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _manager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _manager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            // Spawn prefab at detected image
            Instantiate(modelPrefab, trackedImage.transform.position, trackedImage.transform.rotation, trackedImage.transform);
        }
    }
}
