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
        if (resultText == null) return;

        StringBuilder sb = new StringBuilder();

        if (c != null)
        {
            foreach (var chem in c.contents)
            {
                ChemicalType type = chem.type;
                float volume = chem.volume;

                sb.AppendLine($"{type}: {volume:0.0} mL");
            }
        }

        resultText.text = sb.ToString();
    }
}
