using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnLocations; // List of spawn locations
    [SerializeField] private List<GameObject> enemyPrefabs; // List of enemy prefabs
    [SerializeField] private GameObject miniBossPrefab; // Reference to MiniBoss prefab
    [SerializeField] private float spawnChance = 0.5f; // Chance for each location to spawn an enemy
    [SerializeField] private TextMeshProUGUI roundText; // Reference to TextMeshPro UI

    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List to track spawned enemies
    private bool isSpawning = false; // Prevents multiple spawn calls simultaneously
    private int roundCounter = 0; // Start at Round 1
    private bool bossSpawned = false; // Ensures the boss spawns only once

    //novo.co

    private int miniBossSpawnNumber = 2;

    private void Start()
    {
        if (spawnLocations.Count == 0 || enemyPrefabs.Count == 0 || miniBossPrefab == null)
        {
            Debug.LogError("Spawn locations, enemy prefabs, or MiniBoss prefab not assigned!");
            return;
        }

        UpdateRoundUI(); // Initialize the round counter text
        StartCoroutine(CheckAndSpawnEnemies());
    }

    private IEnumerator CheckAndSpawnEnemies()
    {
        while (true)
        {
            if (AllEnemiesDestroyed() && !isSpawning)
            {
                EventManager.Instance.TriggerEvent("healDamageEvent", 1);
                Debug.Log("All enemies destroyed. Starting new spawn cycle.");

                roundCounter++; // Increment round counter
                UpdateRoundUI(); // Update the round counter in UI

                if (roundCounter == miniBossSpawnNumber && !bossSpawned)
                {
                    yield return StartCoroutine(SpawnMiniBoss()); // Spawn the boss
                }
                else if (roundCounter < miniBossSpawnNumber)
                {
                    yield return StartCoroutine(SpawnEnemies()); // Continue normal spawning
                }
            }
            yield return new WaitForSeconds(0.5f); // Check periodically
        }
    }

    private IEnumerator SpawnMiniBoss()
    {
        isSpawning = true;
        bossSpawned = true; // Ensure the boss only spawns once

        Debug.Log("Spawning MiniBoss!");


        AudioManager.instance.SetMusic(MusicEnum.Sanctum);
        // Choose a random spawn location
        GameObject bossSpawnLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        // Instantiate the MiniBoss as a child of the spawn location
        GameObject miniBoss = Instantiate(miniBossPrefab, bossSpawnLocation.transform.position, Quaternion.identity);
        miniBoss.transform.SetParent(bossSpawnLocation.transform, true); // Set parent while maintaining world position

        spawnedEnemies.Add(miniBoss); // Add to tracking list

        Debug.Log($"MiniBoss spawned at {bossSpawnLocation.name}");

        isSpawning = false;
        yield return null;
    }


    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;
        bool atLeastOneSpawned = false;

        while (!atLeastOneSpawned)
        {
            for (int i = 0; i < spawnLocations.Count; i++)
            {
                float randomValue = Random.value; // Generate a random value between 0 and 1

                if (randomValue <= spawnChance)
                {
                    GameObject enemyToSpawn = SelectEnemyForPosition(i);

                    if (enemyToSpawn != null)
                    {
                        atLeastOneSpawned = true;

                        // Spawn the enemy and make it a child of the spawn location
                        GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnLocations[i].transform.position, Quaternion.identity);

                        if(spawnedEnemy.tag == "Monster")
                        {

                            Vector3 spawnLocation;
                            spawnedEnemy.transform.parent = spawnLocations[i].transform;

                            spawnLocation = spawnedEnemy.transform.position;
                            spawnLocation.x -= 3.5f;

                            spawnedEnemy.transform.position = spawnLocation;


                        }
                        else
                        {
                            spawnedEnemy.transform.parent = spawnLocations[i].transform;
                        }
                        spawnedEnemy.transform.parent = spawnLocations[i].transform;

                        spawnedEnemies.Add(spawnedEnemy); // Add to the tracking list
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

    private GameObject SelectEnemyForPosition(int positionIndex)
    {
        List<GameObject> possibleEnemies = new List<GameObject>();

        foreach (GameObject enemy in enemyPrefabs)
        {
            string enemyTag = enemy.tag; // Get the enemy's tag

            if (enemyTag == "Skeleton" && (positionIndex == 0 || positionIndex == 1 || positionIndex == 2))
            {
                possibleEnemies.Add(enemy);
            }
            else if (enemyTag == "Zombie" && (positionIndex == 1))
            {
                possibleEnemies.Add(enemy);
            }
            else if (enemyTag == "Monster" && (positionIndex == 0 || positionIndex == 2))
            {
                possibleEnemies.Add(enemy);
            }
        }

        if (possibleEnemies.Count > 0)
        {
            return possibleEnemies[Random.Range(0, possibleEnemies.Count)];
        }

        return null;
    }

    private bool AllEnemiesDestroyed()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count == 0;
    }

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = roundCounter.ToString();
        }
        else
        {
            Debug.LogWarning("Round UI Text is not assigned!");
        }
    }

    private void Update()
    {
        if (bossSpawned && AllEnemiesDestroyed())
        {
            Debug.Log("MiniBoss defeated. Loading WinScreen.");
            SceneManager.LoadScene("WinScreen");
        }
    }
}
