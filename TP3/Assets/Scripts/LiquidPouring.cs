using UnityEngine;
using LiquidVolumeFX;

public class LiquidPouring : MonoBehaviour
{

    public float fillSpeed = 0.01f;
    public float sinkFactor = 0.1f;
    LiquidVolume lv;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lv = transform.parent.GetComponent<LiquidVolume>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Liquid"))
        {
            if (lv.level < 1f)
                lv.level += fillSpeed;
                Destroy(other.gameObject);
        }
    }
}
