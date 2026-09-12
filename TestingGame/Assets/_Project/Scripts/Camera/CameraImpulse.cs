using UnityEngine;

/// <summary>
/// Spring-damped position/rotation kick, decoupled from HeadBob's transform so the two
/// never overwrite each other. Auto-fires from PlayerMotor's jump/land events; anything
/// else (weapon recoil, explosions) can call AddImpulse directly.
/// </summary>
public class CameraImpulse : MonoBehaviour
{
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private float spring = 120f;
    [SerializeField] private float damping = 12f;
    [SerializeField] private Vector3 jumpKick = new Vector3(0f, 0.04f, 0f);
    [SerializeField] private float landKickPerImpactSpeed = 0.01f;
    [SerializeField] private float maxLandKick = 0.12f;

    private Vector3 posOffset;
    private Vector3 posVelocity;
    private Vector3 rotOffset;
    private Vector3 rotVelocity;
    private Vector3 restLocalPosition;

    private void Reset()
    {
        motor = GetComponentInParent<PlayerMotor>();
    }

    private void Awake()
    {
        restLocalPosition = transform.localPosition;
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

    public void AddImpulse(Vector3 positionKick, Vector3 rotationKick = default)
    {
        posVelocity += positionKick;
        rotVelocity += rotationKick;
    }

    private void HandleJumped() => AddImpulse(-jumpKick);

    private void HandleLanded(float impactSpeed) =>
        AddImpulse(Vector3.down * Mathf.Min(impactSpeed * landKickPerImpactSpeed, maxLandKick));

    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        posVelocity += -posOffset * spring * dt;
        posVelocity *= Mathf.Clamp01(1f - damping * dt);
        posOffset += posVelocity * dt;

        rotVelocity += -rotOffset * spring * dt;
        rotVelocity *= Mathf.Clamp01(1f - damping * dt);
        rotOffset += rotVelocity * dt;

        transform.localPosition = restLocalPosition + posOffset;
        transform.localRotation = Quaternion.Euler(rotOffset);
    }
}
