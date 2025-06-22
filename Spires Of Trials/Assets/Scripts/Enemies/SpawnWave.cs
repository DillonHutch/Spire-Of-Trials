using UnityEngine;
using System.Collections.Generic;

public enum WaveType
{
    Regular,
    MiniBoss,
    FrogBoss,
    FinalBoss
}

[CreateAssetMenu(menuName = "Spawning/Spawn Wave")]
public class SpawnWave : ScriptableObject
{
    [Tooltip("Which round this wave corresponds to.")]
    public int roundNumber;

    [Tooltip("Is this wave a boss or regular enemies?")]
    public WaveType waveType = WaveType.Regular;

    [Tooltip("If boss, which prefab to use.  If Regular, leave null.")]
    public GameObject bossPrefab;

    [Tooltip("Optional offset to apply when spawning the boss.")]
    public Vector3 bossOffset = Vector3.zero;

    [Tooltip("List of entries to spawn when this wave is Regular.")]
    public List<EnemySpawnEntry> regularSpawns;
}

[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Prefab of the enemy.")]
    public GameObject prefab;

    [Tooltip("Chance (0–1) to spawn, per spawn-location.")]
    public float spawnChance = 0.5f;

    [Tooltip("Which spawn-indices (0=left,1=center,2=right) are valid for this enemy?")]
    public List<int> validPositions;
}
