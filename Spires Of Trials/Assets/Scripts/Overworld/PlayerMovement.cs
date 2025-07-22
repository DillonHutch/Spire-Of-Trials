using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Units per second (base)")]
    public float moveSpeed = 5f;

    // multiplier for power effects (e.g. SuperSpeed)
    private float speedMultiplier = 1f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private Animator anim;

    [Tooltip("Toggled by your BattleSceneController or dialogue system")]
    public bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        EventManager.Instance.StartListening("StartPlayerMovement", StartMovement);
        EventManager.Instance.StartListening("StopPlayerMovement", StopMovement);
    }

    void OnDisable()
    {
        EventManager.Instance.StopListening("StartPlayerMovement", StartMovement);
        EventManager.Instance.StopListening("StopPlayerMovement", StopMovement);
    }

    private void StartMovement()
    {
        canMove = true;
    }

    private void StopMovement()
    {
        // disable further input
        canMove = false;

        // clear our movement state
        movement = Vector2.zero;
        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveY", 0f);

        // halt any physics velocity
        rb.velocity = Vector2.zero;

        // flush Unity’s input buffer so held keys don’t carry over
        Input.ResetInputAxes();
    }


    void Update()
    {

        if (!canMove)
        {

            movement = Vector2.zero;
            return;
        }

        float mx = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        float my = canMove ? Input.GetAxisRaw("Vertical") : 0f;
        movement = new Vector2(mx, my).normalized;

        anim.SetFloat("MoveX", movement.x);
        anim.SetFloat("MoveY", movement.y);
    }

    void FixedUpdate()
    {
        // apply base speed and any multiplier
        rb.velocity = movement * moveSpeed * speedMultiplier;
    }

    /// <summary>
    /// Called by PowerController to adjust movement speed (e.g. SuperSpeed)
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    /// <summary>
    /// Reset speed multiplier back to normal
    /// </summary>
    public void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
    }
}
