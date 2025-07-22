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

 


    // Blend‑tree parameter hashes
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        // start idle
        _anim.SetFloat(MoveX, 0f);
        _anim.SetFloat(MoveY, 0f);
    }

    /// <summary>
    /// Moves in 'direction' for 'distance' units at moveSpeed, then calls onComplete.
    /// </summary>
    public IEnumerator Move(
        Vector3 direction,
        float distance,
        Action onComplete = null
    )
    {
        // normalized direction for consistent animation values
        Vector3 dirNorm = direction.normalized;
        _anim.SetFloat(MoveX, dirNorm.x);
        _anim.SetFloat(MoveY, dirNorm.y);

        Vector3 start = transform.position;
        Vector3 target = start + dirNorm * distance;
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        onComplete?.Invoke();

        // reset to idle
        _anim.SetFloat(MoveX, 0f);
        _anim.SetFloat(MoveY, 0f);
    }

    /// <summary>
    /// Runs a list of MoveInstruction back‑to‑back at the same moveSpeed.
    /// </summary>
    public IEnumerator MoveSequence(
        List<MoveInstruction> moves,
        Action onComplete = null
    )
    {
        foreach (MoveInstruction inst in moves)
        {
            yield return StartCoroutine(Move(inst.direction, inst.distance));
        }

        onComplete?.Invoke();
    }
}
