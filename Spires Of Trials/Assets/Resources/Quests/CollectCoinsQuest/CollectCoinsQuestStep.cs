using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectCoinsQuestStep : QuestStep
{

    [Tooltip("Name must match the key you use in ResourceManager")]
    [SerializeField] private string resourceName;

    [Tooltip("How many of that resource to collect")]
    [SerializeField] private int targetAmount = 5;

    private int currentAmount;

    void Start()
    {
        // catch up on anything collected before this quest started
        currentAmount = ResourceManager.Instance.GetResourceCount(resourceName);
        UpdateState();

        if (currentAmount >= targetAmount)
            FinishQuestStep();
    }

    void OnEnable()
    {
        // listen for only this resource
        EventManager.Instance.StartListening($"resourceAdded_{resourceName}", OnResourceAdded);
    }

    void OnDisable()
    {
        EventManager.Instance.StopListening($"resourceAdded_{resourceName}", OnResourceAdded);
    }

    private void OnResourceAdded()
    {
        currentAmount = ResourceManager.Instance.GetResourceCount(resourceName);
        UpdateState();

        if (currentAmount >= targetAmount)
            FinishQuestStep();
    }

    private void UpdateState()
    {
        ChangeState(
            currentAmount.ToString(),
            $"Collected {currentAmount} / {targetAmount} {resourceName}"
        );
    }

    protected override void SetQuestStepState(string state)
    {
        currentAmount = int.Parse(state);
        UpdateState();
    }


}
