using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniBoss : EnemyParent
{
   
    [SerializeField] private Transform leftSpawn;
    [SerializeField] private Transform centerSpawn;
    [SerializeField] private Transform rightSpawn;

    private Transform player;
    
    
    private Transform currentParent;
    

    Color originalColor;
    SpriteRenderer iconRenderer;
    Color iconOriginalColor;


    Coroutine knightAttackCoroutine;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    protected override void Start()
    {
        base.Start();

        iconOriginalColor = iconRenderer != null ? iconRenderer.color : Color.white;
        originalColor = spriteRenderer.color;
        iconRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Assumes the first child is the icon

        attackIntervalMin = 1f;
        attackIntervalMax = 1f;
        windUpTime = 1f;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        leftSpawn = GameObject.FindWithTag("LeftSpawn")?.transform;
        centerSpawn = GameObject.FindWithTag("MiddleSpawn")?.transform;
        rightSpawn = GameObject.FindWithTag("RightSpawn")?.transform;

        if (leftSpawn == null || centerSpawn == null || rightSpawn == null)
        {
            Debug.LogError("MiniBoss spawn positions are not properly set! Check your tags.");
            return;
        }


        SetNewParent(GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn));
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        if (knightAttackCoroutine != null)
        {
            StopCoroutine(knightAttackCoroutine);
        }
        knightAttackCoroutine = StartCoroutine(KnightAttackLoop());


    }


    private void UpdateColor()
    {
        if (currentSequenceIndex >= attackSequence.Count) return;
        if (attackIndicatorRenderer == null)
        {
           // Debug.LogError($"{gameObject.name}: attackIndicatorRenderer is NULL. Ensure attackIndicator has a SpriteRenderer.");
            return;
        }

        string nextAttack = attackSequence[currentSequenceIndex];
        switch (nextAttack)
        {
            case "melee":
                attackIndicatorRenderer.sprite = meleeSprite;
                animator.SetFloat("AttackType", 0f);
                break;
            case "magic":
                attackIndicatorRenderer.sprite = magicSprite;
                animator.SetFloat("AttackType", .33f);
                break;
            case "range":
                attackIndicatorRenderer.sprite = rangeSprite;
                animator.SetFloat("AttackType", .66f);
                break;
            case "heavy":
                attackIndicatorRenderer.sprite = heavySprite;
                animator.SetFloat("AttackType", 1f);
                break;
        }
    }


    private IEnumerator KnightAttackLoop()
    {
        while (true)
        {
            int attackBurstCount = Random.Range(5, 8); // Number of rapid attacks before resting
            for (int i = 0; i < attackBurstCount; i++)
            {
                int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
                float attackDelay = Random.Range(.5f, .5f); // Faster attack intervals

                yield return new WaitForSeconds(attackDelay); // Short delay between rapid attacks

                isAttacking = true;

                // Move MiniBoss to a random spawn point before attacking
                Transform randomSpawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
                SetNewParent(randomSpawn);

                animator.SetTrigger("WindUp"); // Start Wind-up Animation

                int miniBossTargetPos = Random.Range(0, 3); // Attack a random position
                SpriteRenderer attackSprite = null;

                if (miniBossTargetPos == 0) attackSprite = leftAttackSprite;
                else if (miniBossTargetPos == 1) attackSprite = centerAttackSprite;
                else if (miniBossTargetPos == 2) attackSprite = rightAttackSprite;

                if (attackSprite != null)
                {
                    attackSprite.enabled = true;
                    StartCoroutine(FlashAttackIndicator(attackSprite));
                }

                AudioManager.instance.PlayOneShot(FMODEvents.instance.knightWU, transform.position);
                yield return new WaitForSeconds(.5f); // Short wind-up time

                AudioManager.instance.PlayOneShot(FMODEvents.instance.knightAttack, transform.position);

                // Check if the player dodged correctly
                int updatedPlayerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
                if (updatedPlayerDodgePosition == miniBossTargetPos) // Player blocks correctly
                {
                    Debug.Log("Player successfully blocked the attack!");

                    // Stop any ongoing shield recoil to prevent bouncing issues
                    if (activeRecoilCoroutine != null) StopCoroutine(activeRecoilCoroutine);
                    if (flashCoroutine != null) StopCoroutine(flashCoroutine);

                    // Only trigger shield recoil if the shield is actually there
                    TriggerShieldRecoil(miniBossTargetPos);

                    AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);  

                }
                else
                {
                    Debug.Log("Player failed to block! Taking damage from MiniBoss.");

                    // Trigger the player's damage event instead
                    EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.playerMetal, this.transform.position);
                }


                if (dodgeBarHighlighter != null)
                {
                    dodgeBarHighlighter.ClearHighlight(miniBossTargetPos);
                }

                animator.SetTrigger("Attack"); // Attack animation

                isAttacking = false;
                UpdateColor();

                if (attackSprite != null)
                {
                    attackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);
                }

                yield return new WaitForSeconds(0.1f); // Small delay before resetting

                // **Reset to Idle before next attack**
                animator.SetTrigger("ReturnToIdle");
                yield return new WaitForSeconds(0.1f); // Give a brief pause for animation reset
            }

            // **Rest Phase** - After the burst of attacks, the MiniBoss pauses
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle"); // Reset to idle before resting
            yield return new WaitForSeconds(3f); // Rest period for punishment window

            
        }
    }


    private Transform GetSpawnFromIndex(int index)
    {
        switch (index)
        {
            case 0: return leftSpawn;
            case 1: return centerSpawn;
            case 2: return rightSpawn;
            default: return centerSpawn;
        }
    }


    new public void TakeDamage(string attackType)
    {

        PlayerAttackingScript player = FindObjectOfType<PlayerAttackingScript>(); // Find the player script

        

        int phaseSize = 4; // Each phase consists of 4 attacks
        int totalPhases = attackSequence.Count / phaseSize;

        int currentPhase = currentSequenceIndex / phaseSize; // Determine which phase the player is in
        int phaseStartIndex = currentPhase * phaseSize; // Start of the current phase
        int phaseEndIndex = phaseStartIndex + phaseSize; // End of the current phase

        if (currentSequenceIndex < attackSequence.Count && attackType == attackSequence[currentSequenceIndex])
        {
            currentSequenceIndex++;
            Debug.Log($"MiniBoss hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            AudioManager.instance.PlayOneShot(FMODEvents.instance.knightDamage, this.transform.position);

            StartCoroutine(FlashRed()); // Flash effect on hit

            if (damageParticlePrefab != null)
            {

                GameObject particles = Instantiate(damageParticlePrefab, partOrgin.transform.position, Quaternion.identity);
                Destroy(particles, 0.5f); // Cleanup after 0.5 sec

            }

            // If phase is completed, move to the next phase
            if (currentSequenceIndex >= phaseEndIndex)
            {
                Debug.Log($"Phase {currentPhase + 1} completed!");
            }

            // If all phases are completed, defeat the miniboss
            if (currentSequenceIndex >= attackSequence.Count)
            {
                Die();
            }
            else
            {
                UpdateColor();
            }

            player?.UpdateCombo(true); // **Notify the player about a successful hit**

        }
        else
        {
            Debug.Log("MiniBoss hit incorrectly! Resetting current phase.");

            // Reset only the current phase, not the entire sequence
            currentSequenceIndex = phaseStartIndex;
            UpdateColor();


            player?.UpdateCombo(false); // **Notify the player about a successful hit**
        }

        

        if (healthBar != null)
        {
            healthBar.value = attackSequence.Count - currentSequenceIndex; // Update health display
        }
    }

    

    private void Die()
    {
        Debug.Log("MiniBoss defeated!");
        if (knightAttackCoroutine != null) StopCoroutine(knightAttackCoroutine);

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (leftAttackSprite != null)
            leftAttackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);

        if (centerAttackSprite != null)
            centerAttackSprite.color = new Color(centerAttackSprite.color.r, centerAttackSprite.color.g, centerAttackSprite.color.b, 0f);

        if (rightAttackSprite != null)
            rightAttackSprite.color = new Color(rightAttackSprite.color.r, rightAttackSprite.color.g, rightAttackSprite.color.b, 0f);



        Destroy(gameObject);
    }




    private void SetNewParent(Transform newParent)
    {
        if (newParent != null)
        {

            
            currentParent = newParent;
            transform.position = currentParent.position;
            transform.SetParent(currentParent);
        }
        
    }

    private Transform GetRandomSpawn(params Transform[] positions)
    {
        return positions.Length > 0 ? positions[Random.Range(0, positions.Length)] : null;
    }

    private Transform GetPlayerPosition()
    {
        return player.position.x < centerSpawn.position.x ? leftSpawn : (player.position.x > centerSpawn.position.x ? rightSpawn : centerSpawn);
    }

   

    private int GetPositionIndex(Transform position)
    {
        if (position == leftSpawn) return 0;
        if (position == centerSpawn) return 1;
        if (position == rightSpawn) return 2;
        return 1;
    }
}
