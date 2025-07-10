using UnityEngine;
using TMPro;


public enum PowerType
{
    Rewind,
    SuperSpeed,
    SlowTime
}

public class PowerController : MonoBehaviour
{
    
    private PowerType[] powers = {
        PowerType.Rewind,
        PowerType.SuperSpeed,
        PowerType.SlowTime    // ← include it in the cycle
    };
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField]
    [Tooltip("How much faster SuperSpeed makes you")]
    private float superSpeedMultiplier = 3f;

    private int currentPowerIndex;
    private PlayerMovement playerMovement;
    private Animator playerAnimator;
    private float normalFixedDelta;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<Animator>();
        normalFixedDelta = Time.fixedDeltaTime;
        UpdatePowerUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CyclePower();
        }

        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftHeld) ActivatePower();
        else DeactivatePower();
    }

    void CyclePower()
    {
        currentPowerIndex = (currentPowerIndex + 1) % powers.Length;
        UpdatePowerUI();
    }

    void UpdatePowerUI()
    {
        if (powerText != null)
            powerText.text = $"Power: {powers[currentPowerIndex]}";
    }

    void ActivatePower()
    {
        switch (powers[currentPowerIndex])
        {
            case PowerType.Rewind:
                TimeController.Instance.SetRewinding(true);
                break;
            case PowerType.SuperSpeed:
                playerMovement.SetSpeedMultiplier(superSpeedMultiplier);
                break;
            case PowerType.SlowTime:
                // slow the whole game to half speed
                Time.timeScale = 0.2f;
                // keep physics steps in sync
                Time.fixedDeltaTime = normalFixedDelta * Time.timeScale;

                // cancel out slow for the player:
                // 1) movement
                playerMovement.SetSpeedMultiplier(1f / Time.timeScale);
                // 2) animations
                if (playerAnimator != null)
                    playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                break;
        }
    }

    void DeactivatePower()
    {
        switch (powers[currentPowerIndex])
        {
            case PowerType.Rewind:
                TimeController.Instance.SetRewinding(false);
                break;
            case PowerType.SuperSpeed:
                playerMovement.ResetSpeedMultiplier();
                break;
            case PowerType.SlowTime:
                // back to normal time
                Time.timeScale = 1f;
                Time.fixedDeltaTime = normalFixedDelta;

                // restore player
                playerMovement.ResetSpeedMultiplier();
                if (playerAnimator != null)
                    playerAnimator.updateMode = AnimatorUpdateMode.Normal;
                break;
        }
    }
}