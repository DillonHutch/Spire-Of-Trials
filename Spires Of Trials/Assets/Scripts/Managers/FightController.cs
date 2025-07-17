using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FightController : MonoBehaviour
{
    [SerializeField] private Button fightButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button runButton;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private SkillMenuController skillMenu;
    BattleSceneController battleController;

    // Drag your TimingController (the one with OpenMiniGame()) here
    [SerializeField] private TimingController timingController;
    [SerializeField] private ArrowMiniGameController arrowMiniGame;
    [SerializeField] private SpaceBarMiniGameController spaceBarMiniGame;
    [SerializeField] private MashMiniGameController mashMiniGame;


    [HideInInspector] public bool skipNextHit;

    [Header("Skill 3 (Slow) Settings")]
    [SerializeField] private float slowDuration = 15f;   // how long the slow lasts
    [SerializeField] private float slowFactor = 0.5f;


    [Header("Skill 4 (Reveal Next Hit)")]
    [SerializeField] private float revealDuration = 30f;


    [Header("Skill 5 (shield)")]
    [HideInInspector] public int invincibleHits = 0;

    private int currentSkillIndex;
    private int lastDamageTaken = 0;    // store last round’s damage


    private PlayerAttackingScript playerAttacker;

    private int damageTakenThisFight;



    private void Awake()
    {
        // wire up the inspector-assigned button
        fightButton.onClick.AddListener(OnFightPressed);
        itemButton.onClick.AddListener(OnItemPressed);
        skillButton.onClick.AddListener(OnSkillPressed);
        runButton.onClick.AddListener(OnRunPressed);

        // ensure the mini-game is hidden at start
        if (timingController != null)
            miniGamePanel.SetActive(false);


        fightButton.onClick.AddListener(() => {
            damageTakenThisFight = 0;
            OnFightPressed();
        });

        EventManager.Instance.StartListening<int>(
    "takeDamageEvent", OnPlayerDamaged
);
    }

    private void OnDestroy()
    {
        EventManager.Instance.StopListening<int>(
            "takeDamageEvent", OnPlayerDamaged
        );
    }


    private void OnPlayerDamaged(int damage)
    {
        damageTakenThisFight += damage;
    }

    void Start()
    {
        battleController = GameObject.FindGameObjectWithTag("BattleController")
                               .GetComponent<BattleSceneController>();
        playerAttacker = FindObjectOfType<PlayerAttackingScript>();
        if (playerAttacker == null)
            Debug.LogError("No PlayerAttackingScript found in scene");
    }


    /// <summary>
    /// Returns true if the player has at least ‘cost’ combo points.
    /// </summary>
    public bool CanPayComboCost(int cost)
    {
        return playerAttacker != null && playerAttacker.ComboCount >= cost;
    }

    /// <summary>
    /// Returns from the skill‐select UI back to the main battle menu.
    /// </summary>
    public void ShowBattleMenu()
    {
        // plays your “open main menu” animation
        animator.Play("fightEnded");
    }



    /// <summary>
    /// Try to pay the combo cost. Returns true if you had enough points.
    /// </summary>
    public bool TryPayComboCost(int cost)
    {
        return playerAttacker != null && playerAttacker.ConsumeCombo(cost);
    }

    private void OnFightPressed()
    {
        animator.Play("closeMenu");

        // start a fixed 10-second fight timer
        timingController.StartTimer(10f);
    }


    private void OnItemPressed()
    {
        animator.Play("fightButtonClicked");
        if (timingController != null)
        {
            miniGamePanel.SetActive(true);
            timingController.StartCombatTimer();
        }
    }

    private void OnSkillPressed()
    {
        Debug.Log("OnSkillPressed called!");
        animator.Play("fightButtonClicked");
        skillMenu.OpenMenu();
    }



    private void OnRunPressed()
    {
        EventManager.Instance.TriggerEvent("takeDamageEvent", 50);
        // wait one frame so listeners still exist
        StartCoroutine(EndBattleNextFrame());
    }

    private IEnumerator EndBattleNextFrame()
    {
        yield return new WaitForEndOfFrame();
        battleController.EndBattle();
    }


    public void OnSkillChosen(int index)
    {
        currentSkillIndex = index;
        switch (index)
        {
            case 0:
                spaceBarMiniGame.StartSequence();
                break;
            case 1:
                mashMiniGame.StartSequence();
                break;
            default:
                arrowMiniGame.SetSkillIndex(index);
                arrowMiniGame.StartSequence();
                break;
        }
    }



    // called by ArrowMiniGameController on success/failure
    public void ApplySkillEffect(bool success)
    {


        TimingController.Instance.StartTimer(5f);

        if (success)
        {
            Debug.Log($"Skill {currentSkillIndex} succeeded");
            switch (currentSkillIndex)
            {
                case 0:
                    HealthManager.Instance.RestoreHits(damageTakenThisFight);
                    lastDamageTaken = 0;
                    break;
                case 1:

                    skipNextHit = true;

                    foreach (var e in FindObjectsOfType<EnemyParent>())
                        e.UpdateNextHitIcon();
                    break;
                case 2:
                    // start the slow effect
                    StartCoroutine(ApplySlowToEnemies());
                    break;
                case 3:
                    int lookAhead = skipNextHit ? 2 : 1;
                    StartCoroutine(RevealNextHits(lookAhead));
                    break;
                case 4:
                    // ← NEW: grant 3 free hits
                    invincibleHits = 3;
                    Debug.Log("Invincibility skill activated: 3 free hits remaining");
                    break;


            }
        }
        else
        {
            Debug.Log($"Skill {currentSkillIndex} failed");
            // … your existing failure logic …
        }
    }


    private IEnumerator ApplySlowToEnemies()
    {
        // find all enemies
        var enemies = FindObjectsOfType<EnemyParent>();
        foreach (var e in enemies)
            e.SetSlow(slowFactor);

        yield return new WaitForSeconds(slowDuration);

        foreach (var e in enemies)
            e.ResetSpeed();
    }


    private IEnumerator RevealNextHits(int offset)
    {
        var enemies = FindObjectsOfType<EnemyParent>();
        foreach (var e in enemies)
            e.ShowNextHitIndicator(true, offset);

        yield return new WaitForSeconds(revealDuration);

        foreach (var e in enemies)
            e.ShowNextHitIndicator(false, offset);
    }



}
