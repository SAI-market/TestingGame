using UnityEngine;

/// <summary>
/// Offsets ViewmodelRoot from look delta (sway), strafe input (lean) and speed (bob),
/// plus spring impulses on jump/land. All additive on top of the rest pose.
/// </summary>
public class ViewmodelSway : MonoBehaviour
{
    [SerializeField] private ViewmodelSettings settings;
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerMotor motor;

    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;

    private Vector3 swayPosOffset;
    private Vector3 swayPosVelocity;
    private Vector3 swayRotOffset;
    private Vector3 swayRotVelocity;

    private Vector3 movePosOffset;
    private Vector3 movePosVelocity;

    private Vector3 impulseOffset;
    private Vector3 impulseVelocity;

    private float bobTimer;

    private void Awake()
    {
        restLocalPosition = transform.localPosition;
        restLocalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        if (motor != null)
        {
            motor.OnJumped += HandleJumped;
            motor.OnLanded += HandleLanded;
        }
    }

    private void OnDisable()
    {
        if (motor != null)
        {
            motor.OnJumped -= HandleJumped;
            motor.OnLanded -= HandleLanded;
        }
    }

    public void AddImpulse(Vector3 impulse)
    {
        impulseVelocity += impulse;
    }

    private void HandleJumped() => AddImpulse(Vector3.up * settings.jumpImpulse);

    private void HandleLanded(float impactSpeed) =>
        AddImpulse(Vector3.down * Mathf.Min(impactSpeed * settings.landImpulseScale, settings.landImpulseScale * 4f));

    private void LateUpdate()
    {
        Vector2 look = input.Look;

        Vector3 targetSwayPos = new Vector3(
            Mathf.Clamp(-look.x * settings.lookSwayAmount, -settings.lookSwayMaxOffset, settings.lookSwayMaxOffset),
            Mathf.Clamp(-look.y * settings.lookSwayAmount, -settings.lookSwayMaxOffset, settings.lookSwayMaxOffset),
            0f);

        Vector3 targetSwayRot = new Vector3(
            look.y * settings.lookSwayRotationAmount * 0.05f,
            look.x * settings.lookSwayRotationAmount * 0.05f,
            -look.x * settings.lookSwayRotationAmount * 0.05f);

        swayPosOffset = Vector3.SmoothDamp(swayPosOffset, targetSwayPos, ref swayPosVelocity, settings.lookSwaySmoothTime);
        swayRotOffset = Vector3.SmoothDamp(swayRotOffset, targetSwayRot, ref swayRotVelocity, settings.lookSwaySmoothTime);

        Vector2 moveInput = input.Move;
        Vector3 targetMovePos = new Vector3(-moveInput.x * settings.moveSwayAmount, 0f, 0f);
        movePosOffset = Vector3.SmoothDamp(movePosOffset, targetMovePos, ref movePosVelocity, settings.moveSwaySmoothTime);

        Vector3 bobOffset = Vector3.zero;
        if (motor != null)
        {
            float speed = motor.HorizontalSpeed;
            if (motor.IsGrounded && speed > 0.1f)
            {
                float amplitude = motor.IsSprinting ? settings.bobAmplitudeSprint : settings.bobAmplitudeWalk;
                bobTimer += Time.deltaTime * speed * Mathf.PI;
                bobOffset = new Vector3(
                    Mathf.Cos(bobTimer * 0.5f) * amplitude * 0.5f,
                    Mathf.Sin(bobTimer) * amplitude,
                    0f);
            }
            else
            {
                bobTimer = 0f;
            }
        }

        impulseVelocity += -impulseOffset * settings.impulseSpring * Time.deltaTime;
        impulseVelocity *= Mathf.Clamp01(1f - settings.impulseDamping);
        impulseOffset += impulseVelocity * Time.deltaTime;

        transform.localPosition = restLocalPosition + swayPosOffset + movePosOffset + bobOffset + impulseOffset;
        transform.localRotation = restLocalRotation * Quaternion.Euler(swayRotOffset);
    }
}
