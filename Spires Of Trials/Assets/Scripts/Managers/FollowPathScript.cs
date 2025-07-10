using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 3) Waypoint follower that also rewinds its index
public class FollowPathScript : RewindableMono
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    private int waypointIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        waypointIndex = 0;
        if (waypoints != null && waypoints.Length > 0)
            transform.position = waypoints[0].position;
    }

    protected override int GetExtraInt()
    {
        return waypointIndex;
    }

    protected override void ApplyExtraInt(int extraInt)
    {
        waypointIndex = extraInt;
    }

    void FixedUpdate()
    {
        if (!TimeController.Instance.IsRewinding)
            Move();
    }

    private void Move()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (waypointIndex >= waypoints.Length)
            waypointIndex = 0;

        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[waypointIndex].position,
            moveSpeed * Time.fixedDeltaTime
        );

        if (Vector2.Distance(transform.position, waypoints[waypointIndex].position) < 0.01f)
            waypointIndex++;
    }
}
