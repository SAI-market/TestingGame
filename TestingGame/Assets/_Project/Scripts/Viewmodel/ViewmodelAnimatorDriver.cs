using UnityEngine;

/// <summary>
/// Writes PlayerMotor state into Animator parameters. Expects the controller to define
/// a float "Speed" (0-1), a bool "Grounded", and triggers "Jump" / "Land".
/// </summary>
public class ViewmodelAnimatorDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private float maxSpeedForBlend = 7.5f;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int GroundedParam = Animator.StringToHash("Grounded");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int LandTrigger = Animator.StringToHash("Land");

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

    private void Update()
    {
        if (animator == null || motor == null) return;
        animator.SetFloat(SpeedParam, Mathf.Clamp01(motor.HorizontalSpeed / maxSpeedForBlend));
        animator.SetBool(GroundedParam, motor.IsGrounded);
    }

    private void HandleJumped()
    {
        if (animator != null) animator.SetTrigger(JumpTrigger);
    }

    private void HandleLanded(float impactSpeed)
    {
        if (animator != null) animator.SetTrigger(LandTrigger);
    }
}
