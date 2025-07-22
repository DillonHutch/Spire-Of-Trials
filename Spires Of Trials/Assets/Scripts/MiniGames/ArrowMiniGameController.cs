// ArrowMiniGameController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowMiniGameController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image[] arrowSlots = new Image[6];

    [Header("Arrow Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [SerializeField] private Animator animator;

    private List<KeyCode> sequence = new List<KeyCode>();
    private int currentIndex;
    private bool isRunning;
    private int skillIndex;

    public void SetSkillIndex(int idx)
    {
        skillIndex = idx;
    }

    public void StartSequence()
    {
        GenerateRandomSequence();
        DisplaySequence();
        currentIndex = 0;
        isRunning = true;
        panel.SetActive(true);
    }

    void Update()
    {
        if (!isRunning) return;

        if (Input.anyKeyDown)
        {
            KeyCode pressed = KeyCode.None;
            if (Input.GetKeyDown(KeyCode.UpArrow)) pressed = KeyCode.UpArrow;
            if (Input.GetKeyDown(KeyCode.DownArrow)) pressed = KeyCode.DownArrow;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) pressed = KeyCode.LeftArrow;
            if (Input.GetKeyDown(KeyCode.RightArrow)) pressed = KeyCode.RightArrow;

            if (pressed != KeyCode.None)
                EvaluateInput(pressed);
        }
    }

    private void EvaluateInput(KeyCode pressed)
    {
        bool correct = (pressed == sequence[currentIndex]);
        arrowSlots[currentIndex].color = correct ? Color.green : Color.red;
        currentIndex++;

        if (!correct)
        {
            Fail();
            return;
        }

        if (currentIndex >= sequence.Count)
            Succeed();
    }

    private void Succeed()
    {
        // Report success and play animation
        FindObjectOfType<FightController>().ApplySkillEffect(true);

        EndSequence();
    }

    private void Fail()
    {
        FindObjectOfType<FightController>().ApplySkillEffect(false);

        EndSequence();
    }

    private void EndSequence()
    {
        isRunning = false;
        panel.SetActive(false);
        TimingController.Instance.StopCombatTimer();
        for (int i = 0; i < arrowSlots.Length; i++)
            arrowSlots[i].color = Color.white;
    }

    private void GenerateRandomSequence()
    {
        sequence.Clear();
        KeyCode[] options = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
        for (int i = 0; i < arrowSlots.Length; i++)
            sequence.Add(options[Random.Range(0, options.Length)]);
    }

    private void DisplaySequence()
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            switch (sequence[i])
            {
                case KeyCode.UpArrow: arrowSlots[i].sprite = upSprite; break;
                case KeyCode.DownArrow: arrowSlots[i].sprite = downSprite; break;
                case KeyCode.LeftArrow: arrowSlots[i].sprite = leftSprite; break;
                case KeyCode.RightArrow: arrowSlots[i].sprite = rightSprite; break;
            }
        }
    }
}
