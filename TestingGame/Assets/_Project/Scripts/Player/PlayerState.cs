using UnityEngine;

public enum LocomotionState
{
    Idle,
    Walk,
    Sprint,
    Crouch,
    Air
}

/// <summary>
/// Reads PlayerMotor/PlayerCrouch and exposes one unified state enum so consumers
/// (animator driver, footstep audio, head bob) don't need references to both.
/// </summary>
public class PlayerState : MonoBehaviour
{
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerCrouch crouch;

    public bool IsGrounded => motor.IsGrounded;
    public float HorizontalSpeed => motor.HorizontalSpeed;
    public bool IsSprinting => motor.IsSprinting;
    public bool IsCrouching => crouch != null && crouch.IsCrouching;

    public LocomotionState Current { get; private set; }

    private void Reset()
    {
        motor = GetComponent<PlayerMotor>();
        crouch = GetComponent<PlayerCrouch>();
    }

    private void Awake()
    {
        if (motor == null) motor = GetComponent<PlayerMotor>();
        if (crouch == null) crouch = GetComponent<PlayerCrouch>();
    }

    private void LateUpdate()
    {
        if (!IsGrounded) Current = LocomotionState.Air;
        else if (IsCrouching) Current = LocomotionState.Crouch;
        else if (IsSprinting && HorizontalSpeed > 0.1f) Current = LocomotionState.Sprint;
        else if (HorizontalSpeed > 0.1f) Current = LocomotionState.Walk;
        else Current = LocomotionState.Idle;
    }
}
