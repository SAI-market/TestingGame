using UnityEngine;

[CreateAssetMenu(fileName = "MovementSettings", menuName = "Game/Player/Movement Settings")]
public class MovementSettings : ScriptableObject
{
    [Header("Speeds (m/s)")]
    public float walkSpeed = 4.5f;
    public float sprintSpeed = 7.5f;
    public float crouchSpeed = 2f;

    [Header("Acceleration")]
    public float groundAcceleration = 60f;
    public float groundFriction = 50f;
    public float airAcceleration = 12f;
    public float airFriction = 2f;

    [Header("Gravity")]
    public float gravity = -22f;
    [Range(0f, 1.5f)] public float risingGravityMultiplier = 1f;
    [Range(1f, 3f)] public float fallingGravityMultiplier = 1.4f;

    [Header("Jump")]
    public float jumpHeight = 1.15f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.15f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Slopes")]
    [Range(0f, 89f)] public float maxSlopeAngle = 50f;
    public float slideSpeed = 8f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundMask = 1 << 9;

    [Header("Crouch")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1f;
    public float crouchTransitionTime = 0.15f;
    public bool crouchIsToggle = false;
}
