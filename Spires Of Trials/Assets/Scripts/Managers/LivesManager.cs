// LivesManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Lives Settings")]
    [SerializeField] private int maxLives = 10;
    [SerializeField] private int maxHitsPerCombat = 10;

    public int CurrentLives { get; private set; }
    public int HitsThisCombat { get; private set; }
    public int MaxHitsPerCombat { get { return maxHitsPerCombat; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentLives = maxLives;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        EventManager em = EventManager.Instance;
        //em.StartListening("OnStartFight", ResetCombatMeter);
        em.StartListening<int>("takeDamageEvent", OnHit);
    }

    private void OnDisable()
    {
        EventManager em = EventManager.Instance;
       // em.StopListening("OnStartFight", ResetCombatMeter);
        em.StopListening<int>("takeDamageEvent", OnHit);
    }

    private void ResetCombatMeter()
    {
        HitsThisCombat = 0;
        EventManager.Instance.TriggerEvent(
            "OnCombatMeterChanged",
            HitsThisCombat,
            maxHitsPerCombat
        );
    }

    // LivesManager.cs
    private void OnHit(int damage)
    {
        // skip any “free” or zero‐damage hits
        if (damage <= 0)
            return;

        HitsThisCombat++;
        EventManager.Instance.TriggerEvent(
            "OnCombatMeterChanged",
            HitsThisCombat,
            maxHitsPerCombat
        );

        if (HitsThisCombat >= maxHitsPerCombat)
        {
            LoseOneLife();
            ResetCombatMeter();
        }
    }


    private void LoseOneLife()
    {
        CurrentLives--;
        EventManager.Instance.TriggerEvent("OnLivesChanged", CurrentLives);

        if (CurrentLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        SceneManager.LoadScene("DeathScreen");
    }
}
