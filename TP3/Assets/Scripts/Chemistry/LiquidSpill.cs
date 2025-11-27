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
    [Header("Spill Offset")]
    public Vector3 localOffset = new Vector3(0, 0, 0.02f); // editable in Inspector
    private ChemicalContainer container;

    void Start()
    {
        lv = GetComponent<LiquidVolume>();
        container = GetComponentInParent<ChemicalContainer>();

        dropTemplates = new GameObject[DROP_TEMPLATES_COUNT];

        for (int k = 0; k < DROP_TEMPLATES_COUNT; k++)
        {
            GameObject oneSpill = Instantiate(spill);

            oneSpill.transform.localScale *= Random.Range(0.45f, 0.65f);
            oneSpill.GetComponent<Renderer>().material.color =
                Color.Lerp(lv.liquidColor1, lv.liquidColor2, Random.value);

            if (oneSpill.GetComponent<Collider>() == null)
            {
                SphereCollider col = oneSpill.AddComponent<SphereCollider>();
                col.radius = 0.03f;
                col.isTrigger = true;
            }

            if (oneSpill.GetComponent<Rigidbody>() == null)
                oneSpill.AddComponent<Rigidbody>();

            oneSpill.tag = "Liquid";

            oneSpill.AddComponent<DropletMetadata>();
            oneSpill.SetActive(false);
            dropTemplates[k] = oneSpill;
        }
    }

    void FixedUpdate()
    {
        if (lv.GetSpillPoint(out Vector3 spillPos, out float spillAmount))
        {
            const int drops = 2;

            float mlSpilled = spillAmount * container.maxVolume * 0.1f;
            if (mlSpilled < 0.1f) mlSpilled = 0.1f;

            List<ChemicalAmount> removed = container.TakeProportional(mlSpilled);

            for (int k = 0; k < drops; k++)
            {
                GameObject oneSpill = Instantiate(dropTemplates[Random.Range(0, DROP_TEMPLATES_COUNT)]);
                oneSpill.SetActive(true);

                Rigidbody rb = oneSpill.GetComponent<Rigidbody>();
                DropletMetadata meta = oneSpill.GetComponent<DropletMetadata>();

                // Copy chemicals
                meta.contents = new List<ChemicalAmount>(removed);

                // OFFSET THAT ROTATES WITH OBJECT
                Vector3 worldOffset = transform.rotation * localOffset;

                // Final position
                rb.position = spillPos + worldOffset + Random.insideUnitSphere * 0.01f;

                rb.AddForce(new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f));

                StartCoroutine(DestroySpill(oneSpill));
            }
        }
    }

    IEnumerator DestroySpill(GameObject spill)
    {
        yield return new WaitForSeconds(1f);
        Destroy(spill);
    }
}
