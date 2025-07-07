using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TimingController : MonoBehaviour
{

    public static TimingController Instance { get; private set; }
    public bool FightActive { get;  set; } = false;

    private float timeLeft;

    private bool timerPaused = false;
    public void PauseTimer() => timerPaused = true;
    public void ResumeTimer() => timerPaused = false;





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

    private Coroutine timerRoutine = null;


    private bool fightPanelUp = true;
    public bool FightPanelUp
    {
        get => fightPanelUp;
        private set => fightPanelUp = value;
    }


    private float lastAwardedDuration;



    public bool SkillPhase { get; private set; }

    public void StartSkillPhase()
    {
        SkillPhase = true;
        // (Optionally) notify UI, play VFX, etc.
    }

    public void EndSkillPhase()
    {
        SkillPhase = false;
    }


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


    public void AddTime(float extraSeconds)
    {
        // increase the clock
        timeLeft += extraSeconds;
        // update UI immediately
        if (awardedTimeText != null)
            awardedTimeText.text = FormatMMSS(timeLeft);

        // restart the countdown so no partial-delta sneakily runs
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerCoroutine(timeLeft));
    }


    void Awake()
    {
        Instance = this;
        fightPanelUp = true;
   
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

        // only award time on Space if the mini-game panel is still open
        if (movingPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            
            AwardTime();
            
        }
            
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

    public void StartTimer(float duration)
    {
        // stop any previous timer
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        // immediately show the correct UI (if you want)
        if (awardedTimeText != null)
            awardedTimeText.text = FormatMMSS(duration);

        // start the fight!
        FightActive = true;
        EventManager.Instance.TriggerEvent("OnStartFight");

        // begin the countdown
        timerRoutine = StartCoroutine(TimerCoroutine(duration));
    }


    private void AwardTime()
    {
        movingPanel.SetActive(false);
        animator.Play("closeMenu");
        foreach (var enemy in FindObjectsOfType<EnemyParent>())
            enemy.StartFight();

        // calculate awarded time
        float norm = (movingImage.anchoredPosition.x + moveRange) / (2f * moveRange);
        float awarded = Mathf.Lerp(minTimeAward, maxTimeAward, norm);

        // 2) store it here
        lastAwardedDuration = awarded;

        // restart timer coroutine with awarded seconds
        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerCoroutine(awarded));

        FightActive = true;
        EventManager.Instance.TriggerEvent("OnStartFight");

        StartCoroutine(ClearPanelUpNextFrame());
    }

    private IEnumerator ClearPanelUpNextFrame()
    {
        // wait one engine frame
        yield return null;
        FightPanelUp = false;
    }

private IEnumerator TimerCoroutine(float duration)
{
    Debug.Log($"[Timing] TimerCoroutine START (duration={duration:F2})");
    timeLeft = duration;
    if (awardedTimeText != null)
        awardedTimeText.text = FormatMMSS(timeLeft);

    while (timeLeft > 0f)
    {
        if (!timerPaused)
        {
            timeLeft -= Time.deltaTime;
            awardedTimeText.text = FormatMMSS(Mathf.Max(timeLeft, 0f));
        }
        yield return null;
    }

    //Debug.Log("[Timing] TimerCoroutine FINISHED — about to invoke onTimerFinished");
    onTimerFinished?.Invoke();

    //Debug.Log("[Timing] Starting StopFightAfterAttacks()");
    StartCoroutine(StopFightAfterAttacks());
}


    /// <summary>
    /// Waits until no enemy is in mid‐attack, then stops the fight.
    /// </summary>
    private IEnumerator StopFightAfterAttacks()
    {
        // grab everyone
        var enemies = FindObjectsOfType<EnemyParent>();

        // 1) stop them from ever scheduling new attacks
        foreach (var e in enemies)
            e.StopFight();

        // 2) now wait just for any *in-flight* attacks to finish
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

        // 3) fire your end‐of‐fight logic
        FightActive = false;
        EventManager.Instance.TriggerEvent("OnStopFight");
        EndSkillPhase();
        FightPanelUp = true;
        animator.Play("fightEnded");

        // 4) (optional) damage survivors, etc.
        var survivors = FindObjectsOfType<EnemyParent>();
        if (survivors.Length > 0)
        {
            int damage = Mathf.RoundToInt(lastAwardedDuration);
            EventManager.Instance.TriggerEvent("takeDamageEvent", damage);
        }
    }


    private string FormatMMSS(float totalSeconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60f);
        int secs = Mathf.RoundToInt(totalSeconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}
