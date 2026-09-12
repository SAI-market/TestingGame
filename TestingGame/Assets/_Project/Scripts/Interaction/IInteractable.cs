/// <summary>
/// Anything the player can aim at and press E on: doors, switches, tools, the exit, the
/// basement door. One interface, no per-object duplicated input handling.
/// </summary>
public interface IInteractable
{
    string InteractPrompt { get; }
    bool CanInteract(PlayerInteractor interactor);
    void Interact(PlayerInteractor interactor);
}
