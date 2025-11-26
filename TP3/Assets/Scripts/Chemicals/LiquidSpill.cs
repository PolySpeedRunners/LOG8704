using UnityEngine;
using LiquidVolumeFX;
using System.Collections;
using System.Collections.Generic;

public class LiquidSpill : MonoBehaviour
{
    public GameObject spill;

    private LiquidVolume lv;
    private GameObject[] dropTemplates;
    private const int DROP_TEMPLATES_COUNT = 10;

    private ChemicalContainer container;

    void Start()
    {
        lv = GetComponent<LiquidVolume>();
        container = GetComponentInParent<ChemicalContainer>();

        dropTemplates = new GameObject[DROP_TEMPLATES_COUNT];

        for (int k = 0; k < DROP_TEMPLATES_COUNT; k++)
        {
            GameObject oneSpill = Instantiate(spill);

            // Size + color
            oneSpill.transform.localScale *= Random.Range(0.45f, 0.65f);
            oneSpill.GetComponent<Renderer>().material.color =
                Color.Lerp(lv.liquidColor1, lv.liquidColor2, Random.value);

            // Collider
            if (oneSpill.GetComponent<Collider>() == null)
            {
                SphereCollider col = oneSpill.AddComponent<SphereCollider>();
                col.radius = 0.03f;
                col.isTrigger = true;
            }

            // Rigidbody
            if (oneSpill.GetComponent<Rigidbody>() == null)
                oneSpill.AddComponent<Rigidbody>();

            // Tag droplets
            oneSpill.tag = "Liquid";

            // Metadata component
            oneSpill.AddComponent<DropletMetadata>();

            oneSpill.SetActive(false);
            dropTemplates[k] = oneSpill;
        }
    }

    void FixedUpdate()
    {
        Vector3 spillPos;
        float spillAmount;

        if (lv.GetSpillPoint(out spillPos, out spillAmount))
        {
            const int drops = 2;

            // Convert spillAmount (0–1) to ml based on container's maxVolume
            float mlSpilled = spillAmount * container.maxVolume * 0.1f;
            if (mlSpilled < 0.1f) mlSpilled = 0.1f;

            // Take proportional chemical amounts from container
            Dictionary<ChemicalType, float> removed = container.TakeProportional(mlSpilled);

            for (int k = 0; k < drops; k++)
            {
                GameObject oneSpill =
                    Instantiate(dropTemplates[Random.Range(0, DROP_TEMPLATES_COUNT)]);
                oneSpill.SetActive(true);

                Rigidbody rb = oneSpill.GetComponent<Rigidbody>();
                DropletMetadata meta = oneSpill.GetComponent<DropletMetadata>();

                // Assign the chemical contents to the droplet
                meta.contents = new Dictionary<ChemicalType, float>(removed);

                // Position + small random offset
                rb.position = spillPos + Random.insideUnitSphere * 0.01f;

                // Randomized initial force
                rb.AddForce(new Vector3(
                    Random.value - 0.5f,
                    Random.value * 0.1f - 0.2f,
                    Random.value - 0.5f
                ));

                StartCoroutine(DestroySpill(oneSpill));
            }

            // NO manual lv.level change here — container handles visual
        }
    }

    IEnumerator DestroySpill(GameObject spill)
    {
        yield return new WaitForSeconds(1f);
        Destroy(spill);
    }
}
