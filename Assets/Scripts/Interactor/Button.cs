using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Button : MonoBehaviour, IInteractable
{
    public UnityEvent<PlayerMovmant> OnInteract = new UnityEvent<PlayerMovmant>();
    public UnityEvent<PlayerMovmant> OnEndInteraction = new UnityEvent<PlayerMovmant>();
    public UnityEvent<PlayerMovmant, PointerEventData> OnDrag = new UnityEvent<PlayerMovmant, PointerEventData>();
    public void StartInteraction(PlayerMovmant player)
    {
        OnInteract?.Invoke(player);
    }

    public void Drag(PlayerMovmant player, PointerEventData eventData)
    {
        OnDrag?.Invoke(player, eventData);
    }

    public void EndInteraction(PlayerMovmant player)
    {
        OnEndInteraction?.Invoke(player);
    }
}
