using UnityEngine;

/// <summary>
/// Common base for interactables: a fixed prompt string and an always-true CanInteract by
/// default. Concrete objects only need to implement Interact().
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected string prompt = "Interactuar";

    public virtual string InteractPrompt => prompt;

    public virtual bool CanInteract(PlayerInteractor interactor) => true;

    public abstract void Interact(PlayerInteractor interactor);
}
