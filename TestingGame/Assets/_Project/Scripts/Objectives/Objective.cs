using System;

[Serializable]
public class Objective
{
    public string id;
    public string label;
    public int requiredCount = 1;

    [NonSerialized] public int currentCount;

    public bool IsComplete => currentCount >= requiredCount;
}
