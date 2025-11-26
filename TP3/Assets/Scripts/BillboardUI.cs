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
        StringBuilder sb = new StringBuilder();

        foreach (var obj in QuestObjectiveManager.Instance.objectives)
        {
            sb.Append(obj.chemA + " + " + obj.chemB + " -> " + obj.result);

            if (obj.completed)
                sb.Append("   <color=green>POG</color>");
            else
                sb.Append("   <color=red>TODO</color>");

            sb.Append("\n");
        }

        objectiveText.text = sb.ToString();
    }
}
