using UnityEngine;
using TMPro;
using System.Text;

public class BillboardUI : MonoBehaviour
{
    public static BillboardUI Instance;

    [Header("UI Reference")]
    public TMP_Text objectiveText;
    void Awake()
    {
        Instance = this;
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }
    public void Refresh()
    {
        if (objectiveText == null)
        {
            Debug.LogWarning("LFDEBUG - BillboardUI: TMP_Text reference is missing!");
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (var obj in QuestObjectiveManager.Instance.objectives)
        {
            if (obj.recipe == null)
            {
                sb.Append("<color=red>Invalid recipe</color>\n");
                continue;
            }

            string recipeName = obj.recipe.name != null ? obj.recipe.name : "Reaction";

            if (obj.completed)
                sb.Append($"<color=green>[ Fait ]</color> {recipeName}");
            else
                sb.Append($"<color=red>[ À Faire ]</color> {recipeName}");

            sb.Append("\n");
        }
        objectiveText.text = sb.ToString();
    }
    //public void Refresh()
    //{
    //    if (objectiveText == null)
    //    {
    //        Debug.LogWarning("LFDEBUG - BillboardUI: TMP_Text reference is missing!");
    //        return;
    //    }

    //    StringBuilder sb = new StringBuilder();

    //    foreach (var obj in QuestObjectiveManager.Instance.objectives)
    //    {
    //        var recipe = obj.recipe;

    //        if (recipe == null)
    //        {
    //            sb.Append("<color=red>Invalid recipe</color>\n");
    //            continue;
    //        }

    //        // Couleur d'état
    //        string status = obj.completed
    //            ? "<color=green>[ Fait ]</color>"
    //            : "<color=red>[ À Faire ]</color>";

    //        // Récupération des ingrédients (reactifs)
    //        string reagents = "—";
    //        if (recipe.reagents != null && recipe.reagents.Count > 0)
    //            reagents = string.Join(" + ", recipe.reagents.Select(r => r.type.ToString()));

    //        // Récupération des produits
    //        string products = "—";
    //        if (recipe.products != null && recipe.products.Count > 0)
    //            products = string.Join(" + ", recipe.products.Select(p => p.type.ToString()));

    //        // Température (si applicable)
    //        string temperature = "";
    //        if (recipe.temperatureRequired > 0f)
    //            temperature = $" + {recipe.temperatureRequired}°C";

    //        sb.Append($"{status} {reagents}{temperature} = {products}\n");
    //    }

    //    objectiveText.text = sb.ToString();
    //}
}
