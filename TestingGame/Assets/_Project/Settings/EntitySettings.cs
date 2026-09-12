using UnityEngine;

[CreateAssetMenu(fileName = "EntitySettings", menuName = "Game/Paranormal/Entity Settings")]
public class EntitySettings : ScriptableObject
{
    [Header("Movement")]
    public float observeSpeed = 1.2f;
    public float chaseSpeed = 4.2f;

    [Header("State timing")]
    public float stateCheckInterval = 2f;
    [Range(0f, 1f)] public float observeChancePerCheck = 0.35f;
    [Range(0f, 1f)] public float manifestChancePerCheck = 0.5f;
    public float observingMinDuration = 3f;
    public float observingMaxDuration = 7f;
    public float manifestDuration = 2f;

    [Header("Pre-basement scares (not lethal)")]
    [Range(0f, 1f)] public float scareChancePerCheck = 0.2f;
    public float scareChaseDuration = 7f;

    [Header("Combat (only lethal once ForceHunt has been called)")]
    public float attackRange = 1.4f;
    public float attackWindup = 0.6f;
}
