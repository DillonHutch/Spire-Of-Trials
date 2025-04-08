using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheildsScript : MonoBehaviour
{
    [SerializeField] private Transform leftShield;
    [SerializeField] private Transform centerShield;
    [SerializeField] private Transform rightShield;

    private Coroutine activeRecoilCoroutine;
    private bool isRecoiling = false;

    public float warningOpacity = 0.5f;
    public float flashTime = 0.2f;

    /// <summary>
    /// Triggers recoil effect on the given shield position (0=left, 1=center, 2=right)
    /// </summary>
    public void TriggerShieldRecoil(int position, MonoBehaviour caller)
    {
        Transform shieldToRecoil = GetShieldByPosition(position);
        if (shieldToRecoil != null && shieldToRecoil.gameObject.activeSelf && !isRecoiling)
        {
            isRecoiling = true;
            activeRecoilCoroutine = caller.StartCoroutine(ShieldRecoil(shieldToRecoil));
        }
    }

    private IEnumerator ShieldRecoil(Transform shield)
    {
        Vector3 originalPosition = shield.position;
        Vector3 recoilPosition = originalPosition + new Vector3(0, -0.2f, 0);

        shield.position = recoilPosition;
        yield return new WaitForSeconds(0.1f);
        shield.position = originalPosition;

        isRecoiling = false;
        activeRecoilCoroutine = null;
    }

    public IEnumerator FlashAttackIndicator(SpriteRenderer attackSprite, MonoBehaviour caller)
    {
        if (attackSprite == null) yield break;

        attackSprite.gameObject.SetActive(true);
        attackSprite.enabled = true;

        yield return caller.StartCoroutine(FlashRoutine(attackSprite));
    }

    private IEnumerator FlashRoutine(SpriteRenderer attackSprite)
    {
        Color originalColor = attackSprite.color;
        for (int i = 0; i < 3; i++)
        {
            attackSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, warningOpacity);
            yield return new WaitForSeconds(flashTime);
            attackSprite.color = originalColor;
            yield return new WaitForSeconds(flashTime);
        }

        // Ensure the indicator is turned off completely at the end
        attackSprite.enabled = false;
        attackSprite.color = originalColor; // Reset to original
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

}
