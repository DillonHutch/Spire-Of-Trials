using UnityEngine;
using TMPro;

public class SkillMenuController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI[] skillLabels;  // Assign in Inspector, size = 6

    [Header("References")]
    [Tooltip("Drag your FightController here (BattleUI object)")]
    [SerializeField] private FightController fightController;

    [Header("Highlight Colors")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private int selectedIndex;
    private bool isOpen;
    private int skillCount;

    const int SKILL_COST = 20;

    void Awake()
    {
        // cache count and validate
        skillCount = (skillLabels != null) ? skillLabels.Length : 0;
        if (skillCount == 0)
            Debug.LogError("SkillMenuController: skillLabels array is empty! Set size=6 and assign each slot.");

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
        {
            skillLabels[i].color = (i == selectedIndex) ? highlightColor : normalColor;
            // optionally make the selected label bold:
            skillLabels[i].fontStyle = (i == selectedIndex) ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    private void ConfirmSelection()
    {
        // if player can pay the cost, close menu and invoke the skill
        if (fightController.TryPayComboCost(SKILL_COST))
        {
            CloseMenu();

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
        else
        {
            // not enough combo points—do nothing
            // optionally play an error sound or flash the UI
            Debug.Log("Not enough combo points to use skill");
        }
    }

    public void OpenMenu()
    {
        isOpen = true;
        menuPanel.SetActive(true);
        selectedIndex = 0;
        UpdateHighlights();
    }

    public void CloseMenu()
    {
        isOpen = false;
        menuPanel.SetActive(false);
    }
}
