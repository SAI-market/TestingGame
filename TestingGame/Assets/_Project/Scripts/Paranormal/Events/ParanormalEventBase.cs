using UnityEngine;

/// <summary>
/// One modular paranormal anomaly. The scheduler only ever talks to this base type, so new
/// event kinds (a new subclass) plug in without touching the scheduler or the activity system.
/// </summary>
public abstract class ParanormalEventBase : MonoBehaviour
{
    [SerializeField] private int minLevel = 1;
    [SerializeField] private float cooldownSeconds = 60f;
    [SerializeField] private float weight = 1f;
    [SerializeField] private float activityBump = 3f;

    private float lastTriggerTime = -999f;

    public float Weight => weight;
    public bool IsOnCooldown => Time.time - lastTriggerTime < cooldownSeconds;

    public bool IsEligible(int currentLevel) => currentLevel >= minLevel && !IsOnCooldown;

    /// <summary>Attempts to fire; returns false if on cooldown. Ignores the level gate (used for scripted beats).</summary>
    public bool TryTrigger(bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && IsOnCooldown) return false;

        lastTriggerTime = Time.time;
        Execute();
        ParanormalActivityManager.Instance?.AddActivity(activityBump);
        return true;
    }

    protected abstract void Execute();
}
