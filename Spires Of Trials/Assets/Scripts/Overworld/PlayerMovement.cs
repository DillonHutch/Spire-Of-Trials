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

    private bool inputLocked;    // swallow held keys until released
    private bool wasCanMove;     // track canMove changes


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wasCanMove = canMove;
        inputLocked = false;
    }

    void Update()
    {
        // 1) When movement just becomes allowed, re-lock input
        if (canMove && !wasCanMove)
            inputLocked = true;
        wasCanMove = canMove;

        // 2) If dialogue (or battle) is active, zero out movement
        if (DialogueManager.GetInstance().dialogueIsPlaying)
        {
            movement = Vector2.zero;
            return;
        }

        // 3) If we’re still locked, wait for all axes to return to zero
        if (inputLocked)
        {
            if (Input.GetAxisRaw("Horizontal") == 0f &&
                Input.GetAxisRaw("Vertical") == 0f)
            {
                inputLocked = false;
            }
            movement = Vector2.zero;
            return;
        }

        // 4) Otherwise, sample axes normally (only when canMove)
        float mx = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        float my = canMove ? Input.GetAxisRaw("Vertical") : 0f;
        movement = new Vector2(mx, my).normalized;
    }

    void FixedUpdate()
    {
        // Don’t slide if dialogue is blocking
        if (DialogueManager.GetInstance().dialogueIsPlaying)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Apply the movement vector
        rb.velocity = movement * moveSpeed;
    }
}
