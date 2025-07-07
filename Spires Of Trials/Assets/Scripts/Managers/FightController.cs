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
    BattleSceneController battleController;

    // Drag your TimingController (the one with OpenMiniGame()) here
    [SerializeField] private TimingController timingController;
    [SerializeField] private ArrowMiniGameController arrowMiniGame;

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
        animator.Play("fightButtonClicked");
        if (arrowMiniGame != null)
        {
            arrowMiniGame.StartSequence();
        }
        if (timingController != null)
        {
            timingController.StartCombatTimer();
        }
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

   



}
