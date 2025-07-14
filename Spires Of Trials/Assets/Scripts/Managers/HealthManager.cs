// HealthManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Configure how many lives you start with")]
    [SerializeField] private int maxLives = 3;
    [Header("How many hits per combat before you lose a life")]
    [SerializeField] private int maxHitsPerCombat = 10;

    private int currentLives;
    private int hitsThisCombat;

    public int CurrentLives => currentLives;
    public int RemainingHits => maxHitsPerCombat - hitsThisCombat;
    public int MaxHitsPerCombat => maxHitsPerCombat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResetLives();
        ResetCombatMeter();

        EventManager.Instance.StartListening<int>("takeDamageEvent", OnTakeDamage);
        SceneManager.sceneLoaded += OnSceneLoaded;

        SyncUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            EventManager.Instance.StopListening<int>("takeDamageEvent", OnTakeDamage);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Battle")
            ResetCombatMeter();
    }

    private void OnTakeDamage(int damage)
    {
        if (damage <= 0 || currentLives <= 0)
            return;

        hitsThisCombat += damage;

        if (hitsThisCombat >= maxHitsPerCombat)
        {
            currentLives = Mathf.Max(0, currentLives - 1);
            hitsThisCombat = 0;
            EventManager.Instance.TriggerEvent("OnLivesChanged", currentLives);
        }

        EventManager.Instance.TriggerEvent("OnCombatMeterChanged", RemainingHits, MaxHitsPerCombat);

        if (currentLives == 0)
        {
            EventManager.Instance.TriggerEvent("OnGameOver");
            // Optionally: SceneManager.LoadScene("GameOver");
        }
    }

    private void ResetCombatMeter()
    {
        hitsThisCombat = 0;
        EventManager.Instance.TriggerEvent("OnCombatMeterChanged", RemainingHits, MaxHitsPerCombat);
    }

    private void ResetLives()
    {
        currentLives = maxLives;
    }

    private void SyncUI()
    {
        EventManager.Instance.TriggerEvent("OnLivesChanged", currentLives);
        EventManager.Instance.TriggerEvent("OnCombatMeterChanged", RemainingHits, MaxHitsPerCombat);
    }
}
