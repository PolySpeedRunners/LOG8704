using UnityEngine;
using LiquidVolumeFX;
using System.Linq;

public class LiquidPouring : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Liquid")) return;

        DropletMetadata droplet = other.GetComponent<DropletMetadata>();
        if (droplet != null)
        {
            ChemicalContainer container = GetComponentInParent<ChemicalContainer>();
            if (container != null)
            {
                container.AddLiquid(droplet.contents);
            }
        }

        Destroy(other.gameObject);
    }
}
