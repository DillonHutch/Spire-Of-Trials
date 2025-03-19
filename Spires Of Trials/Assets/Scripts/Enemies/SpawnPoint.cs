using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a spawn point in the game.
/// Defines spawn positions for enemies or objects using an enumerated system.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    #region Enum

    /// <summary>
    /// Enum representing different spawn point positions.
    /// LEFT = 0, MIDDLE = 1, RIGHT = 2.
    /// </summary>
    public enum SpawnPointNum
    {
        LEFT = 0,   // Left-side spawn point
        MIDDLE = 1, // Middle spawn point
        RIGHT = 2   // Right-side spawn point
    }

    #endregion

    #region Serialized Fields

    /// <summary>
    /// Determines the specific spawn point's position.
    /// Assigned in the Unity Inspector.
    /// </summary>
    [SerializeField] private SpawnPointNum spawnPointNum;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the integer value of the assigned spawn point position.
    /// Allows external scripts to access the spawn point number.
    /// </summary>
    public int SpawnPointNumber { get { return (int)spawnPointNum; } }

    #endregion
}
