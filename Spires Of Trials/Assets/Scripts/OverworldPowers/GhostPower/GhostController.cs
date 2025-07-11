using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class GhostController : MonoBehaviour
{
    [Tooltip("Seconds of history to record")]
    [SerializeField] private float bufferDuration = 10f;
    [Tooltip("How far behind the ghost should be")]
     private float ghostDelay = 2f;
    [Tooltip("Your translucent ghost prefab")]
    [SerializeField] private GameObject ghostPrefab;

    // buffered snapshot of position, rotation, and animation state
    private struct State
    {
        public Vector3 pos;
        public Quaternion rot;
        public float time;
        public float moveX;
        public float moveY;
    }

    private readonly List<State> buffer = new List<State>();

    Animator playerAnim;
    SpriteRenderer playerSprite;
    GameObject ghostInstance;
    Animator ghostAnim;
    SpriteRenderer ghostSprite;

    void Start()
    {
        playerAnim = GetComponent<Animator>();
        playerSprite = GetComponent<SpriteRenderer>();

        ghostInstance = Instantiate(ghostPrefab);
        ghostInstance.SetActive(false);

        ghostAnim = ghostInstance.GetComponent<Animator>();
        ghostSprite = ghostInstance.GetComponent<SpriteRenderer>();

        // give the ghost the exact same controller so clips & layers match
        ghostAnim.runtimeAnimatorController = playerAnim.runtimeAnimatorController;
    }

    void FixedUpdate()
    {
        float now = Time.time;

        // prune old states first
        while (buffer.Count > 0 && now - buffer[0].time > bufferDuration)
            buffer.RemoveAt(0);

        // grab your player’s anim parameters
        float mx = playerAnim.GetFloat("MoveX");
        float my = playerAnim.GetFloat("MoveY");

        // record pos/rot/time + MoveX/MoveY
        buffer.Add(new State
        {
            pos = transform.position,
            rot = transform.rotation,
            time = now,
            moveX = mx,
            moveY = my
        });

        // ghost on/off logic…
        bool wantGhost = PowerController.Instance.CurrentPower == PowerType.Ghost
                      && Input.GetKey(KeyCode.LeftShift);
        if (wantGhost)
        {
            if (!ghostInstance.activeSelf) ghostInstance.SetActive(true);
            UpdateGhost(now);
        }
        else if (ghostInstance.activeSelf)
        {
            ghostInstance.SetActive(false);
        }
    }


  private void UpdateGhost(float now)
{
    float targetTime = now - ghostDelay;
    if (buffer.Count == 0) return;

    // find the two entries around targetTime
    State prev = buffer[0], next = buffer[buffer.Count - 1];
    for (int i = 0; i < buffer.Count; i++)
    {
        if (buffer[i].time >= targetTime)
        {
            next = buffer[i];
            prev = (i > 0) ? buffer[i - 1] : buffer[i];
            break;
        }
    }

    // interpolate transform
    float dt = next.time - prev.time;
    float t  = dt > 0f ? (targetTime - prev.time) / dt : 0f;
    ghostInstance.transform.position = Vector3.Lerp(prev.pos, next.pos, t);
    ghostInstance.transform.rotation = Quaternion.Slerp(prev.rot, next.rot, t);

    // interpolate your movement floats
    float ghostMX = Mathf.Lerp(prev.moveX, next.moveX, t);
    float ghostMY = Mathf.Lerp(prev.moveY, next.moveY, t);

    // drive the ghost’s Animator with those same floats
    ghostAnim.SetFloat("MoveX", ghostMX);
    ghostAnim.SetFloat("MoveY", ghostMY);
    // force it to sample immediately
    ghostAnim.Update(0f);

}



}
