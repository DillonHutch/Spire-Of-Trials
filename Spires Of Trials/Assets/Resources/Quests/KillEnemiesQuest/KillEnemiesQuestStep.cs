using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class KillEnemiesQuestStep : QuestStep
{
    [Tooltip("Must match the key used in ResourceManager")]
    [SerializeField] private string resourceName = "enemiesKilled";
    [SerializeField] private int enemiesToKill = 4;

    private int enemiesKilled;

    private void Start()
    {
        // catch up on any kills before this quest started
        enemiesKilled = ResourceManager.Instance.GetResourceCount(resourceName);
        UpdateState();

        if (enemiesKilled >= enemiesToKill)
            FinishQuestStep();
    }

    private void OnEnable()
    {
        EventManager.Instance.StartListening($"resourceAdded_{resourceName}", OnEnemyKilled);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening($"resourceAdded_{resourceName}", OnEnemyKilled);
    }

    private void OnEnemyKilled()
    {
        enemiesKilled = ResourceManager.Instance.GetResourceCount(resourceName);
        UpdateState();

        if (enemiesKilled >= enemiesToKill)
            FinishQuestStep();
    }

    private void UpdateState()
    {
        ChangeState(
            enemiesKilled.ToString(),
            $"Killed {enemiesKilled} / {enemiesToKill} enemies."
        );
    }

    protected override void SetQuestStepState(string state)
    {
        enemiesKilled = int.Parse(state);
        UpdateState();
    }

}
