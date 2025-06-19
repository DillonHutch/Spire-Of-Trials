using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TimingController : MonoBehaviour
{
    [Header("Moving Image")]
    [Tooltip("The UI element (RectTransform) to move left/right.")]
    public RectTransform movingImage;
    [Tooltip("Distance in pixels from center to left/right extremes.")]
    public float moveRange = 200f;
    [Tooltip("Speed in pixels per second.")]
    public float moveSpeed = 200f;

    [Header("Range Indicator (new)")]
    [Tooltip("UI Image/RectTransform that shows the full range.")]
    public RectTransform rangeIndicator;


    [Header("Time Awards (seconds)")]
    [Tooltip("Time awarded when image is all the way left.")]
    public float minTimeAward = 30f;
    [Tooltip("Time awarded when image is all the way right.")]
    public float maxTimeAward = 300f;

    [Header("Optional UI Feedback")]
    [Tooltip("If assigned, shows awarded time as MM:SS when you hit Space.")]
    public TextMeshProUGUI awardedTimeText;

    [Header("Timer Events")]
    [Tooltip("Methods to run when the awarded timer expires.")]
    private UnityEvent onTimerFinished;

    // +1 moving right, –1 moving left
    private int direction = +1;

    [SerializeField] private GameObject movingPanel;
    [SerializeField] private Animator animator;

    void OnValidate()
    {
        // update in Editor when you change moveRange
        if (rangeIndicator != null)
        {
            var size = rangeIndicator.sizeDelta;
            size.x = 2f * moveRange;
            rangeIndicator.sizeDelta = size;
            rangeIndicator.anchoredPosition = new Vector2(0f, rangeIndicator.anchoredPosition.y);
        }
    }

    void Start()
    {
        // init range graphic
        if (rangeIndicator != null)
        {
            var size = rangeIndicator.sizeDelta;
            size.x = 2f * moveRange;
            rangeIndicator.sizeDelta = size;
            rangeIndicator.anchoredPosition = new Vector2(0f, rangeIndicator.anchoredPosition.y);
        }

        // your existing Start logic
        if (movingImage != null)
            movingImage.anchoredPosition = new Vector2(-moveRange, movingImage.anchoredPosition.y);
    }

    void Update()
    {
        if (movingImage != null)
            MoveImage();

        if (Input.GetKeyDown(KeyCode.Space))
            AwardTime();
    }

    private void MoveImage()
    {
        if (!movingPanel.activeInHierarchy) return;

        // Advance position
        Vector2 pos = movingImage.anchoredPosition;
        pos.x += direction * moveSpeed * Time.deltaTime;

        // Bounce off edges
        if (pos.x >= moveRange)
        {
            pos.x = moveRange;
            direction = -1;
        }
        else if (pos.x <= -moveRange)
        {
            pos.x = -moveRange;
            direction = +1;
        }

        movingImage.anchoredPosition = pos;
    }

    private void AwardTime()
    {
        movingPanel.SetActive(false);

        // play your fight animation
        animator.Play("fightClicked");

        // start the fight logic on all enemies
        foreach (var enemy in FindObjectsOfType<EnemyParent>())
            enemy.StartFight();

        // Normalize: 0 at left, 1 at right
        float norm = (movingImage.anchoredPosition.x + moveRange) / (2f * moveRange);
        float awarded = Mathf.Lerp(minTimeAward, maxTimeAward, norm);

        Debug.Log($"Hit at {norm:P0}, awarding {awarded:F1} seconds");

        // kick off the countdown coroutine
        StartCoroutine(TimerCoroutine(awarded));
    }

    private IEnumerator TimerCoroutine(float duration)
    {
        float timeLeft = duration;
        if (awardedTimeText != null)
            awardedTimeText.text = FormatMMSS(timeLeft);

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            awardedTimeText.text = FormatMMSS(Mathf.Max(timeLeft, 0f));
            yield return null;
        }

        // fire inspector‐hooked events
        onTimerFinished?.Invoke();

        // instead of EndFight(), wait for attacks to finish first
        StartCoroutine(StopFightAfterAttacks());
    }

    /// <summary>
    /// Waits until no enemy is in mid‐attack, then stops the fight.
    /// </summary>
    private IEnumerator StopFightAfterAttacks()
    {
        // grab all enemies once
        EnemyParent[] enemies = FindObjectsOfType<EnemyParent>();

        // wait until every enemy has finished attacking
        bool anyAttacking;
        do
        {
            anyAttacking = false;
            foreach (var e in enemies)
            {
                if (e.IsAttacking)  
                {
                    anyAttacking = true;
                    break;
                }
            }
            yield return null;
        }
        while (anyAttacking);

        // now safely end the fight on all of them
        foreach (var e in enemies)
            e.StopFight();

        EventManager.Instance.TriggerEvent("takeDamageEvent", 1);

        // play your “fight ended” animation
        animator.Play("fightEnded");
    }

    private string FormatMMSS(float totalSeconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60f);
        int secs = Mathf.RoundToInt(totalSeconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}
