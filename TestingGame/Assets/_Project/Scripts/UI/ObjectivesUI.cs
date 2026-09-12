using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Minimal checklist: rebuilds a single text block whenever objectives change.</summary>
public class ObjectivesUI : MonoBehaviour
{
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private Text label;

    private void OnEnable()
    {
        if (objectiveManager == null) return;
        objectiveManager.OnObjectiveProgress += _ => Refresh();
        objectiveManager.OnObjectiveCompleted += _ => Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (objectiveManager == null || label == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("OBJETIVOS");
        foreach (var o in objectiveManager.Objectives)
        {
            sb.Append(o.IsComplete ? "✓ " : "☐ ").AppendLine(o.label);
        }
        label.text = sb.ToString();
    }
}
