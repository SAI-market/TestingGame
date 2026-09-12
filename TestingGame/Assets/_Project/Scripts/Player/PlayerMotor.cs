using System;
using UnityEngine;

/// <summary>
/// Single source of truth for player velocity and grounded state. Nobody else calls
/// CharacterController.Move(); everything else reads IsGrounded / Velocity / HorizontalSpeed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private MovementSettings settings;
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerCrouch crouch;

    private CharacterController controller;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool isGrounded;
    private bool wasGrounded;
    private Vector3 groundNormal = Vector3.up;
    private float groundAngle;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpCutApplied;

    public bool IsGrounded => isGrounded;
    public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
    public float HorizontalSpeed => horizontalVelocity.magnitude;
    public bool IsSprinting { get; private set; }

    public event Action OnJumped;
    public event Action<float> OnLanded;

    private void Reset()
    {
        input = GetComponent<PlayerInputReader>();
        crouch = GetComponent<PlayerCrouch>();
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (input == null) input = GetComponent<PlayerInputReader>();
        if (crouch == null) crouch = GetComponent<PlayerCrouch>();
    }

    private void Update()
    {
        float incomingVerticalVelocity = verticalVelocity;

        CheckGround();
        UpdateTimers();

        Vector2 moveInput = input.Move;
        bool crouching = crouch != null && crouch.IsCrouching;
        bool wantsSprint = input.SprintHeld && !crouching && moveInput.y > 0.1f;
        IsSprinting = wantsSprint;

        float targetSpeed = crouching ? settings.crouchSpeed : (IsSprinting ? settings.sprintSpeed : settings.walkSpeed);

        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();

        bool steepSlope = isGrounded && groundAngle > settings.maxSlopeAngle;

        if (steepSlope)
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, slideDir * settings.slideSpeed, settings.groundAcceleration * Time.deltaTime);
        }
        else
        {
            Vector3 targetVelocity = wishDir * targetSpeed;
            bool hasInput = wishDir.sqrMagnitude > 0.0001f;
            float accel = isGrounded
                ? (hasInput ? settings.groundAcceleration : settings.groundFriction)
                : (hasInput ? settings.airAcceleration : settings.airFriction);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, accel * Time.deltaTime);
        }

        // Gravity
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            jumpCutApplied = false;
        }
        else
        {
            float gravityMultiplier = verticalVelocity > 0f ? settings.risingGravityMultiplier : settings.fallingGravityMultiplier;
            verticalVelocity += settings.gravity * gravityMultiplier * Time.deltaTime;
        }

        // Jump buffer
        if (input.JumpPressedThisFrame) jumpBufferTimer = settings.jumpBuffer;

        // Jump execution
        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !steepSlope)
        {
            verticalVelocity = Mathf.Sqrt(2f * settings.jumpHeight * -settings.gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpCutApplied = false;
            OnJumped?.Invoke();
        }

        // Variable jump height: cut the upward velocity once if the button is released early
        if (!input.JumpHeld && verticalVelocity > 0f && !jumpCutApplied)
        {
            verticalVelocity *= settings.jumpCutMultiplier;
            jumpCutApplied = true;
        }

        Vector3 motion = (horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
        controller.Move(motion);

        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

        if (isGrounded && !wasGrounded)
        {
            OnLanded?.Invoke(Mathf.Abs(incomingVerticalVelocity));
        }
        wasGrounded = isGrounded;
    }

    private void CheckGround()
    {
        float radius = Mathf.Max(0.01f, controller.radius - controller.skinWidth);
        float bottomLocalY = controller.center.y - (controller.height * 0.5f - controller.radius);
        Vector3 origin = transform.position + new Vector3(controller.center.x, bottomLocalY, controller.center.z);
        float castDistance = settings.groundCheckDistance + controller.skinWidth;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, castDistance, settings.groundMask, QueryTriggerInteraction.Ignore))
        {
            groundNormal = hit.normal;
            groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            isGrounded = true;
        }
        else
        {
            groundNormal = Vector3.up;
            groundAngle = 0f;
            isGrounded = false;
        }
    }

    private void UpdateTimers()
    {
        coyoteTimer = isGrounded ? settings.coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (controller == null || settings == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        float radius = Mathf.Max(0.01f, controller.radius - controller.skinWidth);
        float bottomLocalY = controller.center.y - (controller.height * 0.5f - controller.radius);
        Vector3 origin = transform.position + new Vector3(controller.center.x, bottomLocalY, controller.center.z);
        Gizmos.DrawWireSphere(origin - Vector3.up * settings.groundCheckDistance, radius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, groundNormal);
    }
}
