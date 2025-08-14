using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SheildsScript : MonoBehaviour
{
    [SerializeField] private Transform leftShield;
    [SerializeField] private Transform centerShield;
    [SerializeField] private Transform rightShield;

    private Dictionary<Transform, Coroutine> activeRecoils = new Dictionary<Transform, Coroutine>();

    float warningOpacity = 0.75f;

    [Header("Parry Motion Settings")]
    [Tooltip("How high the shields pop up when you parry.")]
    public float parryRecoilHeight = 0.5f;
    [Tooltip("How long (seconds) to return back down.")]
    public float parryReturnDuration = 1f;

    // Tracks whether we’re mid-descent
    private bool parryInProgress = false;
    public bool ParryInProgress => parryInProgress;

    // Cache originals
    private Vector3 leftOrig, centerOrig, rightOrig;

    private GameObject leftGO, centerGO, rightGO;

    private readonly Dictionary<SpriteRenderer, EnemyParent> indicatorOwners =
        new Dictionary<SpriteRenderer, EnemyParent>();

    void Awake()
    {
        leftOrig = leftShield.localPosition;
        centerOrig = centerShield.localPosition;
        rightOrig = rightShield.localPosition;

        leftGO = leftShield.gameObject;
        centerGO = centerShield.gameObject;
        rightGO = rightShield.gameObject;

        // hide them right away
        SetShieldsActive(false);

        // listen for the fight-start and fight-end events
        EventManager.Instance.StartListening("OnStartFight", OnBattleStart);
        EventManager.Instance.StartListening("OnStopFight", OnBattleEnd);
    }

    void OnDestroy()
    {
        EventManager.Instance.StopListening("OnStartFight", OnBattleStart);
        EventManager.Instance.StopListening("OnStopFight", OnBattleEnd);
    }

    private void OnBattleStart()
    {
        //SetShieldsActive(true);
    }

    private void OnBattleEnd()
    {
        SetShieldsActive(false);
    }

    private void SetShieldsActive(bool active)
    {
        leftGO.SetActive(active);
        centerGO.SetActive(active);
        rightGO.SetActive(active);
    }

    /// <summary>
    /// Call this whenever the player presses Space to parry.
    /// </summary>
    public void TriggerGlobalParry()
    {
        if (!parryInProgress)
            StartCoroutine(GlobalParryCoroutine());
    }

    private IEnumerator GlobalParryCoroutine()
    {
        parryInProgress = true;
        Vector3 upOffset = Vector3.up * parryRecoilHeight;

        // snap all shields to “up” position
        leftShield.localPosition = leftOrig + upOffset;
        centerShield.localPosition = centerOrig + upOffset;
        rightShield.localPosition = rightOrig + upOffset;

        // slowly lerp them back down over parryReturnDuration
        float elapsed = 0f;
        while (elapsed < parryReturnDuration)
        {
            float t = elapsed / parryReturnDuration;
            leftShield.localPosition = Vector3.Lerp(leftOrig + upOffset, leftOrig, t);
            centerShield.localPosition = Vector3.Lerp(centerOrig + upOffset, centerOrig, t);
            rightShield.localPosition = Vector3.Lerp(rightOrig + upOffset, rightOrig, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure exact reset
        leftShield.localPosition = leftOrig;
        centerShield.localPosition = centerOrig;
        rightShield.localPosition = rightOrig;

        parryInProgress = false;
    }

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

    public void InitializeShields(Transform left, Transform center, Transform right)
    {
        leftShield = left;
        centerShield = center;
        rightShield = right;
    }

    private Transform GetShieldByPosition(int position)
    {
        switch (position)
        {
            case 0: return leftShield;
            case 1: return centerShield;
            case 2: return rightShield;
            default: return null;
        }
    }

    public void ResetShieldPositions()
    {
        if (leftShield != null) leftShield.localPosition = new Vector3(-11f, -10f, 0f);
        if (centerShield != null) centerShield.localPosition = new Vector3(0f, -10f, 0f);
        if (rightShield != null) rightShield.localPosition = new Vector3(11f, -10f, 0f);
    }

    public void ShowIndicator(SpriteRenderer sr, EnemyParent owner)
    {
        if (sr == null) return;
        indicatorOwners[sr] = owner;       // record owner
        sr.gameObject.SetActive(true);
    }

    // Hide a specific indicator only if this enemy owns it
    public void HideIndicatorForOwner(EnemyParent owner, SpriteRenderer sr)
    {
        if (sr == null) return;
        if (indicatorOwners.TryGetValue(sr, out EnemyParent currentOwner) && currentOwner == owner)
        {
            sr.gameObject.SetActive(false);
            indicatorOwners.Remove(sr);
        }
    }

    // Hide any indicators this enemy owns, without touching others
    public void HideAllOwnedIndicators(EnemyParent owner)
    {
        List<SpriteRenderer> toClear = indicatorOwners
            .Where(kv => kv.Value == owner)
            .Select(kv => kv.Key)
            .ToList();

        foreach (SpriteRenderer sr in toClear)
        {
            if (sr != null) sr.gameObject.SetActive(false);
            indicatorOwners.Remove(sr);
        }
    }

    public void HideAllIndicators()
    {
        leftShield.gameObject.SetActive(false);
        centerShield.gameObject.SetActive(false);
        rightShield.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // stop any ongoing recoil coroutines
        foreach (Coroutine recoiler in activeRecoils.Values)
            StopCoroutine(recoiler);

        activeRecoils.Clear();
        ResetShieldPositions();

        // indicators may be shared; do not blanket disable them here
        // just forget ownership so destroyed objects don't linger in the map
        List<SpriteRenderer> nullKeys = indicatorOwners.Keys.Where(sr => sr == null).ToList();
        foreach (SpriteRenderer k in nullKeys)
        {
            indicatorOwners.Remove(k);
        }
    }

    public void CancelAllShieldEffects()
    {
        //reset positions and hide
        ResetShieldPositions();
        //SetShieldsActive(false);
    }
}
