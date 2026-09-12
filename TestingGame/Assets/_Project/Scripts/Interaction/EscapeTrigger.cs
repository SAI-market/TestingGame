using UnityEngine;

/// <summary>
/// Placed at the basement's way out. Reaching it while the entity hasn't caught the player
/// ends the game in the "Escape" ending.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EscapeTrigger : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterController>() == null) return;
        GameManager.Instance?.PlayerEscaped();
    }
}
