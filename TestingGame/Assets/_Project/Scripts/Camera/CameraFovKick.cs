using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFovKick : MonoBehaviour
{
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private float sprintFovAdd = 8f;
    [SerializeField] private float smoothTime = 0.25f;

    private Camera cam;
    private float baseFov;
    private float velocity;

    private void Reset()
    {
        motor = GetComponentInParent<PlayerMotor>();
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseFov = cam.fieldOfView;
    }

    private void Update()
    {
        if (motor == null) return;
        float targetFov = baseFov + (motor.IsSprinting ? sprintFovAdd : 0f);
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref velocity, smoothTime);
    }
}
