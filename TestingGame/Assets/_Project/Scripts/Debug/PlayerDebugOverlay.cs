using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Press F3 to toggle an on-screen readout of the motor's live state.</summary>
public class PlayerDebugOverlay : MonoBehaviour
{
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerState state;

    private bool visible;
    private GUIStyle style;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            visible = !visible;
        }
    }

    private void OnGUI()
    {
        if (!visible || motor == null) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            style.normal.textColor = Color.white;
        }

        string text =
            $"Speed: {motor.HorizontalSpeed:F2} m/s\n" +
            $"Vertical Vel: {motor.Velocity.y:F2}\n" +
            $"Grounded: {motor.IsGrounded}\n" +
            $"Sprinting: {motor.IsSprinting}\n" +
            $"State: {(state != null ? state.Current.ToString() : "-")}";

        GUI.Label(new Rect(12, 12, 320, 140), text, style);
    }
}
