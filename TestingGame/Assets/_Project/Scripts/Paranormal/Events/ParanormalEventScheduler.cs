using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks a semi-random eligible event every so often. "Semi" because eligibility is gated by
/// paranormal level and per-event cooldown, and the pick is weighted rather than uniform —
/// not every anomaly happens in every playthrough.
/// </summary>
public class ParanormalEventScheduler : MonoBehaviour
{
    public static ParanormalEventScheduler Instance { get; private set; }

    [SerializeField] private List<ParanormalEventBase> events = new List<ParanormalEventBase>();
    [SerializeField] private ParanormalActivityManager activityManager;

    private float timer;
    private bool paused;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (paused || activityManager == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        TryFireRandomEvent();
        ResetTimer();
    }

    public void RegisterEvent(ParanormalEventBase evt)
    {
        if (evt != null && !events.Contains(evt)) events.Add(evt);
    }

    public void SetPaused(bool value) => paused = value;

    private void ResetTimer()
    {
        float min = activityManager.GetSchedulerIntervalMin();
        float max = activityManager.GetSchedulerIntervalMax();
        timer = Random.Range(min, max);
    }

    private void TryFireRandomEvent()
    {
        int level = activityManager.Level;
        List<ParanormalEventBase> eligible = new List<ParanormalEventBase>();
        float totalWeight = 0f;

        foreach (var evt in events)
        {
            if (evt == null || !evt.IsEligible(level)) continue;
            eligible.Add(evt);
            totalWeight += Mathf.Max(0.01f, evt.Weight);
        }

        if (eligible.Count == 0) return;

        float roll = Random.Range(0f, totalWeight);
        float accum = 0f;
        foreach (var evt in eligible)
        {
            accum += Mathf.Max(0.01f, evt.Weight);
            if (roll <= accum)
            {
                evt.TryTrigger();
                return;
            }
        }
    }
}
