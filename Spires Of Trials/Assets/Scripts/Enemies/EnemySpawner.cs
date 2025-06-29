using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using FMODUnity;


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
    [SerializeField] private GameObject miniBossPrefab;       // Reference to the MiniBoss prefab
    [SerializeField] private GameObject frogBossPrfab;
    [SerializeField] private GameObject finalBossPrfab;
    [SerializeField] private float spawnChance = 0.5f;        // Probability for each location to spawn an enemy

    BattleSceneController battleController;

    #endregion

    #region UI & Round Management

    [Header("Round System")]
    [SerializeField] private TextMeshProUGUI roundText; // UI element displaying the current round number
    private int roundCounter = RoundManager.ROUND_NUMBER; // Tracks the current round, starting at Round 1
    private int maxRounds = 5;


    [Header("Data-Driven Waves")]
    [Tooltip("Drag in all your SpawnWave assets, sorted by roundNumber.")]
    public List<SpawnWave> spawnWaves;

    private Dictionary<int, SpawnWave> _wavesByRound;


    [Header("Fallback Settings")]
    [Tooltip("Used if you forgot to author a SpawnWave for a given round.")]
    [SerializeField]
    private List<EnemySpawnEntry> defaultEntries;



    private GameObject currentMiniBoss;

    bool goneToGarden;
    bool goneToSanctum;

    bool isTrans = false;

    #endregion

    #region Spawn Tracking

    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List to keep track of active spawned enemies
    private bool isSpawning = false; // Ensures only one spawn process runs at a time
    private bool bossSpawned = false; // Prevents the MiniBoss from spawning more than once
    private bool frogBossSpawned = false;
    private bool finalBossSpawned = false;

    #endregion

    #region MiniBoss Settings

    [Header("MiniBoss Settings")]
    private int miniBossSpawnNumber = 25; // The round number when the MiniBoss will appear
    private int frogBossSpawnNumber = 50;
    private int finalBossSpawnNumber = 75;

    private ENEMY enemy;

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
       
        // Build a quick lookup table
        _wavesByRound = spawnWaves
            .ToDictionary(w => w.roundNumber, w => w);
       
    }

    /// <summary>
    /// Called when the script starts.
    /// Ensures that spawn locations, enemy prefabs, and the MiniBoss prefab are assigned.
    /// Initializes the round UI and starts the enemy spawn cycle.
    /// </summary>
    private void Start()
    {

        battleController = GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleSceneController>();


        // Validate that all necessary spawn points and enemy prefabs are assigned
        if (spawnLocations.Count == 0 || enemyPrefabs.Count == 0 || miniBossPrefab == null)
        {
            Debug.LogError("Spawn locations, enemy prefabs, or MiniBoss prefab not assigned!");
            return; // Prevent execution if any critical assignment is missing
        }

        UpdateRoundUI(); // Initialize the round counter text display
        StartCoroutine(CheckAndSpawnEnemies()); // Begin enemy spawning routine


        if (SceneManager.GetActiveScene().name == "Ruins")
        {
            goneToGarden = false;
            goneToSanctum = false;
        }

        if (SceneManager.GetActiveScene().name == "Garden")
        {
            goneToGarden = true;
            goneToSanctum = false;
        }

        if (SceneManager.GetActiveScene().name == "Sanctum")
        {
            goneToSanctum = true;
        }
    }

    /// <summary>
    /// Called once per frame.
    /// Checks if the MiniBoss has been defeated, and if so, loads the Win Screen.
    /// </summary>
    private void Update()
    {
        if (roundCounter == 5)
        {
            roundCounter = 0;
            RoundManager.ROUND_NUMBER = roundCounter;
            battleController.EndBattle();
            AudioManager.instance.SetMusic(MusicEnum.Title);

        }



    }

    #endregion

    #region Spawning

    /// <summary>
    /// Continuously checks if all enemies are destroyed before starting a new spawn cycle.
    /// Triggers healing, updates the round UI, and decides whether to spawn regular enemies or the MiniBoss.
    /// </summary>
    private IEnumerator CheckAndSpawnEnemies()
    {
        while (roundCounter < maxRounds)
        {
            yield return new WaitUntil(AllEnemiesDestroyed);

            roundCounter++;
            UpdateRoundUI();

            if (_wavesByRound.TryGetValue(roundCounter, out var wave))
            {
                if (wave.waveType != WaveType.Regular)
                {
                    // boss spawn
                    yield return SpawnBoss(
                      wave.bossPrefab,
                      wave.bossOffset,
                      wave.waveType
                    );
                }
                else
                {
                    // regular enemies
                    yield return SpawnRegularEnemies(wave.regularSpawns);
                }
            }
            else
            {
                // fallback if you forgot to author a wave
                yield return SpawnRegularEnemies(defaultEntries);
            }
        }
    }


    private IEnumerator SpawnBoss(GameObject prefab, Vector3 offset, WaveType type)
    {
        isSpawning = true;
        AudioManager.instance.SetMusic(MusicEnum.RuinsBoss);
        var loc = spawnLocations[1];
        var boss = Instantiate(prefab, loc.transform.position, Quaternion.identity);
        EventManager.Instance.TriggerEvent("InitializeAttackSprites", (
                                          leftFlash,
                                          centerFlash,
                                          rightFlash,
                                          leftShield,
                                          centerShield,
                                          rightShield
                                                      ));
        boss.transform.SetParent(loc.transform, true);
        spawnedEnemies.Add(boss);
        isSpawning = false;
        yield return null;
    }

    /// <summary>
    /// Spawns regular enemies at random locations based on spawn chances.
    /// Ensures at least one enemy is spawned per round.
    /// </summary>
    private IEnumerator SpawnRegularEnemies(List<EnemySpawnEntry> entries)
    {
        isSpawning = true;

        // ensure at least one spawn...
        bool atLeastOne = false;
        while (!atLeastOne)
        {
            foreach (var entry in entries)
            {
                foreach (int pos in entry.validPositions)
                {
                    if (Random.value <= entry.spawnChance)
                    {
                        var loc = spawnLocations[pos];
                        var go = Instantiate(entry.prefab, loc.transform.position, Quaternion.identity);
                        EventManager.Instance.TriggerEvent("InitializeAttackSprites", (
                                         leftFlash,
                                         centerFlash,
                                         rightFlash,
                                         leftShield,
                                         centerShield,
                                         rightShield
                                                     ));
                        go.transform.SetParent(loc.transform, true);
                        
                        spawnedEnemies.Add(go);
                        atLeastOne = true;
                    }
                }
            }
            if (!atLeastOne)
                yield return null;  // try again next frame
        }

        isSpawning = false;
    }


    public void ForceSpawnEnemy()
    {
        if (_wavesByRound.TryGetValue(roundCounter, out var wave)
            && wave.waveType == WaveType.Regular)
        {
            StartCoroutine(SpawnRegularEnemies(wave.regularSpawns));
        }
        else
        {
            StartCoroutine(SpawnRegularEnemies(defaultEntries));
        }
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
            if(SceneManager.GetActiveScene().name == "Battle")
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
            else if (SceneManager.GetActiveScene().name == "Garden")
            {
                if (enemyTag == "VineSerpant" && (positionIndex == 0 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Thornbrute" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Wendingo" && (positionIndex == 1)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
            }
            else if (SceneManager.GetActiveScene().name == "Sanctum" && !finalBossSpawned)
            {
                if (enemyTag == "Devil" && (positionIndex == 0 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Vampire" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Cleric" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
            }
            else if(SceneManager.GetActiveScene().name == "Sanctum" && finalBossSpawned)
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
                else if (enemyTag == "VineSerpant" && (positionIndex == 0 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Thornbrute" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Wendingo" && (positionIndex == 1)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Devil" && (positionIndex == 0 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Vampire" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2)) // Slimes spawn only on the sides
                {
                    possibleEnemies.Add(enemy);
                }
                else if (enemyTag == "Cleric" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2)) // Slimes spawn only on the sides
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
    private bool AllEnemiesDestroyed()
    {
        // Remove any null references from the spawnedEnemies list (enemies that have been destroyed)
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        // If no enemies remain, return true
        return spawnedEnemies.Count == 0;
    }

    /// <summary>
    /// Updates the UI to reflect the current round number.
    /// Ensures the UI element is assigned before attempting to update it.
    /// </summary>
    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = roundCounter.ToString(); // Display the current round number
        }
        else
        {
            Debug.LogWarning("Round UI Text is not assigned! Ensure roundText is set in the Inspector.");
        }
    }

    #endregion

}
