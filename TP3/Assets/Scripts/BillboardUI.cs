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
            Debug.LogWarning("BillboardUI: TMP_Text reference is missing!");
            return;
        }

        StringBuilder sb = new StringBuilder();

        Debug.Log("billboard - " + QuestObjectiveManager.Instance.objectives.Count);

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
}
