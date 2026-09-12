using UnityEngine;

[CreateAssetMenu(fileName = "LookSettings", menuName = "Game/Player/Look Settings")]
public class LookSettings : ScriptableObject
{
    [Header("Mouse Sensitivity")]
    public float sensitivityX = 0.12f;
    public float sensitivityY = 0.12f;

    [Header("Gamepad Sensitivity (deg/sec)")]
    public float gamepadSensitivityX = 180f;
    public float gamepadSensitivityY = 180f;

    public bool invertY = false;

    [Header("Pitch Clamp")]
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Smoothing")]
    [Range(0f, 0.2f)] public float smoothingTime = 0f;
}
