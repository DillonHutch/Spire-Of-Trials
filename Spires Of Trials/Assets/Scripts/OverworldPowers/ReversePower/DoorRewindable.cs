using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Tooltip("How far the door moves up/down")]
    [SerializeField] private float moveDistance = 3f;
    [Tooltip("How fast the door moves")]
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * moveDistance;
    }

    void FixedUpdate()
    {
        // true while you hold either Shift key
        bool shouldOpen = Input.GetKey(KeyCode.LeftShift)
                       || Input.GetKey(KeyCode.RightShift);

        Vector3 target = shouldOpen
            ? openPosition
            : closedPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.fixedDeltaTime
        );
    }
}
