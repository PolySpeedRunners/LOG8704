using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;

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
            var recipe = obj.recipe;

            if (recipe == null)
            {
                sb.Append("<color=red>Invalid recipe</color>\n");
                continue;
            }

            // Couleur d'état
            string status = obj.completed
                ? "<color=green>[ Fait ]</color>"
                : "<color=red>[ À Faire ]</color>";

            // Récupération des ingrédients (reactifs)
            string reagents = "—";
            if (recipe.reactants != null && recipe.reactants.Count > 0)
                reagents = string.Join(" + ", recipe.reactants.Select(r => r.type.ToString()));

            // Récupération des produits
            string products = "—";
            if (recipe.products != null && recipe.products.Count > 0)
                products = string.Join(" + ", recipe.products.Select(p => p.type.ToString()));

            // Température (si applicable)
            string temperature = "";
            if (recipe.targetTemperature > 0f)
                temperature = $" + {recipe.targetTemperature}°C";

            sb.Append($"{status} {reagents}{temperature} = {products}\n");
        }

        objectiveText.text = sb.ToString();
    }
}
