using UnityEngine;
using System.Collections.Generic;

public class QuestObjectiveManager : MonoBehaviour
{
    public static QuestObjectiveManager Instance;

    [SerializeField]
    private float ratioToWin = 0.5f;

    // [System.Serializable]
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
            ReactionGoal goal = new ReactionGoal();
            goal.recipe = reaction;
            goal.completed = false;

            objectives.Add(goal);
        }
    }

    private bool validateReaction(List<ChemicalRatio> products, ChemicalType contentType)
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
            foreach (KeyValuePair<ChemicalType, float> content in c.contents)
            {
                if (!obj.completed && validateReaction(obj.recipe.products, content.Key))
                {
                    obj.completed = true;

                    // Debug.Log($"Objectif complété : {obj.chemA} + {obj.chemB}");

                    // Mise à jour du billboard
                    if (BillboardUI.Instance != null)
                        BillboardUI.Instance.Refresh();

                    return;
                }
            }
        }
        foreach (var kvp in c.contents)
        {
            Debug.Log($"[DEBUG] contient : {kvp.Key} = {kvp.Value}");
        }
        Debug.Log("Aucun objectif validé par cet objet.");
    }


}
