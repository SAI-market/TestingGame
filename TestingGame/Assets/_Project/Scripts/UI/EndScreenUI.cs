using UnityEngine;
using UnityEngine.UI;

/// <summary>Full-screen death/escape panel. Restart is a plain key press, no button/EventSystem needed.</summary>
public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text subtitleText;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowDeath()
    {
        Show("NO SALISTE", "Algo te alcanzó en el sótano.\n\nPresioná R para reintentar.");
    }

    public void ShowEscape()
    {
        Show("LOGRASTE ESCAPAR", "Nunca más volvés a esa casa.\n\nPresioná R para jugar de nuevo.");
    }

    private void Show(string title, string subtitle)
    {
        if (panel != null) panel.SetActive(true);
        if (titleText != null) titleText.text = title;
        if (subtitleText != null) subtitleText.text = subtitle;
    }
}
