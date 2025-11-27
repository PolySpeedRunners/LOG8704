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
    [SerializeField]
    public List<ChemicalDefinition> ChemicalDefinitions; //  { get; private set; }

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
        if (ReactionDB == null)
            Debug.LogError("LFDEBUG - ReactionDatabase in Resources!");

        if (ReactionsRecepies.Count == 0)
            Debug.LogError("LFDEBUG - ReactionRecipe SINGLETON NOT FOUND in Resources!");

        if (ChemicalDB == null)
            Debug.LogError("LFDEBUG - ChemicalDatabase SINGLETON NOT FOUND in Resources!");

        if (ChemicalDefinitions.Count == 0)
            Debug.LogError("LFDEBUG - ChemicalDefinition SINGLETON NOT FOUND in Resources!");

        //ReactionDB = Resources.Load<ReactionDatabase>("CustomAssets/Receipes/Reaction Database");
        //ReactionDBtest = Resources.Load<ReactionRecipe>("CustomAssets/Receipes/PVA-Reaction1");
        ChemicalDB = Resources.Load<ChemicalDatabase>("CustomAssets/Chemicals/ChemicalDatabase");
        Debug.Log("LFDEBUG - hallo c'est pour le cehmical db" + ChemicalDB == null);
        //ChemicalDefinition = Resources.Load<ChemicalDefinition>("CustomAssets/Chemicals/PVA");
    }
}
