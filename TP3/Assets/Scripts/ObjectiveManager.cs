using UnityEngine;
using System.Collections.Generic;

public class QuestObjectiveManager : MonoBehaviour
{
    public static QuestObjectiveManager Instance;

    [SerializeField]
    private float ratioToWin = 0.5f;

    [System.Serializable]
    public class ReactionGoal
    {
        public ReactionRecipe recipe;
        public bool completed;
    }

    // [SerializeField]
    private ReactionDatabase reactionDatabase;
    [SerializeField]
    public List<ReactionGoal> objectives = new List<ReactionGoal>();

    void Awake()
    {
        Instance = this;
        Debug.Log("LFDEBUG - objectivemanager woke");
        // auto-load at runtime
        if (reactionDatabase == null)
        {
            reactionDatabase = Resources.Load<ReactionDatabase>("CustomAssets/Receipes/ReactionDatabase");
            if (reactionDatabase == null)
                Debug.LogError("LFDEBUG - ReactionDatabase NOT FOUND in Resources!");
            else
                Debug.Log("LFDEBUG - lets go bruthers");
        }
        objectives.Clear();


        ReactionRecipe DilutionPVA = Resources.Load<ReactionRecipe>("CustomAssets/Receipes/Dilution-PVA");
        ReactionRecipe PVAReaction1 = Resources.Load<ReactionRecipe>("CustomAssets/Receipes/PVA-Reaction1");

        if (DilutionPVA != null)
        {
            objectives.Add(new ReactionGoal { recipe = DilutionPVA, completed = false });
            Debug.Log("LFDEBUG - Added objective: " + DilutionPVA.name);
        }
        else
        {
            Debug.LogError("LFDEBUG - Dilution-PVA recipe not found in Resources!");
        }

        if (PVAReaction1 != null)
        {
            objectives.Add(new ReactionGoal { recipe = PVAReaction1, completed = false });
            Debug.Log("LFDEBUG - Added objective: " + PVAReaction1.name);
        }
        else
        {
            Debug.LogError("LFDEBUG - PVA-Reaction1 recipe not found in Resources!");
        }

        // --- Refresh Billboard UI ---
        if (BillboardUI.Instance != null)
        {
            BillboardUI.Instance.Refresh();
            Debug.Log("LFDEBUG - BillboardUI refreshed");
        }
        else
        {
            Debug.LogWarning("LFDEBUG - BillboardUI.Instance is null, cannot refresh");
        }
    }

    private void Start()
    {
        Debug.Log($"LFDEBUG - HEYYYYYYYYYYYYYYYYYYYYYYY IM ALIIIIIIIIIIIIIIIIIIIIIIIIIIVE");

        //if (reactionDatabase == null)
        //{
        //    Debug.Log($"LFDEBUG - MORE NULLLLLLLSSSS of the reaction database");
        //}

        //foreach (ReactionRecipe reaction in reactionDatabase.reactions)
        //{
        //    ReactionGoal goal = new ReactionGoal
        //    {
        //        recipe = reaction,
        //        completed = false
        //    };

        //    objectives.Add(goal);
        //    if (reaction == null)
        //        Debug.LogError($"LFDEBUG - Reaction #{reaction} is NULL!");
        //    else
        //        Debug.Log($"LFDEBUG - Reaction #{reaction}: {reaction.name}");
        //}

        Debug.Log($"LFDEBUG - HI IM CALLING REFRESH");
        BillboardUI.Instance.Refresh();
    }

    private bool ValidateReaction(List<ChemicalRatio> products, ChemicalType contentType)
    {
        foreach (ChemicalRatio reaction in products)
        {
            if (reaction.type == contentType)
                return true;
        }
        return false;
    }

    public void CheckContainer(ChemicalContainer c)
    {
        foreach (var obj in objectives)
        {
            if (obj.completed) continue;

            foreach (ChemicalAmount content in c.contents)
            {
                if (ValidateReaction(obj.recipe.products, content.type))
                {
                    obj.completed = true;

                    // Optional debug
                    Debug.Log($"LFDEBUG - Objective completed for {content.type}");

                    if (BillboardUI.Instance != null)
                        BillboardUI.Instance.Refresh();

                    return; // Stop after the first valid reaction
                }
            }
        }

        // Debugging: show what chemicals are in the container
        foreach (var content in c.contents)
        {
            Debug.Log($"LFDEBUG - contains: {content.type} = {content.volume}");
        }
        Debug.Log("LFDEBUG - we reached the end of checkcontainer");
    }
}
