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

    private List<KeyCode> sequence = new List<KeyCode>();
    private int currentIndex;
    private bool isRunning;


    [SerializeField] private Animator animator;

    public void StartSequence()
    {
        GenerateRandomSequence();
        DisplaySequence();
        this.currentIndex = 0;
        this.isRunning = true;
        this.panel.SetActive(true);
    }

    private void Update()
    {
        if (!this.isRunning)
        {
            return;
        }

        if (Input.anyKeyDown)
        {
            KeyCode pressed = KeyCode.None;
            if (Input.GetKeyDown(KeyCode.UpArrow)) { pressed = KeyCode.UpArrow; }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { pressed = KeyCode.DownArrow; }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { pressed = KeyCode.LeftArrow; }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { pressed = KeyCode.RightArrow; }

            if (pressed != KeyCode.None)
            {
                EvaluateInput(pressed);
            }
        }
    }

    private void EvaluateInput(KeyCode pressed)
    {
        bool correct = (pressed == this.sequence[this.currentIndex]);

        // feedback
        this.arrowSlots[this.currentIndex].color = correct ? Color.green : Color.red;

        this.currentIndex++;

        if (!correct)
        {
            Fail();
            return;
        }

        if (this.currentIndex >= this.sequence.Count)
        {
            Succeed();
        }
    }

    private void Succeed()
    {
        Debug.Log("Arrow mini-game: SUCCESS!");
        EndSequence();

        EventManager.Instance.TriggerEvent("OnStartFight");
        animator.Play("closeMenu");
        TimingController.Instance.FightActive = true;

    }

    private void Fail()
    {
        Debug.Log("Arrow mini-game: FAILURE!");
        EndSequence();

        EventManager.Instance.TriggerEvent("OnStartFight");
        animator.Play("closeMenu");
        TimingController.Instance.FightActive = true;
    }

    private void EndSequence()
    {
        this.isRunning = false;
        this.panel.SetActive(false);

        // reset for next time
        for (int i = 0; i < this.arrowSlots.Length; i++)
        {
            this.arrowSlots[i].color = Color.white;
        }
    }

    private void GenerateRandomSequence()
    {
        this.sequence.Clear();
        KeyCode[] options = new KeyCode[]
        {
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow
        };

        for (int i = 0; i < this.arrowSlots.Length; i++)
        {
            int index = UnityEngine.Random.Range(0, options.Length);
            KeyCode choice = options[index];
            this.sequence.Add(choice);
        }
    }

    private void DisplaySequence()
    {
        for (int i = 0; i < this.sequence.Count; i++)
        {
            KeyCode kc = this.sequence[i];
            Image slot = this.arrowSlots[i];

            switch (kc)
            {
                case KeyCode.UpArrow:
                    slot.sprite = this.upSprite;
                    break;
                case KeyCode.DownArrow:
                    slot.sprite = this.downSprite;
                    break;
                case KeyCode.LeftArrow:
                    slot.sprite = this.leftSprite;
                    break;
                case KeyCode.RightArrow:
                    slot.sprite = this.rightSprite;
                    break;
            }
        }
    }
}
