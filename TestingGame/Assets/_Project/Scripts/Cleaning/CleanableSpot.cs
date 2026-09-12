using UnityEngine;

/// <summary>
/// One patch of dirt. Holds its own type/tool/progress and reports itself to the
/// ObjectiveManager when finished. Reusable for dust, cobwebs, stains and debris alike —
/// only the inspector values change.
/// </summary>
public class CleanableSpot : MonoBehaviour
{
    [SerializeField] private ToolType requiredTool = ToolType.Vacuum;
    [SerializeField] private float requiredSeconds = 3f;
    [SerializeField] private string objectiveId;
    [SerializeField] private GameObject dirtVisual;
    [SerializeField] private ParticleSystem cleaningVfx;

    private Collider spotCollider;

    public ToolType RequiredTool => requiredTool;
    public float Progress { get; private set; }
    public bool IsClean { get; private set; }

    private void Awake()
    {
        spotCollider = GetComponent<Collider>();
        if (dirtVisual == null) dirtVisual = gameObject;
    }

    /// <summary>Called by the equipped tool every frame it's aimed at this spot with matching type.</summary>
    public void AddProgress(float deltaTime)
    {
        if (IsClean) return;

        Progress = Mathf.Clamp01(Progress + deltaTime / Mathf.Max(0.01f, requiredSeconds));

        if (cleaningVfx != null && !cleaningVfx.isPlaying) cleaningVfx.Play();

        if (Progress >= 1f) Complete();
    }

    /// <summary>Called every frame a tool stops aiming at this spot, so effort doesn't persist forever.</summary>
    public void StopProgress()
    {
        if (cleaningVfx != null && cleaningVfx.isPlaying) cleaningVfx.Stop();
    }

    private void Complete()
    {
        IsClean = true;
        if (dirtVisual != null) dirtVisual.SetActive(false);
        if (spotCollider != null) spotCollider.enabled = false;
        if (cleaningVfx != null) cleaningVfx.Stop();
        ObjectiveManager.Instance?.ReportSpotCleaned(objectiveId);
    }
}
