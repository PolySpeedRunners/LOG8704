using UnityEngine;
using System.Collections.Generic;

public class MixingSystemManager : MonoBehaviour
{
    public static MixingSystemManager Instance;

    [System.Serializable]
    public class MixRecipe
    {
        public string chemicalA;
        public string chemicalB;
        public string result;
    }

    public List<MixRecipe> recipes = new List<MixRecipe>();

    private void Awake()
    {
        Instance = this;
    }

    public string GetMixtureResult(string chemA, string chemB)
    {
        foreach (var recipe in recipes)
        {
            if ((recipe.chemicalA == chemA && recipe.chemicalB == chemB) ||
                (recipe.chemicalA == chemB && recipe.chemicalB == chemA))
            {
                return recipe.result;
            }
        }

        return "Unknown"; // fallback or null
    }
}
