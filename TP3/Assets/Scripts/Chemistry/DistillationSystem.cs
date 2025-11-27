using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LiquidVolumeFX;

public class DistillationSystem : MonoBehaviour
{
    [Header("Settings")]
    public float drainRate = 20f; // mL per second
    public GameObject emptyBeakerPrefab;

    [Header("Reject Positions (3 recommended)")]
    public List<Transform> rejectPositions = new List<Transform>();

    [Header("Output Positions (3 recommended)")]
    public List<Transform> outputPositions = new List<Transform>();

    [Header("Distillation Receiver")]
    [SerializeField] private LiquidVolume distillationSphere;
    [SerializeField] private float distillationMaxVolume = 200f;

    [Header("Dependencies")]
    public ReactionDatabase reactionDatabase;

    private ChemicalContainer activeContainer = null;
    private bool isDraining = false;
    private bool isProcessing = false;

    private List<ChemicalAmount> collectedContents = new();
    private float collectedTotal = 0f;

    private Coroutine drainRoutine;

    private int rejectIndex = 0;
    private int outputIndex = 0;


    // ---------------------------------------------------------
    // TRY TO PLACE BEAKER
    // ---------------------------------------------------------
    public bool TryPlaceContainer(ChemicalContainer container)
    {
        if (isDraining || isProcessing)
        {
            RejectBeaker(container);
            return false;
        }

        activeContainer = container;
        drainRoutine = StartCoroutine(DrainRoutine());
        return true;
    }

    // ---------------------------------------------------------
    // REJECT BEAKER (using next reject position)
    // ---------------------------------------------------------
    private void RejectBeaker(ChemicalContainer container)
    {
        if (rejectPositions.Count == 0)
        {
            Debug.LogWarning("No reject positions assigned!");
            return;
        }

        Transform pos = rejectPositions[rejectIndex];
        rejectIndex = (rejectIndex + 1) % rejectPositions.Count;

        container.transform.position = pos.position;
        container.transform.rotation = pos.rotation;
    }

    // ---------------------------------------------------------
    // DRAIN BEAKER
    // ---------------------------------------------------------
    private IEnumerator DrainRoutine()
    {
        isDraining = true;

        while (activeContainer != null && activeContainer.totalVolume > 0f)
        {
            float amountToTake = drainRate * Time.deltaTime;

            List<ChemicalAmount> removed = activeContainer.TakeProportional(amountToTake);

            foreach (var chem in removed)
            {
                collectedTotal += chem.volume;

                var existing = collectedContents.Find(c => c.type == chem.type);
                if (existing == null)
                    collectedContents.Add(new ChemicalAmount { type = chem.type, volume = chem.volume });
                else
                    existing.volume += chem.volume;
            }

            UpdateDistillationVisual();

            // 🔥 REQUIRED!
            yield return null;
        }

        isDraining = false;

        // Wait 2 seconds before checking reaction
        yield return new WaitForSeconds(2f);

        StartCoroutine(ProcessDistillation());
    }

    // ---------------------------------------------------------
    // PROCESS REACTION OUTPUT
    // ---------------------------------------------------------
    private IEnumerator ProcessDistillation()
    {
        isProcessing = true;

        if (!reactionDatabase.TryFindDistillationReaction(collectedContents, out ReactionRecipe recipe))
        {
            Debug.Log("No distillation reaction matched.");

            if (activeContainer != null)
                RejectBeaker(activeContainer);

            ResetSystem();
            yield break;
        }

        Debug.Log("✔ Reaction found! Separating products…");

        // Spawn beakers for each product in the reaction recipe
        foreach (var prod in recipe.products)
        {
            float volume = 0f;

            // Determine volume proportionally based on collected contents
            var matchingChemical = collectedContents.Find(c => c.type == prod.type);
            if (matchingChemical != null)
                volume = matchingChemical.volume; // use collected amount as base
            else
                volume = distillationMaxVolume * prod.ratio; // fallback: proportional to max

            SpawnOutputBeaker(new ChemicalAmount { type = prod.type, volume = volume });
            yield return new WaitForSeconds(0.5f);
        }

        ResetSystem();
    }


    // ---------------------------------------------------------
    // SPAWN OUTPUT BEAKER (using next output position)
    // ---------------------------------------------------------
    private void SpawnOutputBeaker(ChemicalAmount chem)
    {
        if (outputPositions.Count == 0)
        {
            Debug.LogWarning("No output positions assigned!");
            return;
        }

        Transform pos = outputPositions[outputIndex];
        outputIndex = (outputIndex + 1) % outputPositions.Count;

        GameObject newBeaker = Instantiate(emptyBeakerPrefab, pos.position, pos.rotation);
        ChemicalContainer container = newBeaker.GetComponent<ChemicalContainer>();

        container.contents.Clear();
        container.contents.Add(new ChemicalAmount { type = chem.type, volume = chem.volume });
    }
    public void StopDraining(ChemicalContainer container)
    {
        if (activeContainer != container)
            return;

        if (drainRoutine != null)
            StopCoroutine(drainRoutine);

        isDraining = false;
        isProcessing = false;

        // Optional: move to reject position
        RejectBeaker(container);

        ResetSystem();
    }

    private void UpdateDistillationVisual()
    {
        if (distillationSphere == null) return;

        float fill = Mathf.Clamp01(collectedTotal / distillationMaxVolume);
        distillationSphere.level = fill;

        // optional: fade color based on fullness
        Color sphereColor = Color.Lerp(
            new Color(1, 1, 1, 0), // empty
            new Color(0.5f, 0.7f, 1f, 1f), // full (light blue)
            fill
        );

        distillationSphere.liquidColor1 = sphereColor;
        distillationSphere.liquidColor2 = sphereColor;
    }


    // ---------------------------------------------------------
    // RESET
    // ---------------------------------------------------------
    private void ResetSystem()
    {
        activeContainer = null;
        collectedContents.Clear();
        collectedTotal = 0f;
        isProcessing = false;

        // Reset sphere visually
        UpdateDistillationVisual();
    }

}
