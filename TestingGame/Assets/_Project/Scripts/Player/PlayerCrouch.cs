using UnityEngine;

/// <summary>
/// Lerps CharacterController height/center and the camera pivot height between standing
/// and crouching. Refuses to stand back up while something blocks the head.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [SerializeField] private MovementSettings settings;
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private LayerMask ceilingMask = 1 << 9; // Ground layer by default

    private CharacterController controller;
    private bool toggledCrouch;
    private float standCameraY;
    private float crouchCameraY;
    private float transitionT; // 0 = fully crouched, 1 = fully standing

    public bool IsCrouching { get; private set; }

    private void Reset()
    {
        input = GetComponent<PlayerInputReader>();
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (input == null) input = GetComponent<PlayerInputReader>();

        controller.height = settings.standHeight;
        controller.center = new Vector3(0f, settings.standHeight * 0.5f, 0f);

        const float eyeOffsetFromTop = 0.15f;
        standCameraY = settings.standHeight - eyeOffsetFromTop;
        crouchCameraY = settings.crouchHeight - eyeOffsetFromTop;

        if (cameraPivot != null)
        {
            Vector3 pos = cameraPivot.localPosition;
            pos.y = standCameraY;
            cameraPivot.localPosition = pos;
        }

        transitionT = 1f;
    }

    private void Update()
    {
        bool wantsCrouch;
        if (settings.crouchIsToggle)
        {
            if (input.CrouchPressedThisFrame) toggledCrouch = !toggledCrouch;
            wantsCrouch = toggledCrouch;
        }
        else
        {
            wantsCrouch = input.CrouchHeld;
        }

        // Can't stand up if something is blocking the head.
        if (!wantsCrouch && IsCrouching && IsCeilingBlocked())
        {
            wantsCrouch = true;
        }

        IsCrouching = wantsCrouch;

        float targetT = IsCrouching ? 0f : 1f;
        float step = Time.deltaTime / Mathf.Max(0.01f, settings.crouchTransitionTime);
        transitionT = Mathf.MoveTowards(transitionT, targetT, step);

        float height = Mathf.Lerp(settings.crouchHeight, settings.standHeight, transitionT);
        controller.height = height;
        controller.center = new Vector3(0f, height * 0.5f, 0f);

        if (cameraPivot != null)
        {
            float camY = Mathf.Lerp(crouchCameraY, standCameraY, transitionT);
            Vector3 pos = cameraPivot.localPosition;
            pos.y = camY;
            cameraPivot.localPosition = pos;
        }
    }

    private bool IsCeilingBlocked()
    {
        float radius = controller.radius * 0.95f;
        Vector3 bottom = transform.position + new Vector3(0f, radius, 0f);
        Vector3 top = transform.position + new Vector3(0f, settings.standHeight - radius, 0f);
        return Physics.CheckCapsule(bottom, top, radius, ceilingMask, QueryTriggerInteraction.Ignore);
    }
}
