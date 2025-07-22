using UnityEngine;
using UnityEngine.UI;

public class SpaceBarMiniGameController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Slider fillSlider;
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField, Range(0, 1f)]
    private float zoneWidth = 0.15f;
    [SerializeField, Range(0, 1f)]
    private float zoneHeight = 0.3f;
    [SerializeField]
    private float fillSpeed = 1f;

    private float zoneStart, zoneEnd;
    private bool isRunning;
    private FightController fightController;

    private void Awake()
    {
        fightController = FindObjectOfType<FightController>();
        if (fightController == null)
            Debug.LogError("SpaceBarMiniGameController: no FightController found");
    }

    public void StartSequence()
    {
        // reset slider…
        fillSlider.value = 0f;

        // pick a random horizontal zone
        zoneStart = Random.Range(0f, 1f - zoneWidth);
        zoneEnd = zoneStart + zoneWidth;

        // pick a random vertical zone
        float vertStart = Random.Range(0f, 1f - zoneHeight);
        float vertEnd = vertStart + zoneHeight;

        // apply both
        targetZone.anchorMin = new Vector2(zoneStart, vertStart);
        targetZone.anchorMax = new Vector2(zoneEnd, vertEnd);
        targetZone.offsetMin = Vector2.zero;
        targetZone.offsetMax = Vector2.zero;

        panel.SetActive(true);
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        // fill while held
        if (Input.GetKey(KeyCode.Space))
        {
            fillSlider.value += fillSpeed * Time.deltaTime;
            if (fillSlider.value >= 1f)
                Fail();
        }

        // on release, check zone
        if (Input.GetKeyUp(KeyCode.Space))
        {
            bool success = fillSlider.value >= zoneStart
                        && fillSlider.value <= zoneEnd;
            if (success)
                Succeed();
            else
                Fail();
        }
    }

    private void Succeed()
    {
        // pass **true** so your heal logic runs
        fightController.ApplySkillEffect(true);

        // resume timer & UI exactly like your arrow mini‑game does

        EndSequence();
    }



    private void Fail()
    {
        fightController.ApplySkillEffect(false);
   

        EndSequence();
    }

    private void EndSequence()
    {
        isRunning = false;
        panel.SetActive(false);
        //TimingController.Instance.StopCombatTimer();
    }
}
