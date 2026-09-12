using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orchestrates the run's high-level progression: cleaning -> job "done" -> front door
/// turns out to be locked -> basement unlocks -> entity hunts for real -> escape or death.
/// Everything else (activity, events, entity) is self-contained and just reacts to Phase.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private SimpleDoor frontDoor;
    [SerializeField] private SimpleDoor basementDoor;
    [SerializeField] private EntityController entity;
    [SerializeField] private ParanormalActivityManager activityManager;
    [SerializeField] private ParanormalEventScheduler eventScheduler;
    [SerializeField] private AmbientAudioController ambientAudio;
    [SerializeField] private PlayerInputReader playerInput;

    public GamePhase Phase { get; private set; } = GamePhase.Cleaning;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (ObjectiveManager.Instance != null) ObjectiveManager.Instance.OnAllObjectivesCompleted += HandleAllObjectivesCompleted;
        if (frontDoor != null) frontDoor.OnDoorOpened += HandleFrontDoorOpened;
        if (basementDoor != null) basementDoor.OnDoorOpened += HandleBasementDoorOpened;
    }

    private void Update()
    {
        if ((Phase == GamePhase.Dead || Phase == GamePhase.Escaped) && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void HandleFrontDoorOpened()
    {
        // Before the job is done there's nothing stopping the player from peeking outside.
    }

    private void HandleAllObjectivesCompleted()
    {
        StartCoroutine(TrapSequence());
    }

    private IEnumerator TrapSequence()
    {
        activityManager?.SetDriftMultiplier(3f);

        MessageUI.Instance?.Show("Listo. Con esto termino. Deberían pagarme por esto.", 3f);
        yield return new WaitForSeconds(4f);

        frontDoor?.SetLocked(true, "La puerta no abre. Está trabada desde afuera.");
        MessageUI.Instance?.Show("¿Qué...? La puerta no abre.", 3f);
        yield return new WaitForSeconds(3.5f);

        MessageUI.Instance?.Show("Se escuchó algo abrirse en el sótano.", 3f);
        basementDoor?.SetLocked(false);
        Phase = GamePhase.Trapped;
    }

    private void HandleBasementDoorOpened()
    {
        if (Phase != GamePhase.Trapped) return;

        Phase = GamePhase.Basement;
        activityManager?.ForceMaxLevel();
        eventScheduler?.SetPaused(true);
        ambientAudio?.EnterBasementMood();
        entity?.ForceHunt();
        MessageUI.Instance?.Show("El aire acá abajo es distinto.", 3f);
    }

    public void PlayerCaught()
    {
        if (Phase == GamePhase.Dead || Phase == GamePhase.Escaped) return;
        Phase = GamePhase.Dead;
        FreezePlayer();
        EndScreenUI.Instance?.ShowDeath();
    }

    public void PlayerEscaped()
    {
        if (Phase != GamePhase.Basement) return;
        Phase = GamePhase.Escaped;
        FreezePlayer();
        EndScreenUI.Instance?.ShowEscape();
    }

    private void FreezePlayer()
    {
        if (playerInput != null) playerInput.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
