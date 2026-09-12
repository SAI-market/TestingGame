using System;
using UnityEngine;

/// <summary>
/// Owns the currently equipped tool, switches on 1-4, and drives cleaning progress on
/// whatever CleanableSpot the player is aiming at through PlayerInteractor. A malfunction
/// (Level 3 paranormal manifestation) can temporarily disable whichever tool is active.
/// </summary>
public class ToolController : MonoBehaviour
{
    public static ToolController Instance { get; private set; }

    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private GameObject[] toolVisuals = new GameObject[4]; // indexed by ToolType
    [SerializeField] private FlashlightController flashlight;

    private CleanableSpot lastTargetedSpot;
    private float malfunctionUntil = -1f;

    public ToolType CurrentTool { get; private set; } = ToolType.Vacuum;
    public bool IsMalfunctioning => Time.time < malfunctionUntil;
    public FlashlightController Flashlight => flashlight;

    public event Action<ToolType> OnToolChanged;

    private void Awake()
    {
        Instance = this;
        if (input == null) input = GetComponent<PlayerInputReader>();
    }

    private void Start()
    {
        Equip(ToolType.Vacuum);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (input.EquipVacuumPressedThisFrame) Equip(ToolType.Vacuum);
        else if (input.EquipBroomPressedThisFrame) Equip(ToolType.Broom);
        else if (input.EquipClothPressedThisFrame) Equip(ToolType.Cloth);
        else if (input.EquipFlashlightPressedThisFrame) Equip(ToolType.Flashlight);

        if (CurrentTool == ToolType.Flashlight)
        {
            if (input.UseToolPressedThisFrame && flashlight != null) flashlight.Toggle();
            return;
        }

        CleanableSpot targeted = null;
        if (!IsMalfunctioning && input.UseToolHeld && interactor.CurrentHit.HasValue)
        {
            var spot = interactor.CurrentHit.Value.collider.GetComponentInParent<CleanableSpot>();
            if (spot != null && spot.RequiredTool == CurrentTool)
            {
                spot.AddProgress(Time.deltaTime);
                targeted = spot;
            }
        }

        if (targeted != lastTargetedSpot) lastTargetedSpot?.StopProgress();
        lastTargetedSpot = targeted;
    }

    public void Equip(ToolType tool)
    {
        CurrentTool = tool;
        for (int i = 0; i < toolVisuals.Length; i++)
        {
            if (toolVisuals[i] != null) toolVisuals[i].SetActive(i == (int)tool);
        }
        if (tool != ToolType.Flashlight) flashlight?.ForceOff();
        OnToolChanged?.Invoke(tool);
    }

    /// <summary>Used by the "manifestation" paranormal event: whatever tool is equipped stops working for a while.</summary>
    public void TriggerMalfunction(float duration)
    {
        malfunctionUntil = Time.time + duration;
    }
}
