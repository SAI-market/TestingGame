using UnityEngine;

[CreateAssetMenu(fileName = "ViewmodelSettings", menuName = "Game/Player/Viewmodel Settings")]
public class ViewmodelSettings : ScriptableObject
{
    [Header("Look Sway")]
    public float lookSwayAmount = 0.03f;
    public float lookSwayRotationAmount = 4f;
    public float lookSwayMaxOffset = 0.04f;
    public float lookSwaySmoothTime = 0.08f;

    [Header("Movement Sway")]
    public float moveSwayAmount = 0.05f;
    public float moveSwaySmoothTime = 0.15f;

    [Header("Bob")]
    public float bobAmplitudeWalk = 0.015f;
    public float bobAmplitudeSprint = 0.03f;
    public float bobAmplitudeCrouch = 0.01f;

    [Header("Impulses")]
    public float jumpImpulse = 0.05f;
    public float landImpulseScale = 0.02f;
    public float impulseSpring = 12f;
    public float impulseDamping = 0.6f;
}
