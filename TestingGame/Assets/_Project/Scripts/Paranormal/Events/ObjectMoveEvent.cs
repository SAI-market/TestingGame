using System.Collections;
using UnityEngine;

/// <summary>Anomaly: a small prop (picture, glass, toy) shifts to a nearby position/rotation by itself.</summary>
public class ObjectMoveEvent : ParanormalEventBase
{
    [SerializeField] private Transform prop;
    [SerializeField] private Vector3 maxPositionOffset = new Vector3(0.15f, 0f, 0.15f);
    [SerializeField] private float maxTiltDegrees = 20f;
    [SerializeField] private float moveDuration = 1.2f;
    [SerializeField] private AudioClip scrapeClip;

    private AudioSource audioSource;
    private Coroutine running;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Execute()
    {
        if (prop == null) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        if (audioSource != null && scrapeClip != null) audioSource.PlayOneShot(scrapeClip);

        Vector3 startPos = prop.localPosition;
        Quaternion startRot = prop.localRotation;

        Vector3 offset = new Vector3(
            Random.Range(-maxPositionOffset.x, maxPositionOffset.x),
            Random.Range(-maxPositionOffset.y, maxPositionOffset.y),
            Random.Range(-maxPositionOffset.z, maxPositionOffset.z));
        Vector3 targetPos = startPos + offset;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 0f, Random.Range(-maxTiltDegrees, maxTiltDegrees));

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / moveDuration);
            prop.localPosition = Vector3.Lerp(startPos, targetPos, f);
            prop.localRotation = Quaternion.Slerp(startRot, targetRot, f);
            yield return null;
        }
    }
}
