using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnLocations; // List of spawn locations
    [SerializeField] private List<GameObject> enemyPrefabs; // List of enemy prefabs
    [SerializeField] private float spawnChance = 0.5f; // Chance for each location to spawn an enemy
    [SerializeField] private TextMeshProUGUI roundText; // Reference to TextMeshPro UI

    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List to track spawned enemies
    private bool isSpawning = false; // Prevents multiple spawn calls simultaneously
    private int roundCounter = 0; // Start at Round 1

    private void Start()
    {
        if (spawnLocations.Count == 0 || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Spawn locations or enemy prefabs are not assigned!");
            return;
        }

        UpdateRoundUI(); // Initialize the round counter text
        StartCoroutine(CheckAndSpawnEnemies());
    }

    private IEnumerator CheckAndSpawnEnemies()
    {
        while (true)
        {
            // Check if all enemies are destroyed
            if (AllEnemiesDestroyed() && !isSpawning)
            {
                EventManager.Instance.TriggerEvent("healDamageEvent", 1);
                Debug.Log("All enemies destroyed. Starting new spawn cycle.");

                roundCounter++; // Increment round counter
                UpdateRoundUI(); // Update the round counter in UI

                yield return StartCoroutine(SpawnEnemies());
            }
            yield return new WaitForSeconds(0.5f); // Check periodically
        }
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
            else if (enemyTag == "Zombie" && ( positionIndex == 1))
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
        if(roundCounter == 25)
        {
            SceneManager.LoadScene("WinScreen");
        }
    }
}
