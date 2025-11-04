using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageRecognitionHandler : MonoBehaviour
{
    [SerializeField] private GameObject modelPrefab;
    private ARTrackedImageManager _trackedImageManager;

    void Awake() => _trackedImageManager = GetComponent<ARTrackedImageManager>();

    void OnEnable() => _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    void OnDisable() => _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            GameObject obj = Instantiate(modelPrefab, trackedImage.transform);
        }

        foreach (var trackedImage in args.updated)
        {
            trackedImage.transform.localScale = Vector3.one * trackedImage.size.x;
        }
    }
}