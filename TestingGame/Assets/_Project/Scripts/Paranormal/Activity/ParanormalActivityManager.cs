using System;
using UnityEngine;

/// <summary>
/// The single internal counter driving the whole haunting. The player never sees the raw
/// number, only its effects (which events can fire, how tense the ambience gets). Levels 1-4
/// map to the four fear stages from the design doc.
/// </summary>
public class ParanormalActivityManager : MonoBehaviour
{
    public static ParanormalActivityManager Instance { get; private set; }

    [SerializeField] private ParanormalSettings settings;

    private float activity;
    private float driftMultiplier = 1f;
    private bool forced;

    public int Level { get; private set; } = 1;
    public float NormalizedActivity => activity / 100f;

    public event Action<int> OnLevelChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (forced) return;

        activity = Mathf.Clamp(activity + settings.passiveDriftPerSecond * driftMultiplier * Time.deltaTime, 0f, 100f);
        RecomputeLevel();
    }

    public void AddActivity(float amount)
    {
        if (forced) return;
        activity = Mathf.Clamp(activity + amount, 0f, 100f);
        RecomputeLevel();
    }

    /// <summary>Speeds up (or slows down) the passive climb — used once the job is nearly done.</summary>
    public void SetDriftMultiplier(float multiplier) => driftMultiplier = multiplier;

    /// <summary>Pins the activity (and level) at maximum for the basement/finale phase.</summary>
    public void ForceMaxLevel()
    {
        forced = true;
        activity = 100f;
        RecomputeLevel();
    }

    public float GetSchedulerIntervalMin() => Mathf.Lerp(settings.minIntervalAtLevel1, settings.minIntervalAtLevel4, (Level - 1) / 3f);
    public float GetSchedulerIntervalMax() => Mathf.Lerp(settings.maxIntervalAtLevel1, settings.maxIntervalAtLevel4, (Level - 1) / 3f);

    private void RecomputeLevel()
    {
        int newLevel = 1;
        if (activity >= settings.level4Threshold) newLevel = 4;
        else if (activity >= settings.level3Threshold) newLevel = 3;
        else if (activity >= settings.level2Threshold) newLevel = 2;

        if (newLevel != Level)
        {
            Level = newLevel;
            OnLevelChanged?.Invoke(Level);
        }
    }
}
