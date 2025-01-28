using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnLocations; // List of spawn locations
    [SerializeField] private List<GameObject> enemyPrefabs; // List of enemy prefabs
    [SerializeField] private float spawnChance = 0.5f; // Chance for each location to spawn an enemy

    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List to track spawned enemies
    private bool isSpawning = false; // To prevent multiple spawn calls simultaneously

    private void Start()
    {
        if (spawnLocations.Count == 0 || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Spawn locations or enemy prefabs are not assigned!");
            return;
        }

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
                yield return StartCoroutine(SpawnEnemies());
            }
            yield return new WaitForSeconds(.5f); // Check periodically
        }
    }

    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;
        bool atLeastOneSpawned = false;

        // Keep running the loop until at least one enemy is spawned
        while (!atLeastOneSpawned)
        {
            for (int i = 0; i < spawnLocations.Count; i++)
            {
                float randomValue = Random.value; // Generate a random value between 0 and 1

                if (randomValue <= spawnChance)
                {
                    atLeastOneSpawned = true;

                    // Randomly select an enemy prefab
                    GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

                    // Spawn the enemy and make it a child of the spawn location
                    GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnLocations[i].transform.position, Quaternion.identity);
                    spawnedEnemy.transform.parent = spawnLocations[i].transform;

                    spawnedEnemies.Add(spawnedEnemy); // Add to the tracking list
                    Debug.Log($"Spawned {enemyToSpawn.name} at {spawnLocations[i].name}");
                }
                else
                {
                    Debug.Log($"No enemy spawned at {spawnLocations[i].name}");
                }
            }

            // If no enemy was spawned, retry the loop after a short delay
            if (!atLeastOneSpawned)
            {
                Debug.Log("No enemies spawned, retrying...");
                yield return null;
            }
        }

        Debug.Log("At least one enemy spawned. Spawning complete.");
        isSpawning = false;
    }

    private bool AllEnemiesDestroyed()
    {
        // Remove any null references (destroyed enemies) from the list
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        

        // If the list is empty, all enemies are destroyed
        return spawnedEnemies.Count == 0;
    }
}
