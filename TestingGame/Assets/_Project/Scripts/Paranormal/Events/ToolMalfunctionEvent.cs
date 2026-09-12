using UnityEngine;

/// <summary>
/// The Level 3 "manifestation" example from the design doc: the player's tool cuts out for a
/// few seconds, something is heard, and it comes back on.
/// </summary>
public class ToolMalfunctionEvent : ParanormalEventBase
{
    [SerializeField] private float malfunctionDuration = 3.5f;
    [SerializeField] private AudioClip[] stingerClips;

    protected override void Execute()
    {
        ToolController.Instance?.TriggerMalfunction(malfunctionDuration);

        if (stingerClips != null && stingerClips.Length > 0)
        {
            var clip = stingerClips[Random.Range(0, stingerClips.Length)];
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }
}
