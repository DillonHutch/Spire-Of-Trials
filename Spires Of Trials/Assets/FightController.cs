using UnityEngine;
using UnityEngine.UI;

public class FightController : MonoBehaviour
{
    [SerializeField] private Button fightButton;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject miniGamePanel;

    // Drag your TimingController (the one with OpenMiniGame()) here
    [SerializeField] private TimingController timingController;

    private void Awake()
    {
        // wire up the inspector-assigned button
        fightButton.onClick.AddListener(OnFightPressed);

        // ensure the mini-game is hidden at start
        if (timingController != null)
            miniGamePanel.SetActive(false);
    }

    private void OnFightPressed()
    {
        // play your fight animation
        animator.Play("fightButtonClicked");


        // start the fight logic on all enemies
        //foreach (var enemy in FindObjectsOfType<EnemyParent>())
        //{
        //    enemy.StartFight();
        //}


        // show/start the mini-game
        if (timingController != null)
            miniGamePanel.SetActive(true);

      
    }
}
