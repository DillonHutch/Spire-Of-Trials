using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [SerializeField] private SpriteRenderer[] spriteRenderers; // Assign sprites in Inspector
    private Color originalColor;
    private float flashDuration = 0.2f; // Time player stays red

    private void Awake()
    {
        if (spriteRenderers.Length > 0)
        {
            originalColor = spriteRenderers[0].color; // Store original color from first sprite
        }
    }

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening("takeDamageEvent", param => TakeDamage((int)param));
            EventManager.Instance.StartListening("healDamageEvent", param => Heal((int)param));
        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening("takeDamageEvent", param => TakeDamage((int)param));
            EventManager.Instance.StopListening("healDamageEvent", param => Heal((int)param));
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (this == null) return; // Prevent execution if the player has been destroyed

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (EventManager.Instance != null) // Check if EventManager exists before triggering the event
        {
            EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
        }

        if (spriteRenderers.Length > 0)
        {
            StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private IEnumerator FlashRed()
    {
        foreach (var sprite in spriteRenderers)
        {
            sprite.color = Color.red;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (var sprite in spriteRenderers)
        {
            sprite.color = originalColor;
        }
    }

    void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
    }

    private void Die()
    {
        if (EventManager.Instance != null) // Prevent null reference when calling event
        {
            EventManager.Instance.TriggerEvent("OnPlayerDied");
        }

        currentHealth = maxHealth;
        SceneManager.LoadScene("MainMenu");
    }

}
