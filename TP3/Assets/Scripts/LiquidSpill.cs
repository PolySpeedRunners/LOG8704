using UnityEngine;
using LiquidVolumeFX;
using System.Collections;

public class LiquidSpill : MonoBehaviour
{
    public GameObject spill;

    LiquidVolume lv;
    GameObject[] dropTemplates;
    const int DROP_TEMPLATES_COUNT = 10;

    void Start()
    {
        lv = GetComponent<LiquidVolume>();

        dropTemplates = new GameObject[DROP_TEMPLATES_COUNT];
        for (int k = 0; k < DROP_TEMPLATES_COUNT; k++)
        {
            GameObject oneSpill = Instantiate(spill);
            oneSpill.transform.localScale *= Random.Range(0.45f, 0.65f);
            oneSpill.GetComponent<Renderer>().material.color =
                Color.Lerp(lv.liquidColor1, lv.liquidColor2, Random.value);

            // Make sure clones have a collider
            if (oneSpill.GetComponent<Collider>() == null)
            {
                SphereCollider col = oneSpill.AddComponent<SphereCollider>();
                col.radius = 0.03f;  // tweak as needed
                col.isTrigger = true;  // collision-based detection
            }

            // Make sure they have a Rigidbody (if not already)
            if (oneSpill.GetComponent<Rigidbody>() == null)
            {
                oneSpill.AddComponent<Rigidbody>();
            }

            // Tag droplets
            oneSpill.tag = "Liquid";

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

            for (int k = 0; k < drops; k++)
            {
                GameObject oneSpill = Instantiate(dropTemplates[Random.Range(0, DROP_TEMPLATES_COUNT)]);
                oneSpill.SetActive(true);

                Rigidbody rb = oneSpill.GetComponent<Rigidbody>();
                rb.position = spillPos + Random.insideUnitSphere * 0.01f;

                rb.AddForce(new Vector3(
                    Random.value - 0.5f,
                    Random.value * 0.1f - 0.2f,
                    Random.value - 0.5f
                ));

                StartCoroutine(DestroySpill(oneSpill));
            }

            lv.level -= spillAmount / 10f + 0.001f;
        }
    }

    IEnumerator DestroySpill(GameObject spill)
    {
        yield return new WaitForSeconds(1f);
        Destroy(spill);
    }
}
