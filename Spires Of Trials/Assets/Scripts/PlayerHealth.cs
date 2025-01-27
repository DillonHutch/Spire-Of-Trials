using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    private int currentHealth;


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

        // Notify UI about initial health
        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify UI about health change
        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify UI about health change
        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        EventManager.Instance.TriggerEvent("OnPlayerDied");
    }
}
