using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



[System.Serializable]
public class EnemyDialogue
{
    [Tooltip("Must match the GameObject.tag (or some ID) on your enemy prefabs")]
    public string enemyTag;
    [Tooltip("One or more Ink JSON assets for this enemy")]
    public TextAsset[] dialogues;
}


public class TimingController : MonoBehaviour
{

    public static TimingController Instance { get; private set; }
    public bool FightActive { get;  set; } = false;

    private float timeLeft;

    private bool timerPaused = false;
    public void PauseTimer() => timerPaused = true;
    public void ResumeTimer() => timerPaused = false;


    [Header("End‑of‑Round Dialogue Settings")]
    [SerializeField, Range(0f, 1f)]
    private float dialogueChance = 0.2f;

    [SerializeField]
    private EnemyDialogue[] enemyDialogues;

    // runtime lookup
    private Dictionary<string, TextAsset[]> dialogueMap;



    [SerializeField] GameObject dialogueArrow;


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
    [SerializeField] private Slider timeSlider;

    [Header("Timer Events")]
    [Tooltip("Methods to run when the awarded timer expires.")]
    private UnityEvent onTimerFinished;

    // +1 moving right, –1 moving left
    private int direction = +1;

    [SerializeField] private GameObject movingPanel;
    [SerializeField] private Animator animator;

    private Coroutine timerRoutine = null;
    private Coroutine combatTimerRoutine = null;




    private float lastAwardedDuration;

    // whether to skip the next end-of-round damage
    private bool skipNextDamage = false;


    private float combatTimer = 0f;


    private bool fightPanelUp = true;
    public bool FightPanelUp
    {
        get => fightPanelUp;
        private set => fightPanelUp = value;
    }



    public float CombatTimer => combatTimer;

    [Header("Combat Timer UI")]
    [SerializeField] private TextMeshProUGUI combatTimerText;


    /// <summary>
    /// Call this to prevent takeDamageEvent from firing
    /// at the end of the upcoming fight round.
    /// </summary>
    public void SkipNextDamageForThisRound()
    {
        skipNextDamage = true;
    }

    // call this at the very start of the battle
    public void ResetCombatTimer()
    {
        combatTimer = 0f;
        if (combatTimerText != null)
            combatTimerText.text = FormatMMSS(combatTimer);

        // make sure the old coroutine is dead
        if (combatTimerRoutine != null)
        {
            StopCoroutine(combatTimerRoutine);
            combatTimerRoutine = null;
        }
    }


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
        // bump the clock
        timeLeft += extraSeconds;

        // if we’re using the slider, extend it and refill it
        if (timeSlider != null)
        {
            timeSlider.maxValue = timeLeft;
            timeSlider.value = timeLeft;
        }

        // restart the countdown from the new total
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerCoroutine(timeLeft));
    }


    public void StartCombatTimer()
    {
        // if it’s already running, do nothing
        if (combatTimerRoutine != null)
            return;

        combatTimerRoutine = StartCoroutine(CombatTimerCoroutine());
    }


    private IEnumerator CombatTimerCoroutine()
    {
        while (true)
        {
            if (!timerPaused)
            {
                combatTimer += Time.deltaTime;
                if (combatTimerText != null)
                    combatTimerText.text = FormatMMSS(combatTimer);
            }
            yield return null;
        }
    }



    public void StopCombatTimer()
    {
        if (combatTimerRoutine != null)
        {
            StopCoroutine(combatTimerRoutine);
            combatTimerRoutine = null;
        }
    }


    void Awake()
    {
        Instance = this;
        fightPanelUp = true;



        dialogueMap = new Dictionary<string, TextAsset[]>();
        foreach (var ed in enemyDialogues)
            if (ed.dialogues != null && ed.dialogues.Length > 0)
                dialogueMap[ed.enemyTag] = ed.dialogues;

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

        // set up the slider
        if (timeSlider != null)
        {
            timeSlider.gameObject.SetActive(true);
            timeSlider.minValue = 0f;
            timeSlider.maxValue = duration;
            timeSlider.value = duration;
        }

        // begin the countdown
        FightActive = true;
        fightPanelUp = false;
        EventManager.Instance.TriggerEvent("OnStartFight");
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
        timeLeft = duration;

        // (remove any awardedTimeText updates here)

        while (timeLeft > 0f)
        {
            if (!timerPaused)
            {
                timeLeft -= Time.deltaTime;

                // update slider
                if (timeSlider != null)
                    timeSlider.value = Mathf.Max(timeLeft, 0f);
            }
            yield return null;
        }

        onTimerFinished?.Invoke();
        StartCoroutine(StopFightAfterAttacks());
    }



    /// <summary>
    /// Waits until no enemy is in mid‐attack, then stops the fight.
    /// </summary>
    private IEnumerator StopFightAfterAttacks()
    {
        // 1) wrap up the fight
        FightActive = false;
        fightPanelUp = true;
        EndSkillPhase();
        StopCombatTimer();

        // immediately stop all enemies
        foreach (var e in FindObjectsOfType<EnemyParent>())
            e.StopFight();

        // 2) maybe do a quip
        if (Random.value < dialogueChance)
        {
            var survivors = FindObjectsOfType<EnemyParent>()
                .Where(e => dialogueMap.ContainsKey(e.tag))
                .ToList();

            if (survivors.Count > 0)
            {
                var chosen = survivors[Random.Range(0, survivors.Count)];
                TextAsset[] quips = dialogueMap[chosen.tag];
                TextAsset enemyInk = quips[Random.Range(0, quips.Length)];

                // —— position the arrow over the chosen enemy ——
                Vector3 worldPos = chosen.transform.position + Vector3.up * 2f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                var canvasRect = dialogueArrow.GetComponentInParent<Canvas>()
                                          .GetComponent<RectTransform>();
                var arrowRect = dialogueArrow.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPos, Camera.main, out Vector2 localPoint);
                // only move X
                arrowRect.anchoredPosition = new Vector2(localPoint.x, arrowRect.anchoredPosition.y);

                // a) play the “enemyTalking” animation
                animator.Play("enemyTalking");
                dialogueArrow.SetActive(true);

                // b) fire up the enemy quip
                var dm = BattleDialogueManager.GetInstance();
                dm.EnterQuipMode(enemyInk);
                yield return new WaitUntil(() => dm.CanContinueToNextLine);
                
                // pause, then swap back without auto‐advance
                yield return new WaitForSeconds(2);
                dm.ResumeOriginalDialogue(autoContinue: false);

                // hide the arrow again
                
                dialogueArrow.SetActive(false);
                animator.Play("enemyTalkingEnd");

                yield return new WaitForSeconds(.1f);
            }
        }

        // 3) fire the global stop‐fight event now that dialogue’s back
        EventManager.Instance.TriggerEvent("OnStopFight");

        // 4) final UI wrap‐up
        FightPanelUp = true;
        animator.Play("fightEnded");
    }








    private string FormatMMSS(float totalSeconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60f);
        int secs = Mathf.RoundToInt(totalSeconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}
