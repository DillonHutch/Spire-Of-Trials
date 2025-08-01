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
        StopFightAfterAttacks();
    }



    /// <summary>
    /// Waits until no enemy is in mid‐attack, then stops the fight.
    /// </summary>
    private void StopFightAfterAttacks()
    {
        // 1) wrap up the fight
        fightPanelUp = true;
        EndSkillPhase();
        StopCombatTimer();

        // STOP the fight flag first
        FightActive = false;

        // 2) clear out any pending attacks
        EnemyAttackQueue.ClearQueue();

        // 3) stop all enemy loops and reset them
        foreach (EnemyParent enemy in FindObjectsOfType<EnemyParent>())
            enemy.StopFight();

        EventManager.Instance.TriggerEvent("OnStopFight");
        animator.Play("fightEnded");
    }





    public IEnumerator PlayQuip()
    {
        // 1) maybe skip entirely
        if (Random.value >= dialogueChance)
            yield break;

        // 2) pick a random survivor that has a quip
        var survivors = FindObjectsOfType<EnemyParent>()
            .Where(e => dialogueMap.ContainsKey(e.tag))
            .ToList();
        if (survivors.Count == 0)
            yield break;

        var chosen = survivors[Random.Range(0, survivors.Count)];
        TextAsset[] quips = dialogueMap[chosen.tag];
        TextAsset enemyInk = quips[Random.Range(0, quips.Length)];

        // 3) position the arrow over them (only X‑axis)
        Vector3 worldPos = chosen.transform.position + Vector3.up * 2f;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        var canvasRect = dialogueArrow.GetComponentInParent<Canvas>()
                                     .GetComponent<RectTransform>();
        var arrowRect = dialogueArrow.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, Camera.main, out Vector2 localPoint);
        arrowRect.anchoredPosition = new Vector2(
            localPoint.x, arrowRect.anchoredPosition.y);

        // 4) show arrow + “talking” anim
        animator.gameObject.SetActive(true);
        animator.Play("enemyTalking");
        dialogueArrow.SetActive(true);

        // 5) fire the quip into your BattleDialogueManager
        var dm = BattleDialogueManager.GetInstance();
        dm.EnterQuipMode(enemyInk);
        yield return new WaitUntil(() => dm.CanContinueToNextLine);

        // 6) pause, then swap back
        yield return new WaitForSeconds(2.5f);
        dm.ResumeOriginalDialogue(autoContinue: false);

        // 7) hide arrow + end anim
        dialogueArrow.SetActive(false);

        animator.Play("enemyTalkingEnd");
        yield return new WaitForSeconds(0.1f);
    }




    private string FormatMMSS(float totalSeconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60f);
        int secs = Mathf.RoundToInt(totalSeconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}
