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
        // play your fight animation
        animator.Play("fightButtonClicked");



        // show/start the mini-game
        if (timingController != null)
            miniGamePanel.SetActive(true);
      
    }

    private void OnItemPressed()
    {
        // play your fight animation
        animator.Play("fightButtonClicked");



        // show/start the mini-game
        if (timingController != null)
            miniGamePanel.SetActive(true);

    }

    private void OnSkillPressed()
    {
        // play your fight animation
        animator.Play("fightButtonClicked");

        TimingController.Instance.StartSkillPhase();
        if (arrowMiniGame != null)
        {
            arrowMiniGame.StartSequence();
        }

    }


    private void OnRunPressed()
    {
       battleController.EndBattle();

    }



}
