using UnityEngine;

/// <summary>Anomaly: a one-shot 3D sound (steps, knock, whisper, breathing) with no visible cause.</summary>
public class AmbientSoundEvent : ParanormalEventBase
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private Vector2 volumeRange = new Vector2(0.7f, 1f);

    protected override void Execute()
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        AudioSource.PlayClipAtPoint(clip, transform.position, Random.Range(volumeRange.x, volumeRange.y));
    }
}
