using UnityEngine;

/// <summary>Two looping layers: a constant house ambience and a tension drone whose volume
/// tracks the paranormal level. Swaps to a basement clip once the finale begins.</summary>
public class AmbientAudioController : MonoBehaviour
{
    [SerializeField] private ParanormalActivityManager activityManager;
    [SerializeField] private AudioSource houseAmbience;
    [SerializeField] private AudioSource tensionDrone;
    [SerializeField] private AudioClip basementAmbienceClip;
    [SerializeField] private float maxDroneVolume = 0.6f;

    private float targetDroneVolume;

    private void OnEnable()
    {
        if (activityManager != null) activityManager.OnLevelChanged += HandleLevelChanged;
    }

    private void OnDisable()
    {
        if (activityManager != null) activityManager.OnLevelChanged -= HandleLevelChanged;
    }

    private void Start()
    {
        HandleLevelChanged(activityManager != null ? activityManager.Level : 1);

        if (houseAmbience != null && !houseAmbience.isPlaying) houseAmbience.Play();
        if (tensionDrone != null)
        {
            tensionDrone.volume = 0f;
            if (!tensionDrone.isPlaying) tensionDrone.Play();
        }
    }

    private void Update()
    {
        if (tensionDrone != null) tensionDrone.volume = Mathf.MoveTowards(tensionDrone.volume, targetDroneVolume, Time.deltaTime * 0.3f);
    }

    private void HandleLevelChanged(int level)
    {
        targetDroneVolume = Mathf.Lerp(0f, maxDroneVolume, (level - 1) / 3f);
    }

    public void EnterBasementMood()
    {
        if (houseAmbience != null && basementAmbienceClip != null)
        {
            houseAmbience.clip = basementAmbienceClip;
            houseAmbience.Play();
        }
        targetDroneVolume = maxDroneVolume;
    }
}
