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
        public List<ChemicalType> reactants;
        public ChemicalType result;
        public bool completed;
    }

    public List<ReactionGoal> objectives = new List<ReactionGoal>();

    void Awake()
    {
        Instance = this;
    }

    //private bool verifyContent(ReactionGoal reaction, ChemicalType product)
    //{
    //    if (products.Contains(reaction.result))
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    public void CheckContainer(ChemicalContainer c)
    {
        foreach (var obj in objectives)
        {
            foreach (KeyValuePair<ChemicalType, float> content in c.contents)
            {
                if (!obj.completed && obj.result == content.Key)//verifyContent(obj, content.Key)) // this should be replaced by a function callin chemicalcontainer maybe?
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
