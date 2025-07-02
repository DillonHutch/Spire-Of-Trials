using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ENEMY
{
    Skeleton,
    Knight
}

public static class EncounterManager
{

  


    public static ENEMY ENEMY_TYPE = ENEMY.Skeleton;


    /// <summary>
    /// If non‐null, the spawner will immediately spawn this prefab as the battle’s
    /// first enemy, then clear the reference.
    /// </summary>
    public static GameObject NextBattleEnemyPrefab;


}
