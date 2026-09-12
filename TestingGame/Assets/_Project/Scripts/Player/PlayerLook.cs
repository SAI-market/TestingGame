using UnityEngine;

/// <summary>
/// Mouse/gamepad look. Yaw rotates the Player root (so transform.forward/right stay correct
/// for movement), pitch rotates only the camera pivot and is clamped.
/// </summary>
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField] private LookSettings settings;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerInputReader input;

    private float yaw;
    private float pitch;
    private float smoothedDeltaX;
    private float smoothedDeltaY;
    private float smoothVelX;
    private float smoothVelY;

    private void Reset()
    {
        input = GetComponent<PlayerInputReader>();
    }

    private void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputReader>();
    }

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = cameraPivot != null ? NormalizePitch(cameraPivot.localEulerAngles.x) : 0f;
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 look = input.Look;

        bool isMouse = input.LastLookWasMouse;
        float deviceTimeScale = isMouse ? 1f : Time.deltaTime;

        float rawDeltaX = look.x * (isMouse ? settings.sensitivityX : settings.gamepadSensitivityX) * deviceTimeScale;
        float rawDeltaY = look.y * (isMouse ? settings.sensitivityY : settings.gamepadSensitivityY) * deviceTimeScale;
        if (settings.invertY) rawDeltaY = -rawDeltaY;

        float deltaX;
        float deltaY;
        if (settings.smoothingTime > 0f)
        {
            smoothedDeltaX = Mathf.SmoothDamp(smoothedDeltaX, rawDeltaX, ref smoothVelX, settings.smoothingTime);
            smoothedDeltaY = Mathf.SmoothDamp(smoothedDeltaY, rawDeltaY, ref smoothVelY, settings.smoothingTime);
            deltaX = smoothedDeltaX;
            deltaY = smoothedDeltaY;
        }
        else
        {
            deltaX = rawDeltaX;
            deltaY = rawDeltaY;
        }

        yaw += deltaX;
        pitch = Mathf.Clamp(pitch - deltaY, settings.minPitch, settings.maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private static float NormalizePitch(float angle) => angle > 180f ? angle - 360f : angle;
}
