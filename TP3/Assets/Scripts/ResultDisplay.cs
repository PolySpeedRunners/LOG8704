using UnityEngine;
using TMPro;
using System.Text;

public class ResultDisplay : MonoBehaviour
{
    public static ResultDisplay Instance;

    [Header("UI Reference")]
    public TMP_Text resultText;
    void Awake()
    {
        Instance = this;
    }

    public void SetObjective(string text)
    {
        if (resultText != null)
            resultText.text = text;
    }

    public void CheckUpdate(ChemicalContainer c)
    {
        StringBuilder sb = new StringBuilder();

        if (c != null)
        {
            foreach (var kvp in c.contents)
            {
                ChemicalType type = kvp.Key;
                float volume = kvp.Value;

                sb.AppendLine($"{type}: {volume:0.0} mL");
            }
        }

        resultText.text = sb.ToString();
    }
}
