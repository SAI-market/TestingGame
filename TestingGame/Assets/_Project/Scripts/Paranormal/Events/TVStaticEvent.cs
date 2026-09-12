using System.Collections;
using UnityEngine;

/// <summary>Anomaly: the TV turns itself on, plays static, then turns off again.</summary>
public class TVStaticEvent : ParanormalEventBase
{
    [SerializeField] private GameObject screenOnVisual;
    [SerializeField] private AudioClip staticClip;
    [SerializeField] private float onDuration = 4f;

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
            audioSource.loop = true;
        }
        if (screenOnVisual != null) screenOnVisual.SetActive(false);
    }

    protected override void Execute()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(StaticRoutine());
    }

    private IEnumerator StaticRoutine()
    {
        if (screenOnVisual != null) screenOnVisual.SetActive(true);
        if (staticClip != null)
        {
            audioSource.clip = staticClip;
            audioSource.Play();
        }

        yield return new WaitForSeconds(onDuration);

        audioSource.Stop();
        if (screenOnVisual != null) screenOnVisual.SetActive(false);
    }
}
