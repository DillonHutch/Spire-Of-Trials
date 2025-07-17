using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MashMiniGameController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;       // root panel for mini‑game
    [SerializeField] private Slider mashSlider;      // fill bar
    [Header("Settings")]
    [SerializeField] private float fillPerHit = 0.05f;
    [SerializeField] private float goal = 1f;        // slider.maxValue should be 1
    [SerializeField] private float timeLimit = 30f;
    [SerializeField] Animator animator;

    public void StartSequence()
    {
        mashSlider.value = 0f;
        panel.SetActive(true);
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        float timer = 0f;
        mashSlider.value = 0f;
        panel.SetActive(true);

        while (true)
        {
            // accumulate time
            timer += Time.deltaTime;

            // count mash hits
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                mashSlider.value = Mathf.Min(goal, mashSlider.value + fillPerHit);

            // if they hit the goal, break out
            if (mashSlider.value >= goal)
                break;

            // if 30 seconds have passed, reset and let them try again
            if (timer >= timeLimit)
            {
                timer = 0f;
                mashSlider.value = 0f;
                // optional: play a “reset” animation or sound here
            }

            yield return null;
        }

        // end sequence on success
        panel.SetActive(false);
        bool success = mashSlider.value >= goal;
        FindObjectOfType<FightController>().ApplySkillEffect(success);
        EventManager.Instance.TriggerEvent("OnStartFight");
        animator.Play("closeMenu");
        TimingController.Instance.FightActive = true;
        
    }

}
