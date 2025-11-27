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

    [SerializeField]
    private ReactionDatabase database;
    public List<ReactionGoal> objectives = new List<ReactionGoal>();

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        objectives.Clear();

        foreach (ReactionRecipe reaction in database.reactions)
        {
            ReactionGoal goal = new ReactionGoal
            {
                recipe = reaction,
                completed = false
            };

            objectives.Add(goal);
        }

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
                    Debug.Log($"Objective completed for {content.type}");

                    if (BillboardUI.Instance != null)
                        BillboardUI.Instance.Refresh();

                    return; // Stop after the first valid reaction
                }
            }
        }

        // Debugging: show what chemicals are in the container
        foreach (var content in c.contents)
        {
            Debug.Log($"[DEBUG] contains: {content.type} = {content.volume}");
        }
        Debug.Log("No objective was validated by this container.");
    }
}
