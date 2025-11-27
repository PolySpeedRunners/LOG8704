using UnityEngine;
using System.Collections.Generic;

public class ChemistryDataRegistry : MonoBehaviour
{
    public static ChemistryDataRegistry Instance { get; private set; }

    [SerializeField]
    public ReactionDatabase ReactionDB; //{ get; private set; }
    [SerializeField]
    public List<ReactionRecipe> ReactionsRecepies; //{ get; private set; }
    [SerializeField]
    public ChemicalDatabase ChemicalDB;//{ get; private set; }
    //[SerializeField]
    //public List<ChemicalDefinition> ChemicalDefinitions; //  { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadDatabases();
    }

    private void LoadDatabases()
    {
        Debug.Log("LFDEBUG - [ChemistryDataRegistry] Loading ScriptableObject databases...");

        //ReactionDB = Resources.Load<ReactionDatabase>("CustomAssets/Receipes/Reaction Database");
        //ReactionDBtest = Resources.Load<ReactionRecipe>("CustomAssets/Receipes/PVA-Reaction1");
        //ChemicalDB = Resources.Load<ChemicalDatabase>("CustomAssets/Chemicals/ChemicalDatabase");
        //ChemicalDefinition = Resources.Load<ChemicalDefinition>("CustomAssets/Chemicals/PVA");

        Debug.Log("LFDEBUG - attempting ReactionDatabase in Resources!");
        if (ReactionDB == null)
            Debug.LogError("LFDEBUG - ReactionDatabasein Resources!");

        Debug.Log("LFDEBUG - attempting ReactionRecipein Resources!");
        if (ReactionsRecepies == null)
            Debug.LogError("LFDEBUG - ReactionRecipe SINGLETON NOT FOUND in Resources!");

        Debug.Log("LFDEBUG - attempting ChemicalDatabase SINGLETON NOT FOUND in Resources!");
        if (ChemicalDB == null)
            Debug.LogError("LFDEBUG - ChemicalDatabase SINGLETON NOT FOUND in Resources!");

        //Debug.Log("LFDEBUG - attempting ChemicalDefinition SINGLETON NOT FOUND in Resources!");
        //if (ChemicalDefinitions == null)
        //    Debug.LogError("LFDEBUG - ChemicalDefinition SINGLETON NOT FOUND in Resources!");
    }
}
