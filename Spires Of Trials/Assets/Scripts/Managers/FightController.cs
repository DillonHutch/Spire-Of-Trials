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

    [Header("Skill 3 (Slow) Settings")]
    [SerializeField] private float slowDuration = 15f;   // how long the slow lasts
    [SerializeField] private float slowFactor = 0.5f;


    [Header("Skill 4 (Reveal Next Hit)")]
    [SerializeField] private float revealDuration = 30f;


    private int currentSkillIndex;
    private int lastDamageTaken = 0;    // store last round’s damage

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
        lastDamageTaken = damage;
    }

    private void Start()
    {
        battleController = GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleSceneController>();
    }

    private void OnFightPressed()
    {
        animator.Play("fightButtonClicked");
        if (timingController != null)
        {
            miniGamePanel.SetActive(true);
            timingController.StartCombatTimer();
        }
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
        // pass it into the arrow mini-game
        arrowMiniGame.SetSkillIndex(index);
        arrowMiniGame.StartSequence();
    }

    // called by ArrowMiniGameController on success/failure
    public void ApplySkillEffect(bool success)
    {
        if (success)
        {
            Debug.Log($"Skill {currentSkillIndex} succeeded");
            switch (currentSkillIndex)
            {
                case 0:
                    // heal for last round’s damage
                    EventManager.Instance.TriggerEvent("healDamageEvent", lastDamageTaken);
                    // reset if you don’t want double‐heals
                    lastDamageTaken = 0;
                    break;
                case 1:
                    
                    //TODO 
                    break;
                case 2:
                    // start the slow effect
                    StartCoroutine(ApplySlowToEnemies());
                    break;
                case 3:
                    StartCoroutine(RevealNextHits());
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


    private IEnumerator RevealNextHits()
    {
        // find every live enemy
        var enemies = FindObjectsOfType<EnemyParent>();
        foreach (var e in enemies)
            e.ShowNextHitIndicator(true);

        yield return new WaitForSeconds(revealDuration);

        foreach (var e in enemies)
            e.ShowNextHitIndicator(false);
    }



}
