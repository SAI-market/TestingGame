using System.Collections;
using UnityEngine;

/// <summary>Anomaly: a dark silhouette darts across a doorway/hallway and disappears.</summary>
public class ShadowFigureEvent : ParanormalEventBase
{
    [SerializeField] private GameObject shadowVisual;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float crossDuration = 0.8f;
    [SerializeField] private AudioClip whooshClip;

    private AudioSource audioSource;
    private Coroutine running;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (shadowVisual != null) shadowVisual.SetActive(false);
    }

    protected override void Execute()
    {
        if (shadowVisual == null || startPoint == null || endPoint == null) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CrossRoutine());
    }

    private IEnumerator CrossRoutine()
    {
        shadowVisual.SetActive(true);
        shadowVisual.transform.position = startPoint.position;
        if (audioSource != null && whooshClip != null) audioSource.PlayOneShot(whooshClip);

        float t = 0f;
        while (t < crossDuration)
        {
            t += Time.deltaTime;
            shadowVisual.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, Mathf.Clamp01(t / crossDuration));
            yield return null;
        }

        shadowVisual.SetActive(false);
    }
}
