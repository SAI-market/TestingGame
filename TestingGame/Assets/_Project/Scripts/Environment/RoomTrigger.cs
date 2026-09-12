using System;
using UnityEngine;

/// <summary>Fires a static event with a room id whenever the player enters. Anything (scripted
/// events, ambience, future systems) can subscribe without needing a direct reference.</summary>
[RequireComponent(typeof(Collider))]
public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private string roomId;

    public static event Action<string> OnAnyRoomEntered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterController>() == null) return;
        OnAnyRoomEntered?.Invoke(roomId);
    }
}
