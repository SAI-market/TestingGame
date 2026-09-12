using UnityEngine;

/// <summary>Purely decorative ambient flicker (basement, hallway) — independent from the
/// paranormal LightFlickerEvent, which is scripted/triggered instead of constant.</summary>
public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private float minIntensity = 0.3f;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private float noiseSpeed = 6f;

    private float noiseOffset;

    private void Awake()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        noiseOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (targetLight == null) return;
        float n = Mathf.PerlinNoise(Time.time * noiseSpeed + noiseOffset, 0.5f);
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, n);
    }
}
