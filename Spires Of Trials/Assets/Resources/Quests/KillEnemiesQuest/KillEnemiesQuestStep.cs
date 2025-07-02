using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class KillEnemiesQuestStep : QuestStep
{
    int enemiesKilled = 0;
    int enemiesToKill = 4;


    private void OnEnable()
    {
        EventManager.Instance.StartListening("killEnemies", KillEnemies);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening("killEnemies", KillEnemies);
    }

    private void UpdateState()
    {

        string state = enemiesKilled.ToString();
        string status = "Killed " + enemiesKilled + " / " + enemiesToKill + " enemies.";
        ChangeState(state, status);

    }

    protected override void SetQuestStepState(string state)
    {
        this.enemiesKilled = System.Int32.Parse(state);
        UpdateState();
    }


    private void KillEnemies()
    {

        if (enemiesKilled < enemiesToKill)
        {
            enemiesKilled++;
            UpdateState();
        }


        if (enemiesKilled >= enemiesToKill)
        {
            FinishQuestStep();
        }


    }

}
