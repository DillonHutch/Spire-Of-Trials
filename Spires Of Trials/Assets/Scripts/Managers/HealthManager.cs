// HealthManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Configure how many lives you start with")]
    [SerializeField] private int maxLives = 3;
    [Header("How many hits per combat before you lose a life")]
    private int maxHitsPerCombat;

    private int currentLives;
    private int hitsThisCombat;

    public int CurrentLives => currentLives;
    public int RemainingHits => maxHitsPerCombat - hitsThisCombat;
    public int MaxHitsPerCombat => maxHitsPerCombat;  

    public int MaxLives => maxLives;

    private void OnEnable()
    {
        EventManager.Instance.StartListening<int>("healDamageEvent", OnHealDamage);
        EventManager.Instance.StartListening<int>("takeDamageEvent", OnTakeDamage);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<int>("healDamageEvent", OnHealDamage);
        EventManager.Instance.StopListening<int>("takeDamageEvent", OnTakeDamage);
    }

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

     
        SceneManager.sceneLoaded += OnSceneLoaded;

        SyncUI();
    }

    private void Start()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        maxHitsPerCombat = (int)playerStats.defense; // Assuming defense is the number of hits you can take before losing a life
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            OnTakeDamage(1);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            OnHealDamage(1);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
           
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


    private void OnHealDamage(int healAmount)
    {
        if (healAmount <= 0 || currentLives >= maxLives)
            return;
          
        currentLives = Mathf.Max(0, currentLives + 1);
        EventManager.Instance.TriggerEvent("OnLivesChanged", currentLives);
        
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

    public void RestoreHits(int amount)
    {
        // only heal at most what you've actually taken
        int heal = Mathf.Min(amount, hitsThisCombat);
        hitsThisCombat -= heal;
        EventManager.Instance.TriggerEvent("OnCombatMeterChanged", RemainingHits, MaxHitsPerCombat);
        Debug.Log($"Healed {heal} hits, meter is now at {RemainingHits}/{MaxHitsPerCombat}");
    }


}
