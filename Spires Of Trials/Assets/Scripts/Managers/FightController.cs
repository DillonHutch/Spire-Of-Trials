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


    PlayerStats playerStats;

    GameObject inventoryCanvas;

    Transform consumablesMenu;

    private void OnEnable()
    {
        EventManager.Instance.StartListening("openFightMenu", GoToFightMenu);

    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening("openFightMenu", GoToFightMenu);
    }


    private void Awake()
    {
        // remove the old fight listener
        fightButton.onClick.RemoveAllListeners();

        // add a new one that runs the quip then starts the fight
        fightButton.onClick.AddListener(() =>
        {
            damageTakenThisFight = 0;
            StartCoroutine(DoQuipThenFight());
        });

        // re‑wire your other buttons as before
        itemButton.onClick.AddListener(OnItemPressed);
        skillButton.onClick.AddListener(OnSkillPressed);
        runButton.onClick.AddListener(OnRunPressed);

        // ensure the mini-game is hidden at start
        if (timingController != null)
            miniGamePanel.SetActive(false);


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

        playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();

        inventoryCanvas = GameObject.Find("InventoryCanvas");
        consumablesMenu = inventoryCanvas.transform.Find("ConsumablesMenu");
  
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

    // this coroutine runs the quip and only when it’s done starts the timer
    private IEnumerator DoQuipThenFight()
    {
        // play your “close menu” animation
        animator.Play("closeMenu");

        // wait for the quip dialogue to finish
        yield return StartCoroutine(TimingController.Instance.PlayQuip());


       
        // now start the fight timer
        timingController.StartTimer(playerStats.attack);
    }





    private void OnItemPressed()
    {
        animator.Play("closeMenu");
        consumablesMenu.gameObject.SetActive(true);

    }

    private void OnSkillPressed()
    {
        Debug.Log("OnSkillPressed called!");
        animator.Play("fightButtonClicked");
        skillMenu.OpenMenu();
    }



    private void OnRunPressed()
    {
        EventManager.Instance.TriggerEvent("takeDamageEvent", 10);
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
        animator.Play("minigameActive");
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
        ApplySkillRoutine(success);
    }

    private void ApplySkillRoutine(bool success)
    {

        animator.Play("closeMenu");

        // a) run your existing success/failure logic
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
                    StartCoroutine(ApplySlowToEnemies());
                    break;
                case 3:
                    int lookAhead = skipNextHit ? 2 : 1;
                    StartCoroutine(RevealNextHits(lookAhead));
                    break;
                case 4:
                    invincibleHits = 3;
                    Debug.Log("Invincibility skill activated: 3 free hits remaining");
                    break;
            }
        }
        else
        {
            Debug.Log($"Skill {currentSkillIndex} failed");
            //EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
        }

        animator.Play("fightEnded");

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

    private void GoToFightMenu()
    {
        // plays your “open main menu” animation
        consumablesMenu.gameObject.SetActive(false);
        animator.Play("fightEnded");
        
    }



}
