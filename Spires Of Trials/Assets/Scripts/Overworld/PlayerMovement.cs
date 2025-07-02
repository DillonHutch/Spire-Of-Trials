using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Units per second")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    [Tooltip("Toggled by your BattleSceneController or dialogue system")]
    public bool canMove = true;

    private bool inputLocked;
    private bool wasCanMove;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wasCanMove = canMove;
        inputLocked = false;
    }

    void OnEnable()
    {
        // subscribe to your start/stop events
        EventManager.Instance.StartListening("StartPlayerMovement", StartMovement);
        EventManager.Instance.StartListening("StopPlayerMovement", StopMovement);
    }

    void OnDisable()
    {
        // unsubscribe when disabled
        EventManager.Instance.StopListening("StartPlayerMovement", StartMovement);
        EventManager.Instance.StopListening("StopPlayerMovement", StopMovement);
    }

    // called when you fire the "StartPlayerMovement" event
    private void StartMovement()
    {
        canMove = true;
        // re-lock so held keys don’t immediately move
        inputLocked = true;
    }

    // called when you fire the "StopPlayerMovement" event
    private void StopMovement()
    {
        canMove = false;
        // optionally clear velocity here:
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // (your existing lock/unlock logic)
        if (canMove && !wasCanMove) inputLocked = true;
        wasCanMove = canMove;

        if (inputLocked)
        {
            if (Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f)
                inputLocked = false;
            movement = Vector2.zero;
            return;
        }

        float mx = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        float my = canMove ? Input.GetAxisRaw("Vertical") : 0f;
        movement = new Vector2(mx, my).normalized;
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }
}
