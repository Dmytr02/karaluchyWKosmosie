using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovableObject : MonoBehaviourPunCallbacks, IInteractable
{
    [SerializeField] private Outline outline;
    public bool isCaring;
    public void StartInteraction(PlayerMovmant player)
    {
        if(!isCaring) player.photonView.RPC("PickUp", RpcTarget.All, photonView.ViewID);
    }

    public void Drag(PlayerMovmant player, PointerEventData eventData)
    {
        
    }

    public void EndInteraction(PlayerMovmant player)
    {
        
    }

    private void FixedUpdate()
    {
        if (Camera.main != null && Vector3.Distance(transform.position, Camera.main.transform.position) < 5) 
            outline.enabled = true;
        else
            outline.enabled = false;
    }
}
