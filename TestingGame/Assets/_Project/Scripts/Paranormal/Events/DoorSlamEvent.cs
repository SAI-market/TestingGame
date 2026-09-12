using System.Collections;
using UnityEngine;

/// <summary>Anomaly: a door swings open (or slams shut) by itself.</summary>
public class DoorSlamEvent : ParanormalEventBase
{
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float swingAngle = 70f;
    [SerializeField] private float swingSpeedDegPerSec = 220f;
    [SerializeField] private AudioClip slamClip;

    private AudioSource audioSource;
    private Coroutine running;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    protected override void Execute()
    {
        if (doorPivot == null) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        Quaternion closed = doorPivot.localRotation;
        Quaternion open = closed * Quaternion.Euler(0f, swingAngle, 0f);

        if (slamClip != null) audioSource.PlayOneShot(slamClip);

        while (Quaternion.Angle(doorPivot.localRotation, open) > 1f)
        {
            doorPivot.localRotation = Quaternion.RotateTowards(doorPivot.localRotation, open, swingSpeedDegPerSec * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(Random.Range(0.6f, 1.4f));

        if (slamClip != null) audioSource.PlayOneShot(slamClip);
        while (Quaternion.Angle(doorPivot.localRotation, closed) > 1f)
        {
            doorPivot.localRotation = Quaternion.RotateTowards(doorPivot.localRotation, closed, swingSpeedDegPerSec * Time.deltaTime);
            yield return null;
        }
    }
}
