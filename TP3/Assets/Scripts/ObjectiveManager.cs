using UnityEngine;
using System.Collections.Generic;

public class QuestObjectiveManager : MonoBehaviour
{
    public static QuestObjectiveManager Instance;

    [System.Serializable]
    public class ReactionGoal
    {
        public string chemA;
        public string chemB;
        public string result;
        public bool completed;
    }

    public List<ReactionGoal> objectives = new List<ReactionGoal>();

    void Awake()
    {
        Instance = this;
    }

    public void NotifyReaction(string product)
    {
        foreach (var obj in objectives)
        {
            if (obj.result == product && !obj.completed)
            {
                obj.completed = true;
                Debug.Log("Objective completed: " + product);
            }
        }

        BillboardUI.Instance.Refresh();
    }

    public void CheckContainer(ChemicalContainer c)
    {
        foreach (var obj in objectives)
        {
            if (!obj.completed && c.contents.ContainsKey(ChemicalType.Acid))
               // c.contents.ContainsKey(obj.chemA) &&
               // c.contents.ContainsKey(obj.chemB))
            {
                obj.completed = true;

                Debug.Log($"Objectif complété : {obj.chemA} + {obj.chemB}");

                // Mise à jour du billboard
                if (BillboardUI.Instance != null)
                    BillboardUI.Instance.Refresh();

                return;
            }
        }
        foreach (var kvp in c.contents)
        {
            Debug.Log($"[DEBUG] contient : {kvp.Key} = {kvp.Value}");
        }
        Debug.Log("Aucun objectif validé par cet objet.");
    }
}
