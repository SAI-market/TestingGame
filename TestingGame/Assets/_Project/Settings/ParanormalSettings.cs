using UnityEngine;

/// <summary>
/// Every tunable number for the paranormal activity system in one place, so pacing can be
/// adjusted without touching code.
/// </summary>
[CreateAssetMenu(fileName = "ParanormalSettings", menuName = "Game/Paranormal/Paranormal Settings")]
public class ParanormalSettings : ScriptableObject
{
    [Header("Activity (0-100 internal, never shown to the player)")]
    public float passiveDriftPerSecond = 0.06f;
    public float level2Threshold = 25f;
    public float level3Threshold = 55f;
    public float level4Threshold = 80f;

    [Header("Semi-random event scheduler")]
    public float minIntervalAtLevel1 = 45f;
    public float maxIntervalAtLevel1 = 90f;
    public float minIntervalAtLevel4 = 12f;
    public float maxIntervalAtLevel4 = 25f;
}
