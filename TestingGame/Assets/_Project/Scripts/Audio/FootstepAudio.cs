using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurfaceFootstepSet
{
    public SurfaceType surfaceType;
    public AudioClip[] steps;
    public AudioClip[] lands;
}

/// <summary>
/// Subscribes to HeadBob's step event and PlayerMotor's landing event, picks a clip
/// set by raycasting down for a SurfaceTag, and plays it with randomized pitch/volume.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private HeadBob headBob;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private LayerMask groundMask = 1 << 9; // Ground layer by default
    [SerializeField] private List<SurfaceFootstepSet> surfaceSets = new List<SurfaceFootstepSet>();
    [SerializeField] private AudioClip[] defaultSteps;
    [SerializeField] private AudioClip[] defaultLands;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1f);
    [SerializeField] private float minLandImpactSpeed = 0.5f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }

    private void OnEnable()
    {
        if (headBob != null) headBob.OnStep += PlayStep;
        if (motor != null) motor.OnLanded += HandleLanded;
    }

    private void OnDisable()
    {
        if (headBob != null) headBob.OnStep -= PlayStep;
        if (motor != null) motor.OnLanded -= HandleLanded;
    }

    private void PlayStep() => Play(GetClips(true));

    private void HandleLanded(float impactSpeed)
    {
        if (impactSpeed < minLandImpactSpeed) return;
        Play(GetClips(false));
    }

    private AudioClip[] GetClips(bool step)
    {
        SurfaceType surface = DetectSurface();
        foreach (var set in surfaceSets)
        {
            if (set.surfaceType != surface) continue;
            AudioClip[] clips = step ? set.steps : set.lands;
            if (clips != null && clips.Length > 0) return clips;
        }
        return step ? defaultSteps : defaultLands;
    }

    private SurfaceType DetectSurface()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f, groundMask, QueryTriggerInteraction.Ignore))
        {
            var tag = hit.collider.GetComponentInParent<SurfaceTag>();
            if (tag != null) return tag.surfaceType;
        }
        return SurfaceType.Default;
    }

    private void Play(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.volume = Random.Range(volumeRange.x, volumeRange.y);
        source.PlayOneShot(clip);
    }
}
