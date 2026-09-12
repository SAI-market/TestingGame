using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Transient center-screen text for narrative beats ("la puerta no abre", etc).</summary>
public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance { get; private set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private Text label;
    [SerializeField] private float fadeSpeed = 3f;

    private Coroutine running;

    private void Awake()
    {
        Instance = this;
        if (group != null) group.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string message, float duration = 3f)
    {
        if (label != null) label.text = message;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowRoutine(duration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        while (group.alpha < 1f)
        {
            group.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        group.alpha = 1f;

        yield return new WaitForSeconds(duration);

        while (group.alpha > 0f)
        {
            group.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        group.alpha = 0f;
    }
}
