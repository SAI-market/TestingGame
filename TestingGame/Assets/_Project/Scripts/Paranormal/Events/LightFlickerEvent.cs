using System.Collections;
using UnityEngine;

/// <summary>Anomaly: one or more lights flicker rapidly, then settle on (or off).</summary>
public class LightFlickerEvent : ParanormalEventBase
{
    [SerializeField] private Light[] lights;
    [SerializeField] private float flickerDuration = 2.5f;
    [SerializeField] private bool endOff = false;
    [SerializeField] private AudioClip electricalClip;

    private AudioSource audioSource;
    private Coroutine running;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Execute()
    {
        if (lights == null || lights.Length == 0) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        if (audioSource != null && electricalClip != null) audioSource.PlayOneShot(electricalClip);

        bool[] wasOn = new bool[lights.Length];
        for (int i = 0; i < lights.Length; i++) wasOn[i] = lights[i] != null && lights[i].enabled;

        float t = 0f;
        while (t < flickerDuration)
        {
            float step = Random.Range(0.03f, 0.12f);
            t += step;
            foreach (var l in lights)
            {
                if (l != null) l.enabled = Random.value > 0.4f;
            }
            yield return new WaitForSeconds(step);
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null) lights[i].enabled = endOff ? false : wasOn[i];
        }
    }
}
