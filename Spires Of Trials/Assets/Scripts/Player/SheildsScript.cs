using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheildsScript : MonoBehaviour
{
    [SerializeField] private Transform leftShield;
    [SerializeField] private Transform centerShield;
    [SerializeField] private Transform rightShield;

    private Dictionary<Transform, Coroutine> activeRecoils = new Dictionary<Transform, Coroutine>();

    private bool isRecoiling = false;

     float warningOpacity = 0.75f;
    public float flashTime = 0.2f;

    /// <summary>
    /// Triggers recoil effect on the given shield position (0=left, 1=center, 2=right)
    /// </summary>
    public void TriggerShieldRecoil(int position, MonoBehaviour caller)
    {
        Transform shieldToRecoil = GetShieldByPosition(position);

        if (shieldToRecoil != null && shieldToRecoil.gameObject.activeSelf)
        {
            // Check if that shield is already recoiling
            if (!activeRecoils.ContainsKey(shieldToRecoil))
            {
                Coroutine coroutine = caller.StartCoroutine(ShieldRecoil(shieldToRecoil));
                activeRecoils.Add(shieldToRecoil, coroutine);
            }
        }
    }


    private IEnumerator ShieldRecoil(Transform shield)
    {
        // capture original local pos
        Vector3 originalLocalPos = shield.localPosition;
        Vector3 recoilLocalPos = originalLocalPos + new Vector3(0, -0.5f, 0);

        try
        {
            shield.localPosition = recoilLocalPos;
            yield return new WaitForSeconds(0.1f);
        }
        finally
        {
            // always restore, even if the coroutine is stopped early
            shield.localPosition = originalLocalPos;
            activeRecoils.Remove(shield);
        }
    }


    // ShieldsScript
    public IEnumerator FlashAttackIndicator(SpriteRenderer attackSprite)
    {
        if (attackSprite == null) yield break;

        // show it
        attackSprite.gameObject.SetActive(true);
        attackSprite.enabled = true;
        Color originalColor = attackSprite.color;

        // flash 3 times
        for (int i = 0; i < 3; i++)
        {
            // semi‑opaque
            attackSprite.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                warningOpacity
            );
            yield return new WaitForSeconds(flashTime);

            // back to normal
            attackSprite.color = originalColor;
            yield return new WaitForSeconds(flashTime);
        }

        // now hide it
        attackSprite.color = originalColor;
        attackSprite.enabled = false;
        attackSprite.gameObject.SetActive(false);
    }


    public void InitializeShields(Transform left, Transform center, Transform right)
    {
        leftShield = left;
        centerShield = center;
        rightShield = right;
    }

    private Transform GetShieldByPosition(int position)
    {
        return position switch
        {
            0 => leftShield,
            1 => centerShield,
            2 => rightShield,
            _ => null
        };
    }

    public void ResetShieldPositions()
    {
        if (leftShield != null) leftShield.localPosition = new Vector3(-11f, -9.92f, 0f);
        if (centerShield != null) centerShield.localPosition = new Vector3(0f, -9.92f, 0f);
        if (rightShield != null) rightShield.localPosition = new Vector3(11f, -9.92f, 0f);
    }


    private void OnDisable()
    {
        // stop any ongoing recoil coroutines
        foreach (var recoiler in activeRecoils.Values)
            StopCoroutine(recoiler);

        activeRecoils.Clear();
        ResetShieldPositions();
    }

}
