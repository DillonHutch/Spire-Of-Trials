using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Devil : EnemyParent
{

    int attackNumber;
    
    protected override int GetAttackPosition()
    {

        int attackPos;
        if (enemyAttackPosition == 0)
        {
            attackPos = Random.Range(1, 3);
            if(attackPos == 1)
            {
                attackNumber = 0;
            }else if(attackPos == 2)
            {
                attackNumber = 1;
            }
            Debug.LogError(attackNumber);
            spriteRenderer.flipX = true;
            return attackPos;
        }



        if (enemyAttackPosition == 2)
        {
            attackPos = Random.Range(0, 2);
            if (attackPos == 0)
            {
                attackNumber = 1;
            }
            else if (attackPos == 1)
            {
                attackNumber = 0;
            }

            Debug.LogError(attackNumber);
            return attackPos;
        }




        return 1;

    }

    protected override void Update()
    {
        base.Update();

        switch (attackNumber)
        {
            case 0:
                animator.SetFloat("AttackDistance", 0f);
                break;
            case 1:
                animator.SetFloat("AttackDistance", 1f);
                break;


        }

    }


    /// <summary>
    /// Defines the attack sequence for the Goblin.
    /// The sequence follows a rotating pattern of "magic", "range", "heavy", and "melee" attacks.
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string> {
                    "heavy", "melee", "magic", "range",   // Combo 1  
                    "magic", "range", "melee", "heavy",   // Combo 2  
                    "range", "melee", "heavy", "magic"    // Combo 3  

                };
    }


}
