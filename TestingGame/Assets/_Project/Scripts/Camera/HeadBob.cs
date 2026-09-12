using System;
using UnityEngine;

/// <summary>
/// Sits on its own transform between CameraPivot and the impulse node, so it never
/// fights CameraImpulse for control of localPosition. Amplitude/frequency scale with
/// locomotion state; amplitude is fully offline (0) when the player wants no bob at all.
/// </summary>
public class HeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerState state;

    [Header("Amplitude / Frequency by state")]
    [SerializeField] private float walkAmplitude = 0.035f;
    [SerializeField] private float walkFrequency = 0.9f;
    [SerializeField] private float sprintAmplitude = 0.07f;
    [SerializeField] private float sprintFrequency = 1.2f;
    [SerializeField] private float crouchAmplitude = 0.02f;
    [SerializeField] private float crouchFrequency = 0.7f;

    [Header("Feel")]
    [SerializeField] private float horizontalRatio = 0.5f;
    [SerializeField] private float returnToRestSpeed = 6f;
    [SerializeField, Range(0f, 1f)] private float intensity = 1f;

    public event Action OnStep;

    private float bobTimer;
    private bool wasInLowerHalf;
    private Vector3 restLocalPosition;

    private void Reset()
    {
        motor = GetComponentInParent<PlayerMotor>();
        state = GetComponentInParent<PlayerState>();
    }

    private void Awake()
    {
        restLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (motor == null)
        {
            transform.localPosition = restLocalPosition;
            return;
        }

        float speed = motor.HorizontalSpeed;
        bool grounded = motor.IsGrounded;

        LocomotionState locomotion = state != null ? state.Current : LocomotionState.Idle;
        float amplitude;
        float frequency;
        switch (locomotion)
        {
            case LocomotionState.Sprint:
                amplitude = sprintAmplitude; frequency = sprintFrequency; break;
            case LocomotionState.Crouch:
                amplitude = crouchAmplitude; frequency = crouchFrequency; break;
            default:
                amplitude = walkAmplitude; frequency = walkFrequency; break;
        }
        amplitude *= intensity;

        Vector3 targetOffset;
        if (grounded && speed > 0.1f)
        {
            bobTimer += Time.deltaTime * frequency * speed * Mathf.PI;
            float y = Mathf.Sin(bobTimer) * amplitude;
            float x = Mathf.Cos(bobTimer * 0.5f) * amplitude * horizontalRatio;
            targetOffset = new Vector3(x, y, 0f);

            bool lowerHalf = Mathf.Sin(bobTimer) < -0.6f;
            if (lowerHalf && !wasInLowerHalf) OnStep?.Invoke();
            wasInLowerHalf = lowerHalf;
        }
        else
        {
            targetOffset = Vector3.zero;
            bobTimer = 0f;
            wasInLowerHalf = false;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, restLocalPosition + targetOffset, Time.deltaTime * returnToRestSpeed);
    }
}
