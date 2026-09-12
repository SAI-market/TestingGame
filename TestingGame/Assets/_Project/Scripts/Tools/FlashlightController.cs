using System;
using UnityEngine;

/// <summary>
/// Handles the flashlight's on/off state and battery. Deliberately simple: battery drains
/// while on and never recharges, no need for anything fancier in the vertical slice.
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light spotLight;
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainPerSecond = 3.5f;

    private float currentBattery;

    public bool IsOn { get; private set; }
    public float BatteryFraction => maxBattery <= 0f ? 0f : currentBattery / maxBattery;

    public event Action<float> OnBatteryChanged;

    private void Awake()
    {
        currentBattery = maxBattery;
        if (spotLight != null) spotLight.enabled = false;
    }

    private void Update()
    {
        if (!IsOn) return;

        currentBattery = Mathf.Max(0f, currentBattery - drainPerSecond * Time.deltaTime);
        OnBatteryChanged?.Invoke(BatteryFraction);

        if (currentBattery <= 0f) SetOn(false);
    }

    public void Toggle()
    {
        if (!IsOn && currentBattery <= 0f) return;
        SetOn(!IsOn);
    }

    public void ForceOff() => SetOn(false);

    private void SetOn(bool on)
    {
        IsOn = on;
        if (spotLight != null) spotLight.enabled = on;
        OnBatteryChanged?.Invoke(BatteryFraction);
    }
}
