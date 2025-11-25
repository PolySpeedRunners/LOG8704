using LiquidVolumeFX;
using UnityEngine;

public class ChemicalContainer : MonoBehaviour
{
    [Header("Liquid Properties")]
    public string chemicalName = "Water";
    public float maxVolume = 250f;
    public float currentVolume = 0f;

    [Range(0f, 1f)]
    public float fillPercentage;

    [Header("Liquid Visual Reference")]
    public LiquidVolume liquidVolume;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (liquidVolume != null)
        {
            fillPercentage = Mathf.Clamp01(liquidVolume.level);
            currentVolume = fillPercentage * maxVolume;
        }
    }

}
