using System;
using UnityEngine;

/// <summary>
/// A door that rotates open/closed around its own pivot on Interact. Can start (or be put)
/// locked, in which case interacting just shows a message instead of opening. Reused for
/// every door in the house, including the front door and the basement door.
/// </summary>
public class SimpleDoor : InteractableBase
{
    [SerializeField] private float openAngle = 100f;
    [SerializeField] private float openSpeedDegPerSec = 160f;
    [SerializeField] private bool startsLocked = false;
    [SerializeField] private string lockedMessage = "Está cerrada con llave.";
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip lockedClip;

    private AudioSource audioSource;
    private Quaternion closedRotation;
    private bool isOpen;
    private bool isLocked;

    public bool IsOpen => isOpen;
    public bool IsLocked => isLocked;

    /// <summary>Fired every time the door successfully swings open (not on close attempts).</summary>
    public event Action OnDoorOpened;

    private void Awake()
    {
        closedRotation = transform.localRotation;
        isLocked = startsLocked;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    private void Update()
    {
        Quaternion target = isOpen ? closedRotation * Quaternion.Euler(0f, openAngle, 0f) : closedRotation;
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, target, openSpeedDegPerSec * Time.deltaTime);
    }

    public override string InteractPrompt => isLocked ? "[E] Puerta cerrada" : (isOpen ? "[E] Cerrar" : "[E] Abrir");

    public override void Interact(PlayerInteractor interactor)
    {
        if (isLocked)
        {
            PlayClip(lockedClip);
            MessageUI.Instance?.Show(lockedMessage);
            return;
        }

        isOpen = !isOpen;
        PlayClip(isOpen ? openClip : closeClip);
        if (isOpen) OnDoorOpened?.Invoke();
    }

    public void SetLocked(bool locked, string message = null)
    {
        isLocked = locked;
        if (message != null) lockedMessage = message;
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
