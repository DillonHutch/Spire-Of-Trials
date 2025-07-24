//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

///// <summary>
///// Keeps track of players health 
///// </summary>
//public class PlayerHealth : MonoBehaviour
//{

//    #region Fields

//    public static PlayerHealth Instance { get; private set; }

//    // **Health Properties**
//    private int maxHealth = 100; // Maximum health of the player
//    private int currentHealth; // Current health of the player, dynamically updated

//    // **Visual Feedback**
//    [SerializeField] private SpriteRenderer[] spriteRenderers; // Array of sprite renderers (assigned in Inspector)
//    private Color originalColor; // Stores the player's original color for flashing effect
    


//    private int damageTakenThisRound = 0;


//    public int MaxHealth
//    {
//        get { return maxHealth; }
//    }

//    public int CurrentHealth
//    {
//        get { return currentHealth; }
//    }

//    #endregion

//    #region UnityMethods

//    /// <summary>
//    /// Called when the script instance is being loaded.
//    /// Stores the player's original color for later use in visual feedback (flashing effect).
//    /// </summary>
//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//            // only initialize health once
//            currentHealth = maxHealth;
//            SceneManager.sceneLoaded += OnSceneLoaded;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }


//    private void OnDestroy()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }


//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {


//        // force the UI to re-sync
//        EventManager.Instance.TriggerEvent(
//            "OnHealthChanged",
//            currentHealth
//        );
//    }


//    /// <summary>
//    /// Called when the object is enabled.
//    /// Subscribes to the damage and healing events to update the player's health accordingly.
//    /// </summary>
//    private void OnEnable()
//    {
//        if (EventManager.Instance != null)
//        {
//            // Listen for damage and healing events, ensuring the correct methods are called when triggered
//            EventManager.Instance.StartListening<int>("takeDamageEvent", TakeDamage);
//            EventManager.Instance.StartListening<int>("healDamageEvent", Heal);
//            EventManager.Instance.StartListening("OnStartFight", ResetRoundDamage);
//        }
//        else
//        {
//            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
//        }
//    }

//    /// <summary>
//    /// Called when the object is disabled.
//    /// Unsubscribes from the damage and healing events to prevent memory leaks.
//    /// </summary>
//    private void OnDisable()
//    {
//        if (EventManager.Instance != null)
//        {
//            EventManager.Instance.StopListening<int>("takeDamageEvent", TakeDamage);
//            EventManager.Instance.StopListening<int>("healDamageEvent", Heal);
//            EventManager.Instance.StopListening("OnStartFight", ResetRoundDamage);
//        }
//    }


//    private void ResetRoundDamage()
//    {
//        damageTakenThisRound = 0;
//    }

//    /// <summary>
//    /// Called when the script starts.
//    /// Initializes the player's health and triggers an event to update the UI.
//    /// </summary>
//    private void Start()
//    {
//        currentHealth = maxHealth; // Set the player's health to the maximum at the start of the game

//        // Notify the system that the player's health has been initialized
//        if (Instance == this)
//            EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
//    }


//    #endregion

//    #region PlayerHealthMethods

//    /// <summary>
//    /// Reduces the player's health when taking damage.
//    /// Triggers health update events, applies visual feedback, and checks for death.
//    /// </summary>
//    /// <param name="damage">The amount of damage to apply.</param>
//    void TakeDamage(int damage)
//    {
//        if (this == null) return; // Prevent execution if the player object has been destroyed

//        damageTakenThisRound += damage;

//        // Reduce the player's health and ensure it doesn't go below 0
//        currentHealth -= damage;
//        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

//        // Trigger the event to update UI and other systems if EventManager exists
//        if (EventManager.Instance != null)
//        {
//            EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
//        }

    
//        // Check if the player has run out of health
//        if (currentHealth <= 0)
//        {
//            Die();
//        }
//    }

    
//    /// <summary>
//    /// Heals the player by the specified amount and ensures health does not exceed the maximum.
//    /// Triggers an event to update the UI.
//    /// </summary>
//    /// <param name="amount">The amount of health to restore.</param>
//    void Heal(int amount)
//    {
//        // Increase the player's health and ensure it doesn't exceed the max health
//        currentHealth += amount;
//        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

//        // Trigger the event to update UI and other systems
//        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
//    }

//    /// <summary>
//    /// Handles player death by triggering an event and resetting the game.
//    /// Loads the Main Menu upon death.
//    /// </summary>
//    private void Die()
//    {
//        // Trigger the "OnPlayerDied" event if the EventManager exists
//        if (EventManager.Instance != null)
//        {
//            EventManager.Instance.TriggerEvent("OnPlayerDied");
//        }

//        // Reset health before restarting the game
//        currentHealth = maxHealth;

//        RoundManager.ROUND_NUMBER = 0;

//        // Load the main menu scene upon death
//        SceneManager.LoadScene("DeathScreen");
//    }


//    /// <summary>
//    /// How much damage the player took during the just‐finished round
//    /// </summary>
//    public int GetDamageTakenThisRound()
//    {
//        return damageTakenThisRound;
//    }

//    #endregion

//}
