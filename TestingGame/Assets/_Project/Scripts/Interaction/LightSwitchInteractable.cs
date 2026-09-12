using UnityEngine;

/// <summary>
/// Toggles a set of lights on Interact. Demonstrates that the same interaction system
/// drives switches as well as doors and tools.
/// </summary>
public class LightSwitchInteractable : InteractableBase
{
    [SerializeField] private Light[] lights;
    [SerializeField] private bool startsOn = true;
    [SerializeField] private AudioClip clickClip;

    private AudioSource audioSource;
    private bool isOn;

    private void Awake()
    {
        isOn = startsOn;
        ApplyState();
        audioSource = GetComponent<AudioSource>();
    }

    public override string InteractPrompt => isOn ? "[E] Apagar luz" : "[E] Encender luz";

    public override void Interact(PlayerInteractor interactor)
    {
        isOn = !isOn;
        ApplyState();
        if (audioSource != null && clickClip != null) audioSource.PlayOneShot(clickClip);
    }

    public void ForceState(bool on)
    {
        isOn = on;
        ApplyState();
    }

    private void ApplyState()
    {
        if (lights == null) return;
        foreach (var l in lights)
        {
            if (l != null) l.enabled = isOn;
        }
    }
}
