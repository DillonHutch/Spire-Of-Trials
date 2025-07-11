using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Tooltip("How far the door moves up/down")]
    [SerializeField] private float moveDistance = 3f;
    [Tooltip("How fast the door moves")]
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool playerInZone;
    private bool isMoving = false;
    private bool lastRewindState = false;

    // now cache all Collider2Ds on this object + its children
    private Collider2D[] allColliders;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * moveDistance;
        transform.position = openPosition;

        // grab every Collider2D in this GameObject hierarchy
        allColliders = GetComponentsInChildren<Collider2D>();
        if (allColliders.Length == 0)
            Debug.LogWarning($"[{name}] No Collider2D found in {name} or its children!");
    }

    void FixedUpdate()
    {
        // detect rewind toggle
        bool isRewinding = TimeController.Instance != null
                         && TimeController.Instance.IsRewinding;
        if (isRewinding != lastRewindState)
        {
            lastRewindState = isRewinding;
            BeginMovement();
        }

        // decide target
        Vector3 target = isRewinding
                       ? openPosition
                       : (playerInZone ? closedPosition : openPosition);

        // move if active
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.fixedDeltaTime
            );

            // re-enable colliders once done
            if (transform.position == target)
            {
                isMoving = false;
                SetCollidersEnabled(true);
            }
        }
    }

    // call from your 2D trigger script
    public void SetPlayerInZone(bool inZone)
    {
        if (inZone != playerInZone)
        {
            playerInZone = inZone;
            BeginMovement();
        }
    }

    private void BeginMovement()
    {
        if (!isMoving)
        {
            isMoving = true;
            // disable all colliders (parent + children)
            SetCollidersEnabled(false);
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (var col in allColliders)
            col.enabled = enabled;
    }
}
