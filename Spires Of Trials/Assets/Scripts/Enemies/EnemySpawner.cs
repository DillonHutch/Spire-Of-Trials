using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using FMODUnity;


[System.Serializable]
public class EnemyCombatSettings
{
    public string enemyTag;   // must match the GameObject.tag
    public int minRounds;   // inclusive
    public int maxRounds;   // inclusive


    [Header("Allowed Spawn Positions (0=Left,1=Center,2=Right)")]
    public bool spawnLeft = true;
    public bool spawnCenter = true;
    public bool spawnRight = true;


    [Header("Spawn Count Settings")]
    public bool hasMultipleSpawns = false;


}



/// <summary>
/// is enemy spawner
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region Fields

    #region Spawn Configuration

    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> spawnLocations; // List of possible enemy spawn locations
    [SerializeField] private List<GameObject> enemyPrefabs;   // List of enemy prefabs to spawn
    //[SerializeField] private float spawnChance = 0.5f;        // Probability for each location to spawn an enemy


    BattleSceneController battleController;

    #endregion

    #region UI & Round Management

    [Header("Round System")]
    [SerializeField] private TextMeshProUGUI roundText; // UI element displaying the current round number
    private int roundCounter = RoundManager.ROUND_NUMBER; // Tracks the current round, starting at Round 1
    private int maxRounds = 5;


    [Header("Combat Settings per Enemy")]
    [Tooltip("Configure how many rounds each enemy type should generate.")]
    public List<EnemyCombatSettings> combatSettings;

    // lookup table
    private Dictionary<string, EnemyCombatSettings> _settingsByTag;

    // override the fixed maxRounds
    private int _dynamicMaxRounds;



 

    private Dictionary<string, EnemyCombatSettings> settingsByTag;
    private List<GameObject> spawnedEnemies = new List<GameObject>();


    private string _combatEnemyTag;


    private GameObject currentMiniBoss;

    bool goneToGarden;
    bool goneToSanctum;

   

    #endregion

    #region Flash & Shield Effects

    [Header("Flash Effects")]
    [SerializeField] private SpriteRenderer leftFlash;   // Flash effect for left attack warning
    [SerializeField] private SpriteRenderer centerFlash; // Flash effect for center attack warning
    [SerializeField] private SpriteRenderer rightFlash;  // Flash effect for right attack warning

    [Header("Shield References")]
    [SerializeField] private Transform leftShield;   // Reference to the left shield
    [SerializeField] private Transform centerShield; // Reference to the center shield
    [SerializeField] private Transform rightShield;  // Reference to the right shield

    #endregion

    #region Miscellaneous

    private SpriteRenderer spriteRenderer; // Reference to the spawner's sprite renderer (if needed)


    /// <summary>
    /// If non‐null, the spawner will immediately spawn this prefab as the battle’s
    /// first enemy, then clear the reference.
    /// </summary>
    public static GameObject NextBattleEnemyPrefab;

    #endregion

    #region UnityMethods


    private void OnEnable()
    {
        
    }


    private void OnDisable()
    {
        
    }


    private void Awake()
    {
        // build quick tag → settings map
        settingsByTag = new Dictionary<string, EnemyCombatSettings>();
        foreach (var s in combatSettings)
            settingsByTag[s.enemyTag] = s;


        
    }


    /// <summary>
    /// Called when the script starts.
    /// Ensures that spawn locations, enemy prefabs, and the MiniBoss prefab are assigned.
    /// Initializes the round UI and starts the enemy spawn cycle.
    /// </summary>
    private void Start()
    {
        
        if(battleController == null)
        {
            battleController = GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleSceneController>();
        }
        else
        {
            Debug.Log("WTF");
        }
        

        // if we came here with a pending tag, immediately kick off combat
        if (!string.IsNullOrEmpty(BattleContext.PendingEnemyTag))
        {
            StartCombat(BattleContext.PendingEnemyTag);
            BattleContext.PendingEnemyTag = null;
        }

        // now wire up UI & controller as before
        roundText = GetComponentInChildren<TextMeshProUGUI>();
        
    }



    #endregion



    /// <summary>
    /// Called by the player trigger when combat should start.
    /// Picks a random round count for that enemy type, then begins spawning.
    /// </summary>
    public void StartCombat(string enemyTag)
    {
       
        _combatEnemyTag = enemyTag;

        // pull settings (or default)
        if (!settingsByTag.TryGetValue(enemyTag, out var cfg))
        {
            Debug.LogWarning($"No combat settings for '{enemyTag}'");
        }

        // determine wave count...
        maxRounds = cfg != null
            ? Random.Range(cfg.minRounds, cfg.maxRounds + 1)
            : 3;

        roundCounter = 0;
        spawnedEnemies.Clear();
        UpdateRoundUI();
        StartCoroutine(CombatRoutine());
    }


    private IEnumerator CombatRoutine()
    {
        // For each wave 1 through maxRounds:
        while (roundCounter < maxRounds)
        {
            // start the next wave
            roundCounter++;
            UpdateRoundUI();

            // spawn that wave
            yield return SpawnRandomEnemies();

            // wait until the player kills every enemy in this wave
            yield return new WaitUntil(AllEnemiesCleared);
        }

        // all waves are done—exit combat
        battleController.EndBattle();
    }



    private IEnumerator SpawnRandomEnemies()
    {
        EnemyCombatSettings cfg = settingsByTag[_combatEnemyTag];

        // build list of valid slots
        List<int> allowedIndices = new List<int>();
        if (cfg.spawnLeft) allowedIndices.Add(0);
        if (cfg.spawnCenter) allowedIndices.Add(1);
        if (cfg.spawnRight) allowedIndices.Add(2);

        int maxPossible = Mathf.Min(3, allowedIndices.Count);
        int toSpawn;

        if (cfg.hasMultipleSpawns)
        {
            if (BattleContext.PendingEnemySlotCount > 0)
                toSpawn = Mathf.Clamp(BattleContext.PendingEnemySlotCount, 1, maxPossible);
            else
                toSpawn = Random.Range(1, maxPossible + 1);
        }
        else
        {
            toSpawn = 1;
        }

        // pick N random distinct slots
        List<int> shuffledSlots = allowedIndices
            .OrderBy(i => Random.value)
            .ToList();
        List<int> chosenSlots = shuffledSlots
            .Take(toSpawn)
            .ToList();

        foreach (int slotIndex in chosenSlots)
        {
            if (slotIndex >= spawnLocations.Count)
                continue;

            // choose prefab by tag
            List<GameObject> candidates
                = enemyPrefabs
                    .Where(p => p.tag == _combatEnemyTag)
                    .ToList();
            GameObject prefabToSpawn
                = (candidates.Count > 0)
                    ? candidates[Random.Range(0, candidates.Count)]
                    : enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

            Transform spawnPoint = spawnLocations[slotIndex].transform;
            GameObject enemyInstance = Instantiate(
                prefabToSpawn,
                spawnPoint.position,
                Quaternion.identity,
                spawnPoint
            );

            // optional center‑scale tweak
            if (slotIndex == 1)
                enemyInstance.transform.localScale = Vector3.one * 0.8f;

            // initialize attack sprites
            EventManager.Instance.TriggerEvent(
                "InitializeAttackSprites",
                (leftFlash, centerFlash, rightFlash,
                 leftShield, centerShield, rightShield)
            );

            // per‑type adjustments
            switch (enemyInstance.tag)
            {
                case "Slime":
                    AdjustSlimePosition(enemyInstance, slotIndex);
                    break;
                    // … other cases …
            }

            spawnedEnemies.Add(enemyInstance);
        }

        // clear for next wave
        BattleContext.PendingEnemySlotCount = 0;

        yield return null;
    }





    /// <summary>
    /// Selects an enemy type based on its valid spawn positions.
    /// </summary>
    /// <param name="positionIndex">The index of the spawn position.</param>
    /// <returns>The selected enemy prefab or null if no valid enemy is available.</returns>
    private GameObject SelectEnemyForPosition(int positionIndex)
    {
        List<GameObject> possibleEnemies = new List<GameObject>();

        foreach (GameObject enemy in enemyPrefabs)
        {
            string enemyTag = enemy.tag; // Get the enemy's tag

            //Debug.LogError(goneToGarden);

            // If rounds are 1-5, only Skeletons spawn
            if (SceneManager.GetActiveScene().name == "Battle")
            {
                // Define valid positions for each enemy type
                if (enemyTag == "Skeleton" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2))
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Goblin" && positionIndex == 1) // Goblins spawn only in the center
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Slime" && (positionIndex == 0 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }

            }
        }

            // Return a random enemy from the list or null if no valid enemies exist
            return possibleEnemies.Count > 0 ? possibleEnemies[Random.Range(0, possibleEnemies.Count)] : null;
    }



    private void AdjustSerpantPosition(GameObject spawnedEnemy, int spawnIndex)
    {

        Vector3 spawnLocation = spawnedEnemy.transform.position;
        spawnedEnemy.transform.position = spawnLocation;

        if (spawnIndex == 0) // If spawning in the leftmost position
        {
            spriteRenderer = spawnedEnemy.GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = true; // Flip the sprite

            spawnLocation.x += 1.5f; // Adjust slime position further
            spawnedEnemy.transform.position = spawnLocation;

            if (spawnedEnemy.transform.childCount > 0)
            {
                Transform childIcon = spawnedEnemy.transform.GetChild(0);
                Transform childPartOrgin = spawnedEnemy.transform.GetChild(1);

                // Adjust the positions of child elements
                childIcon.localPosition = new Vector3(-2.24f, 0.62f, 0);
                childPartOrgin.localPosition = new Vector3(-3.5f, 2.5f, 0);
            }

            // Adjust the Canvas position for Slime enemies
            Canvas snakeCanavs = spawnedEnemy.GetComponentInChildren<Canvas>();
            if (snakeCanavs != null)
            {
                RectTransform canvasTransform = snakeCanavs.GetComponent<RectTransform>();
                if (canvasTransform != null)
                {
                    canvasTransform.localPosition = new Vector3(1918f, 1080.036f, 0);
                }
            }
        }
        else
        {
            spawnLocation.x -= 1.5f; // Offset slime spawn position

            //if (spawnedEnemy.transform.childCount > 0)
            //{
            //    Transform childIcon = spawnedEnemy.transform.GetChild(0);
            //    Transform childPartOrgin = spawnedEnemy.transform.GetChild(1);

            //    // Adjust the positions of child elements
            //    childIcon.localPosition = new Vector3(0f, 0.62f, 0);
            //    //childPartOrgin.localPosition = new Vector3(2.28f, 2.5f, 0);
            //}

            //// Adjust the Canvas position for Slime enemies
            //Canvas slimeCanvas = spawnedEnemy.GetComponentInChildren<Canvas>();
            //if (slimeCanvas != null)
            //{
            //    RectTransform canvasTransform = slimeCanvas.GetComponent<RectTransform>();
            //    if (canvasTransform != null)
            //    {
            //        canvasTransform.localPosition = new Vector3(1918f, canvasTransform.localPosition.y, 0);
            //    }
            //}

            spawnedEnemy.transform.position = spawnLocation;
        }
    }



    private void AdjustClericPosition(GameObject spawnedEnemy, int spawnIndex)
    {

        Vector3 spawnLocation = spawnedEnemy.transform.position;
        spawnedEnemy.transform.position = spawnLocation;




        spawnLocation.x -= .4f; // Adjust slime position further
        spawnedEnemy.transform.position = spawnLocation;

        // Adjust the Canvas position for Slime enemies
        Canvas snakeCanavs = spawnedEnemy.GetComponentInChildren<Canvas>();
        if (snakeCanavs != null)
        {
            RectTransform canvasTransform = snakeCanavs.GetComponent<RectTransform>();
            if (canvasTransform != null)
            {
                canvasTransform.localPosition = new Vector3(1920.5f, 1081f, 0);
            }
        }


    }


    /// <summary>
    /// Adjusts the position, scaling, and child elements of Slime enemies.
    /// Ensures proper placement and mirroring when needed.
    /// </summary>
    /// <param name="spawnedEnemy">The instantiated Slime enemy.</param>
    /// <param name="spawnIndex">The spawn position index.</param>
    private void AdjustDevilPosition(GameObject spawnedEnemy, int spawnIndex)
    {
        Vector3 spawnLocation = spawnedEnemy.transform.position;
        spawnLocation.x -= 3.5f; // Offset slime spawn position

        spawnedEnemy.transform.position = spawnLocation;

        if (spawnIndex == 0) // If spawning in the leftmost position
        {
            spriteRenderer = spawnedEnemy.GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = true; // Flip the sprite

            spawnLocation.x += 7f; // Adjust slime position further
            spawnedEnemy.transform.position = spawnLocation;

            if (spawnedEnemy.transform.childCount > 0)
            {
                Transform childIcon = spawnedEnemy.transform.GetChild(0);
                Transform childPartOrgin = spawnedEnemy.transform.GetChild(1);

                // Adjust the positions of child elements
                childIcon.localPosition = new Vector3(-3.5f, 0.62f, 0);
                childPartOrgin.localPosition = new Vector3(-3.5f, 2.5f, 0);
            }

            Canvas snakeCanavs = spawnedEnemy.GetComponentInChildren<Canvas>();
            if (snakeCanavs != null)
            {
                RectTransform canvasTransform = snakeCanavs.GetComponent<RectTransform>();
                if (canvasTransform != null)
                {
                    canvasTransform.localPosition = new Vector3(1912.85f, 1080.036f, 0);
                }
            }
        }
    }




    /// <summary>
    /// Adjusts the position, scaling, and child elements of Slime enemies.
    /// Ensures proper placement and mirroring when needed.
    /// </summary>
    /// <param name="spawnedEnemy">The instantiated Slime enemy.</param>
    /// <param name="spawnIndex">The spawn position index.</param>
    private void AdjustSlimePosition(GameObject spawnedEnemy, int spawnIndex)
    {
        Vector3 spawnLocation = spawnedEnemy.transform.position;
        spawnLocation.x -= 3.5f; // Offset slime spawn position

        spawnedEnemy.transform.position = spawnLocation;

        if (spawnIndex == 0) // If spawning in the leftmost position
        {
            spriteRenderer = spawnedEnemy.GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = true; // Flip the sprite

            spawnLocation.x += 7f; // Adjust slime position further
            spawnedEnemy.transform.position = spawnLocation;

            if (spawnedEnemy.transform.childCount > 0)
            {
                Transform childIcon = spawnedEnemy.transform.GetChild(0);
                Transform childPartOrgin = spawnedEnemy.transform.GetChild(1);

                // Adjust the positions of child elements
                childIcon.localPosition = new Vector3(-3.5f, 0.62f, 0);
                childPartOrgin.localPosition = new Vector3(-3.5f, 2.5f, 0);
            }

            // Adjust the Canvas position for Slime enemies
            Canvas slimeCanvas = spawnedEnemy.GetComponentInChildren<Canvas>();
            if (slimeCanvas != null)
            {
                RectTransform canvasTransform = slimeCanvas.GetComponent<RectTransform>();
                if (canvasTransform != null)
                {
                    canvasTransform.localPosition = new Vector3(1913.19f, canvasTransform.localPosition.y, 0);
                }
            }
        }
    }

    #endregion

    #region Round Support

    /// <summary>
    /// Checks if all enemies have been destroyed by cleaning up null references.
    /// Returns true if there are no remaining enemies in the list.
    /// </summary>
    /// <returns>True if all enemies are destroyed, otherwise false.</returns>
    private bool AllEnemiesCleared()
    {
        spawnedEnemies.RemoveAll(e => e == null);
        return spawnedEnemies.Count == 0;
    }

    /// <summary>
    /// Updates the UI to reflect the current round number.
    /// Ensures the UI element is assigned before attempting to update it.
    /// </summary>
    private void UpdateRoundUI()
    {
        if (roundText != null)
            roundText.text = $"Round: {roundCounter}/{maxRounds}";
    }



    #endregion

}
