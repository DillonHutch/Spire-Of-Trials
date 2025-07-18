// NPC.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MoveInstruction
{
    public Vector3 direction;
    public float distance;

    public MoveInstruction(Vector3 dir, float dist)
    {
        direction = dir;
        distance = dist;
    }
}

public class NPC : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("World‑units per second")]
    [SerializeField] private float moveSpeed = 2f;

    /// <summary>
    /// Moves in 'direction' for 'distance' units at moveSpeed, then calls onComplete.
    /// </summary>
    public IEnumerator Move(
        Vector3 direction,
        float distance,
        Action onComplete = null
    )
    {
        var start = transform.position;
        var target = start + direction.normalized * distance;
        var duration = distance / moveSpeed;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Runs a list of MoveInstruction back‑to‑back at the same moveSpeed.
    /// </summary>
    public IEnumerator MoveSequence(
        List<MoveInstruction> moves,
        Action onComplete = null
    )
    {
        foreach (var inst in moves)
            yield return StartCoroutine(Move(inst.direction, inst.distance));

        onComplete?.Invoke();
    }
}
