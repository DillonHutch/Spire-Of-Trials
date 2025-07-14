// CombatMeterUI.cs
using UnityEngine;
using UnityEngine.UI;

public class CombatMeterUI : MonoBehaviour
{
    [SerializeField] private Slider meterSlider;
    [SerializeField] private Image fillImage;        // drag in your Fill img
    [SerializeField] private Gradient meterGradient;  // e.g. green→red

    private void Awake()
    {
        if (fillImage == null && meterSlider != null)
            fillImage = meterSlider.fillRect.GetComponentInChildren<Image>();
    }

    private void OnEnable()
    {
        int max = HealthManager.Instance.MaxHitsPerCombat;
        meterSlider.minValue = 0;
        meterSlider.maxValue = max;

        // start full
        meterSlider.value = max;
        fillImage.color = meterGradient.Evaluate(1f);

        EventManager.Instance.StartListening<int, int>(
            "OnCombatMeterChanged",
            UpdateMeter
        );
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<int, int>(
            "OnCombatMeterChanged",
            UpdateMeter
        );
    }

    private void UpdateMeter(int remainingHits, int maxHits)
    {
        meterSlider.value = remainingHits;
        if (meterGradient != null)
        {
            float t = remainingHits / (float)maxHits;
            fillImage.color = meterGradient.Evaluate(t);
        }
    }
}
