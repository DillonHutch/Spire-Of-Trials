using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraLookController : MonoBehaviour
{
    [Tooltip("Maximum world-space offset from player when looking")]
    [SerializeField] private float maxOffset = 3f;
    [Tooltip("How fast the camera recentres")]
    [SerializeField] private float smoothSpeed = 8f;

    private CinemachineFramingTransposer composer;
    private Vector3 baseOffset;

    void Awake()
    {
        var vcam = GetComponent<CinemachineVirtualCamera>();
        composer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        baseOffset = composer.m_TrackedObjectOffset;
    }

    void LateUpdate()
    {
        // only when CameraLook is active and Shift held
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCameraLook = PowerController.Instance.CurrentPower == PowerType.CameraLook;

        Vector3 targetOffset = baseOffset;
        if (isCameraLook && shift)
        {
            // mouse delta from centre, normalized to [-1..1]
            Vector2 mouse = Input.mousePosition;
            Vector2 centre = new Vector2(Screen.width * .5f, Screen.height * .5f);
            Vector2 norm = (mouse - centre) / centre;
            norm = Vector2.ClampMagnitude(norm, 1f);

            // apply that to world-space offset
            targetOffset += new Vector3(norm.x * maxOffset,
                                        norm.y * maxOffset,
                                        0f);
        }

        // smooth between current and target
        composer.m_TrackedObjectOffset = Vector3.Lerp(
            composer.m_TrackedObjectOffset,
            targetOffset,
            Time.deltaTime * smoothSpeed
        );
    }
}
