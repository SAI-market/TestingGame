using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple, reliable state machine for the entity: Idle -> Observing -> Manifesting are the
/// pre-danger glimpses (mostly hidden, occasionally seen at a distance for a second or two).
/// Chasing/Attacking are only lethal once ForceHunt() is called (basement/finale) — before
/// that, an escalation to Chasing is a scare that always ends in a harmless "miss".
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EntityController : MonoBehaviour
{
    [SerializeField] private EntitySettings settings;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] hidingSpots;
    [SerializeField] private Renderer[] visualRenderers;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource moveLoopSource;
    [SerializeField] private AudioClip[] manifestClips;
    [SerializeField] private AudioClip attackClip;

    private NavMeshAgent agent;
    private float nextCheckTime;
    private float stateEndTime;
    private bool huntForced;

    public EntityState State { get; private set; } = EntityState.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        SetVisible(false);
        agent.isStopped = true;
    }

    private void Update()
    {
        UpdateMoveLoop();

        switch (State)
        {
            case EntityState.Idle: HandleIdle(); break;
            case EntityState.Observing: HandleObserving(); break;
            case EntityState.Manifesting: HandleManifesting(); break;
            case EntityState.Chasing: HandleChasing(); break;
            case EntityState.Attacking: HandleAttacking(); break;
        }
    }

    /// <summary>Called once by GameManager when the finale begins: from now on the chase is real.</summary>
    public void ForceHunt()
    {
        huntForced = true;
        EnterChasing();
    }

    private int CurrentLevel() => ParanormalActivityManager.Instance != null ? ParanormalActivityManager.Instance.Level : 1;

    private void HandleIdle()
    {
        if (huntForced) return;
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + settings.stateCheckInterval;

        int level = CurrentLevel();
        if (level >= 2 && Random.value < settings.observeChancePerCheck)
        {
            EnterObserving();
        }
    }

    private void EnterObserving()
    {
        State = EntityState.Observing;
        SetVisible(false);
        agent.isStopped = false;
        agent.speed = settings.observeSpeed;

        Transform spot = PickHidingSpot();
        if (spot != null) agent.SetDestination(spot.position);

        stateEndTime = Time.time + Random.Range(settings.observingMinDuration, settings.observingMaxDuration);
    }

    private void HandleObserving()
    {
        if (Time.time < stateEndTime) return;

        int level = CurrentLevel();
        if (level >= 4 && Random.value < settings.scareChancePerCheck)
        {
            EnterChasing();
        }
        else if (level >= 3 && Random.value < settings.manifestChancePerCheck)
        {
            EnterManifesting();
        }
        else
        {
            BackToIdle();
        }
    }

    private void EnterManifesting()
    {
        State = EntityState.Manifesting;
        agent.isStopped = true;
        SetVisible(true);
        PlayOneShot(manifestClips);
        stateEndTime = Time.time + settings.manifestDuration;
    }

    private void HandleManifesting()
    {
        if (Time.time < stateEndTime) return;
        SetVisible(false);
        BackToIdle();
    }

    private void EnterChasing()
    {
        State = EntityState.Chasing;
        SetVisible(true);
        agent.isStopped = false;
        agent.speed = settings.chaseSpeed;
        stateEndTime = Time.time + settings.scareChaseDuration;
    }

    private void HandleChasing()
    {
        if (player != null) agent.SetDestination(player.position);

        if (player != null && Vector3.Distance(transform.position, player.position) <= settings.attackRange)
        {
            EnterAttacking();
            return;
        }

        if (!huntForced && Time.time >= stateEndTime)
        {
            SetVisible(false);
            BackToIdle();
        }
    }

    private void EnterAttacking()
    {
        State = EntityState.Attacking;
        agent.isStopped = true;
        PlayOneShot(new[] { attackClip });
        stateEndTime = Time.time + settings.attackWindup;
    }

    private void HandleAttacking()
    {
        if (player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (Time.time < stateEndTime) return;

        bool inRange = player != null && Vector3.Distance(transform.position, player.position) <= settings.attackRange + 0.3f;

        if (inRange && huntForced)
        {
            GameManager.Instance?.PlayerCaught();
            return;
        }

        if (huntForced)
        {
            EnterChasing();
        }
        else
        {
            SetVisible(false);
            BackToIdle();
        }
    }

    private void BackToIdle()
    {
        State = EntityState.Idle;
        agent.isStopped = true;
        nextCheckTime = Time.time + settings.stateCheckInterval;
    }

    private Transform PickHidingSpot()
    {
        if (hidingSpots == null || hidingSpots.Length == 0) return null;
        return hidingSpots[Random.Range(0, hidingSpots.Length)];
    }

    private void SetVisible(bool visible)
    {
        if (visualRenderers == null) return;
        foreach (var r in visualRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    private void UpdateMoveLoop()
    {
        if (moveLoopSource == null) return;
        bool shouldPlay = !agent.isStopped && agent.velocity.sqrMagnitude > 0.05f;
        if (shouldPlay && !moveLoopSource.isPlaying) moveLoopSource.Play();
        else if (!shouldPlay && moveLoopSource.isPlaying) moveLoopSource.Stop();
    }

    private void PlayOneShot(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
