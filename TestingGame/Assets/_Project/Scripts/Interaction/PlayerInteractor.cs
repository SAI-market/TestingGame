using UnityEngine;

/// <summary>
/// Single raycaster for the whole interaction system. Every frame it looks for an
/// IInteractable in front of the camera and, on Interact pressed, calls it. Tools read
/// CurrentHit to know what they're pointed at for cleaning.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private float maxDistance = 2.5f;

    public RaycastHit? CurrentHit { get; private set; }
    public IInteractable CurrentInteractable { get; private set; }

    private void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        if (rayOrigin == null || input == null) return;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Collide))
        {
            CurrentHit = hit;
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            CurrentInteractable = (interactable != null && interactable.CanInteract(this)) ? interactable : null;
        }
        else
        {
            CurrentHit = null;
            CurrentInteractable = null;
        }

        if (input.InteractPressedThisFrame && CurrentInteractable != null)
        {
            CurrentInteractable.Interact(this);
        }
    }
}
