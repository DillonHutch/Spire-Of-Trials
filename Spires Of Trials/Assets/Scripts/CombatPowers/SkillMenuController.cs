using UnityEngine;
using TMPro;
using System.Collections;

public class SkillMenuController : MonoBehaviour
{
    const int SKILL_COST = 20;

    [Header("UI Setup")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI[] skillLabels; // size = 6
    [SerializeField] private TextMeshProUGUI costLabel;     // single cost label

    [Header("References")]
    [SerializeField] private FightController fightController;

    [Header("Highlight Colors")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Cost Colors")]
    [SerializeField] private Color canPayColor = Color.green;
    [SerializeField] private Color cannotPayColor = Color.yellow;
    [SerializeField] private Color errorColor = Color.red;

    private int selectedIndex;
    private bool isOpen;
    private int skillCount;

    void Awake()
    {
        skillCount = skillLabels?.Length ?? 0;
        if (skillCount == 0)
            Debug.LogError("SkillMenuController: skillLabels is empty or null");
        if (costLabel == null)
            Debug.LogError("SkillMenuController: costLabel not assigned");
        CloseMenu();
    }

    void Update()
    {
        if (!isOpen || skillCount == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveSelection(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveSelection(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveSelection(3);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveSelection(-3);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            ConfirmSelection();
    }

    private void MoveSelection(int delta)
    {
        selectedIndex = (selectedIndex + delta + skillCount) % skillCount;
        UpdateHighlights();
        UpdateCostDisplay();
    }

    private void UpdateHighlights()
    {
        for (int i = 0; i < skillCount; i++)
        {
            skillLabels[i].color = (i == selectedIndex) ? highlightColor : normalColor;
            skillLabels[i].fontStyle = (i == selectedIndex) ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    private void UpdateCostDisplay()
    {
        // always show the same cost, but color it based on affordability
        costLabel.text = SKILL_COST.ToString();
        bool canPay = fightController.CanPayComboCost(SKILL_COST);
        costLabel.color = canPay ? canPayColor : cannotPayColor;
        costLabel.fontStyle = FontStyles.Bold;
    }

    private void ConfirmSelection()
    {
        if (fightController.TryPayComboCost(SKILL_COST))
        {
            // you have enough points
            CloseMenu();
            fightController.OnSkillChosen(selectedIndex);
        }
        else
        {
            // not enough → animate the single costLabel
            StartCoroutine(AnimateCostLabelError());
        }
    }

    public void OpenMenu()
    {
        isOpen = true;
        menuPanel.SetActive(true);
        selectedIndex = 0;
        UpdateHighlights();
        UpdateCostDisplay();
    }

    public void CloseMenu()
    {
        isOpen = false;
        menuPanel.SetActive(false);
    }

    private IEnumerator AnimateCostLabelError()
    {
        var rt = costLabel.rectTransform;
        var original = rt.localScale;
        var elapsed = 0f;
        const float duration = 0.5f;

        costLabel.color = errorColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float shake = 1f + Mathf.Sin(elapsed * 20f) * 0.2f;
            rt.localScale = original * shake;
            yield return null;
        }

        rt.localScale = original;
        // restore yellow or green based on current combo
        costLabel.color = fightController.CanPayComboCost(SKILL_COST)
                            ? canPayColor
                            : cannotPayColor;
    }
}
