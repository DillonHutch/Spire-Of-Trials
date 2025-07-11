
// SkillMenuController.cs (updated with inspector reference and fallback)
using UnityEngine;
using UnityEngine.UI;

public class SkillMenuController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Image[] skillIcons;  // Assign in Inspector, size = 6

    [Header("References")]
    [Tooltip("Drag your FightController here (BattleUI object)")]
    [SerializeField] private FightController fightController;

    [Header("Highlight Colors")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private int selectedIndex;
    private bool isOpen;
    private int skillCount;

    void Awake()
    {
        // cache count and validate
        skillCount = (skillIcons != null) ? skillIcons.Length : 0;
        if (skillCount == 0)
            Debug.LogError("SkillMenuController: skillIcons array is empty! Set size=6 and assign each slot.");

        CloseMenu();
    }

    void Update()
    {
        if (!isOpen || skillCount == 0)
            return;

        // navigation
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveSelection(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveSelection(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveSelection(3);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveSelection(-3);

        // confirm
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            ConfirmSelection();
    }

    private void MoveSelection(int delta)
    {
        selectedIndex = (selectedIndex + delta + skillCount) % skillCount;
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        for (int i = 0; i < skillCount; i++)
            skillIcons[i].color = (i == selectedIndex) ? highlightColor : normalColor;
    }

    private void ConfirmSelection()
    {
        CloseMenu();

        // ensure we have a FightController
        if (fightController == null)
        {
            fightController = FindObjectOfType<FightController>();
            if (fightController == null)
            {
                Debug.LogError("SkillMenuController: FightController not assigned or found!");
                return;
            }
        }

        fightController.OnSkillChosen(selectedIndex);
    }

    public void OpenMenu()
    {
        isOpen = true;
        menuPanel.SetActive(true);
        Debug.Log("AM I OPEN??");
        selectedIndex = 0;
        UpdateHighlights();
    }

    public void CloseMenu()
    {
        isOpen = false;
        menuPanel.SetActive(false);
    }
}

