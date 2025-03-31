using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


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
    [SerializeField] private float spawnChance = 0.5f;        // Probability for each location to spawn an enemy

    #endregion

    #region UI & Round Management

    [Header("Round System")]
    [SerializeField] private TextMeshProUGUI roundText; // UI element displaying the current round number
    private int roundCounter = 0; // Tracks the current round, starting at Round 1

    #endregion

    #region Spawn Tracking

    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List to keep track of active spawned enemies
    private bool isSpawning = false; // Ensures only one spawn process runs at a time
    private bool bossSpawned = false; // Prevents the MiniBoss from spawning more than once

    #endregion

    #region MiniBoss Settings

    [Header("MiniBoss Settings")]
    [SerializeField] private int miniBossSpawnNumber = 10; // The round number when the MiniBoss will appear

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

    #endregion


    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script starts.
    /// Ensures that spawn locations, enemy prefabs, and the MiniBoss prefab are assigned.
    /// Initializes the round UI and starts the enemy spawn cycle.
    /// </summary>
    private void Start()
    {


        

        // Validate that all necessary spawn points and enemy prefabs are assigned
        if (spawnLocations.Count == 0 || enemyPrefabs.Count == 0 || miniBossPrefab == null)
        {
            Debug.LogError("Spawn locations, enemy prefabs, or MiniBoss prefab not assigned!");
            return; // Prevent execution if any critical assignment is missing
        }

        UpdateRoundUI(); // Initialize the round counter text display
        StartCoroutine(CheckAndSpawnEnemies()); // Begin enemy spawning routine
    }

    /// <summary>
    /// Called once per frame.
    /// Checks if the MiniBoss has been defeated, and if so, loads the Win Screen.
    /// </summary>
    private void Update()
    {
        // If the MiniBoss has already spawned and all enemies are defeated, trigger the win screen
        if (RoundManager.ROUND_NUMBER == 21 && AllEnemiesDestroyed())
        {
            Debug.Log("MiniBoss defeated. Loading WinScreen.");
            SceneManager.LoadScene("WinScreen"); // Load the Win Screen
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
        while (true)
        {
            if (AllEnemiesDestroyed() && !isSpawning)
            {
                EventManager.Instance.TriggerEvent("healDamageEvent", 1); // Heal the player after each round
                Debug.Log("All enemies destroyed. Starting new spawn cycle.");

                roundCounter++; // Increase round count
                UpdateRoundUI(); // Update UI

                // Adjust spawn chance dynamically based on round number
                if (roundCounter <= 5)
                    spawnChance = .05f; // Guarantee at least one spawn
                else if (roundCounter < 10)
                    spawnChance = 0.2f; // 20% chance
                else if (roundCounter < 15)
                    spawnChance = 0.3f; // 30% chance
                else if (roundCounter < miniBossSpawnNumber)
                    spawnChance = 0.5f; // 50% chance
                else if (roundCounter == miniBossSpawnNumber && !bossSpawned)
                {
                    bossSpawned = true;
                    yield return StartCoroutine(SpawnMiniBoss());
                    continue;
                }

                RoundManager.ROUND_NUMBER = roundCounter;

                if (!bossSpawned) { yield return StartCoroutine(SpawnEnemies()); }
                
            }

            yield return new WaitForSeconds(0.5f);
        }
    }


    /// <summary>
    /// Spawns the MiniBoss at a random spawn location and updates game states accordingly.
    /// </summary>
    private IEnumerator SpawnMiniBoss()
    {
        isSpawning = true;
        bossSpawned = true; // Ensure the boss spawns only once

        Debug.Log("Spawning MiniBoss!");

        // Change background music for the boss fight
        AudioManager.instance.SetMusic(MusicEnum.RuinsBoss);

        // Choose a random spawn location for the MiniBoss
        GameObject bossSpawnLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        // Instantiate the MiniBoss at the selected location
        GameObject miniBoss = Instantiate(miniBossPrefab, bossSpawnLocation.transform.position, Quaternion.identity);
        miniBoss.GetComponent<EnemyParent>()?.InitializeAttackSprites(leftFlash, centerFlash, rightFlash, leftShield, centerShield, rightShield);

        // Set the MiniBoss as a child of the spawn location
        miniBoss.transform.SetParent(bossSpawnLocation.transform, true);

        // Track the spawned MiniBoss
        spawnedEnemies.Add(miniBoss);

        Debug.Log($"MiniBoss spawned at {bossSpawnLocation.name}");

        isSpawning = false;
        yield return null;
    }

    /// <summary>
    /// Spawns regular enemies at random locations based on spawn chances.
    /// Ensures at least one enemy is spawned per round.
    /// </summary>
    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;
        bool atLeastOneSpawned = false;

        while (!atLeastOneSpawned)
        {
            for (int i = 0; i < spawnLocations.Count; i++)
            {
                float randomValue = Random.value; // Generate a random value (0 to 1)

                if (randomValue <= spawnChance) // If the random value is within the spawn chance, spawn an enemy
                {
                    GameObject enemyToSpawn = SelectEnemyForPosition(i);

                    if (enemyToSpawn != null)
                    {
                        atLeastOneSpawned = true; // Ensure at least one enemy is spawned

                        // Spawn the enemy and set its parent to the spawn location
                        GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnLocations[i].transform.position, Quaternion.identity);
                        spawnedEnemy.GetComponent<EnemyParent>()?.InitializeAttackSprites(leftFlash, centerFlash, rightFlash, leftShield, centerShield, rightShield);

                        // Special handling for Slime enemy position and adjustments
                        if (spawnedEnemy.tag == "Slime")
                        {
                            spawnedEnemy.transform.parent = spawnLocations[i].transform;
                            AdjustSlimePosition(spawnedEnemy, i);
                        }
                        else
                        {
                            spawnedEnemy.transform.parent = spawnLocations[i].transform;
                        }

                        // Scale enemies differently if they spawn in the middle position
                        if (i == 1) // Middle spawn location
                        {
                            spawnedEnemy.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                        }

                        // Track the spawned enemy
                        spawnedEnemies.Add(spawnedEnemy);
                        Debug.Log($"Spawned {enemyToSpawn.tag} at {spawnLocations[i].name}");
                    }
                    else
                    {
                        Debug.Log($"No valid enemy to spawn at {spawnLocations[i].name}");
                    }
                }
                else
                {
                    Debug.Log($"No enemy spawned at {spawnLocations[i].name}");
                }
            }

            if (!atLeastOneSpawned)
            {
                Debug.Log("No enemies spawned, retrying...");
                yield return null;
            }
        }

        Debug.Log("At least one enemy spawned. Spawning complete.");
        isSpawning = false;
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


            // If rounds are 1-5, only Skeletons spawn
            if (roundCounter <= 5)
            {
                // Define valid positions for each enemy type
                if (enemyTag == "Skeleton" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2))
                {
                    possibleEnemies.Add(enemy);
                }
            }
            else
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
