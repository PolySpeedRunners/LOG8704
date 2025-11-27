using UnityEngine;
using System.Collections.Generic;

public class QuestObjectiveManager : MonoBehaviour
{
    public static QuestObjectiveManager Instance;

    [SerializeField]
    private float ratioToWin = 0.3f;

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
        if (reactionDatabase == null)
        {
            reactionDatabase = Resources.Load<ReactionDatabase>("CustomAssets/Receipes/ReactionDatabase");
            if (reactionDatabase == null)
                Debug.LogError("LFDEBUG - ReactionDatabase NOT FOUND in Resources!");
        }
        objectives.Clear();
    }

    private void Start()
    {
        if (reactionDatabase == null)
        {
            Debug.Log($"reaction database was null");
        }
        foreach (ReactionRecipe reaction in reactionDatabase.reactions)
        {
            ReactionGoal goal = new ReactionGoal
            {
                recipe = reaction,
                completed = false
            };

            objectives.Add(goal);
            if (reaction == null)
                Debug.LogError($"LFDEBUG - Reaction #{reaction} is NULL!");
        }

        BillboardUI.Instance.Refresh();
    }

    private bool ValidateReaction(List<ChemicalRatio> products, ChemicalType contentType)
    {
        foreach (ChemicalRatio reaction in products)
        {
            if (reaction.type == contentType && reaction.ratio >= ratioToWin)
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
    }
}
