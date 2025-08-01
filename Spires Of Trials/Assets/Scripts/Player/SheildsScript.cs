using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheildsScript : MonoBehaviour
{
    [SerializeField] private Transform leftShield;
    [SerializeField] private Transform centerShield;
    [SerializeField] private Transform rightShield;

    private Dictionary<Transform, Coroutine> activeRecoils = new Dictionary<Transform, Coroutine>();


    //private Dictionary<SpriteRenderer, Coroutine> flashCoroutines = new Dictionary<SpriteRenderer, Coroutine>();


    float warningOpacity = 0.75f;
   // public float flashTime = 0.2f;


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

        // listen for the fight‑start and fight‑end events
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

    public void ShowIndicator(SpriteRenderer sr)
    {
        Debug.Log("Showing shield indicator: " + sr.name);
        sr.gameObject.SetActive(true);
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
        foreach (var recoiler in activeRecoils.Values)
            StopCoroutine(recoiler);

        activeRecoils.Clear();
        ResetShieldPositions();
    }



    public void CancelAllShieldEffects()
    {


        //reset positions and hide
        ResetShieldPositions();
        SetShieldsActive(false);
    }

}
