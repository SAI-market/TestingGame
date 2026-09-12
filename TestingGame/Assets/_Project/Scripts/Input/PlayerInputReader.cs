using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single point of contact with the Input System. Every other player script reads
/// input through this component instead of touching InputActions directly.
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private bool lockCursorOnEnable = true;

    private InputActionMap playerMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction toggleCursorAction;
    private InputAction interactAction;
    private InputAction useToolAction;
    private InputAction equipVacuumAction;
    private InputAction equipBroomAction;
    private InputAction equipClothAction;
    private InputAction equipFlashlightAction;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool CrouchPressedThisFrame { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }

    public bool InteractPressedThisFrame { get; private set; }
    public bool UseToolHeld { get; private set; }
    public bool UseToolPressedThisFrame { get; private set; }
    public bool EquipVacuumPressedThisFrame { get; private set; }
    public bool EquipBroomPressedThisFrame { get; private set; }
    public bool EquipClothPressedThisFrame { get; private set; }
    public bool EquipFlashlightPressedThisFrame { get; private set; }

    /// <summary>True if the last non-zero Look sample came from a mouse rather than a gamepad stick.</summary>
    public bool LastLookWasMouse { get; private set; } = true;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("PlayerInputReader: no InputActionAsset assigned.", this);
            enabled = false;
            return;
        }

        playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
        moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
        jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
        sprintAction = playerMap.FindAction("Sprint", throwIfNotFound: true);
        crouchAction = playerMap.FindAction("Crouch", throwIfNotFound: true);
        toggleCursorAction = playerMap.FindAction("ToggleCursor", throwIfNotFound: true);
        interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);
        useToolAction = playerMap.FindAction("UseTool", throwIfNotFound: true);
        equipVacuumAction = playerMap.FindAction("EquipVacuum", throwIfNotFound: true);
        equipBroomAction = playerMap.FindAction("EquipBroom", throwIfNotFound: true);
        equipClothAction = playerMap.FindAction("EquipCloth", throwIfNotFound: true);
        equipFlashlightAction = playerMap.FindAction("EquipFlashlight", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        playerMap?.Enable();
        if (lockCursorOnEnable) SetCursorLocked(true);
    }

    private void OnDisable()
    {
        playerMap?.Disable();
    }

    private void Update()
    {
        Move = moveAction.ReadValue<Vector2>();

        Vector2 rawLook = lookAction.ReadValue<Vector2>();
        if (rawLook.sqrMagnitude > 0f && lookAction.activeControl != null)
        {
            LastLookWasMouse = lookAction.activeControl.device is Mouse;
        }
        Look = rawLook;

        SprintHeld = sprintAction.IsPressed();
        CrouchHeld = crouchAction.IsPressed();
        CrouchPressedThisFrame = crouchAction.WasPressedThisFrame();
        JumpPressedThisFrame = jumpAction.WasPressedThisFrame();
        JumpHeld = jumpAction.IsPressed();

        if (toggleCursorAction.WasPressedThisFrame())
        {
            SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
        }

        InteractPressedThisFrame = interactAction.WasPressedThisFrame();
        UseToolHeld = useToolAction.IsPressed();
        UseToolPressedThisFrame = useToolAction.WasPressedThisFrame();
        EquipVacuumPressedThisFrame = equipVacuumAction.WasPressedThisFrame();
        EquipBroomPressedThisFrame = equipBroomAction.WasPressedThisFrame();
        EquipClothPressedThisFrame = equipClothAction.WasPressedThisFrame();
        EquipFlashlightPressedThisFrame = equipFlashlightAction.WasPressedThisFrame();
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
