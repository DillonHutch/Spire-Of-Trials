using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectCoinsQuestStep : QuestStep
{

    private int coinsCollected = 0;

    private int coinsToComplete = 5;

    private void OnEnable()
    {
        EventManager.Instance.StartListening("coinCollected", CoinCollected);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening("coinCollected", CoinCollected);
    }


    private void CoinCollected()
    {

        if (coinsCollected < coinsToComplete)
        {
            coinsCollected++;
            UpdateState();
        }


        if(coinsCollected >= coinsToComplete)
        {
            FinishQuestStep();
        }


    }

    private void UpdateState()
    {

        string state = coinsCollected.ToString();
        ChangeState(state);

    }

    protected override void SetQuestStepState(string state)
    {
        this.coinsCollected = System.Int32.Parse(state);
        UpdateState();
    }


}
