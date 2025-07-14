// LivesUI.cs
using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("Assign exactly N hearts here")]
    [SerializeField] private Image[] heartIcons;

    private void Awake()
    {
        if (heartIcons.Length == 0)
            Debug.LogError("LivesUI: assign your heart icon Images!");
    }

    private void OnEnable()
    {
        UpdateHearts(HealthManager.Instance.CurrentLives);
        EventManager.Instance.StartListening<int>(
            "OnLivesChanged",
            UpdateHearts
        );
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<int>(
            "OnLivesChanged",
            UpdateHearts
        );
    }

    private void UpdateHearts(int lives)
    {
        int clamped = Mathf.Clamp(lives, 0, heartIcons.Length);
        for (int i = 0; i < heartIcons.Length; i++)
            heartIcons[i].enabled = (i < clamped);
    }
}
