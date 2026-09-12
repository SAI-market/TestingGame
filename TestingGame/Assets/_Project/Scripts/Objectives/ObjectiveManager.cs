using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the job's checklist. CleanableSpots report themselves here by objectiveId;
/// nobody else needs to know how many spots exist per room.
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [SerializeField] private List<Objective> objectives = new List<Objective>();

    private bool allCompletedFired;

    public IReadOnlyList<Objective> Objectives => objectives;

    public event Action<Objective> OnObjectiveProgress;
    public event Action<Objective> OnObjectiveCompleted;
    public event Action OnAllObjectivesCompleted;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ReportSpotCleaned(string objectiveId)
    {
        if (string.IsNullOrEmpty(objectiveId)) return;

        Objective objective = objectives.Find(o => o.id == objectiveId);
        if (objective == null || objective.IsComplete) return;

        objective.currentCount++;
        OnObjectiveProgress?.Invoke(objective);

        if (objective.IsComplete) OnObjectiveCompleted?.Invoke(objective);

        CheckAllComplete();
    }

    private void CheckAllComplete()
    {
        if (allCompletedFired) return;

        foreach (var o in objectives)
        {
            if (!o.IsComplete) return;
        }

        allCompletedFired = true;
        OnAllObjectivesCompleted?.Invoke();
    }
}
