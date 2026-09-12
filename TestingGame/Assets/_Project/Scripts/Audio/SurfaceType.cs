using UnityEngine;

public enum SurfaceType
{
    Default,
    Concrete,
    Metal,
    Wood,
    Grass,
    Water
}

/// <summary>Drop this on level geometry so FootstepAudio can pick the right clip set.</summary>
public class SurfaceTag : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Default;
}
