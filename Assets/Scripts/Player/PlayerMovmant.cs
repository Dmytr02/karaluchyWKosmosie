using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerMovmant : MonoBehaviourPunCallbacks
{
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 2.0f;
    [SerializeField] float runSpeed = 2.0f;
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float jumpForce = 10;
    [SerializeField] float interactDistance = 5;
    
    public Transform hand;
    public MovableObject movableInHand;
    
    [SerializeField] private Animator animator;
    
    Joystick joystick;
    
    float yForce = 0;
    bool isRunning = false;

    public static List<PlayerMovmant>  players = new List<PlayerMovmant>();
    
    public static PlayerMovmant Instance;
    
    public bool canRotateCamera = true;
    
    private IInteractable isInteraction;
    public int InteractID = -1;
    
    void Start()
    {
        players.Add(this);
        if (photonView.IsMine)
        {
            Instance = this;

            foreach (var i in GetComponentsInChildren<Renderer>()) i.enabled = false;
            

            
            
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;

            SelfUI.instance.eventTriggerJumpButon.OnPointerDownEvent.AddListener((eventData) =>{Jump();});
            
            SelfUI.instance.eventTriggerFullScrean.OnDragEvent.AddListener((eventData) =>{RotateCamera((PointerEventData)eventData);});
            SelfUI.instance.eventTriggerFullScrean.OnPointerDownEvent.AddListener((eventData) =>{OnPointerDown(eventData);});
            SelfUI.instance.eventTriggerFullScrean.OnPointerUpEvent.AddListener((eventData) =>{OnPointerUp(eventData);});
            
            SelfUI.instance.eventTriggerRunButon.OnPointerDownEvent.AddListener((eventData) => { RunOrDropObject();});
        }
        else enabled = false;
    }

    void OnDestroy()
    {
        players.Remove(this);
    }
    
    private void Jump()
    {
        if(characterController.isGrounded) yForce = jumpForce;
    }

    private void RunOrDropObject()
    {
        if(movableInHand) photonView.RPC("Drop", RpcTarget.All);
        else
        {
            isRunning = !isRunning;
            photonView.RPC("isRunningRPC", RpcTarget.All, isRunning);
        }
    }

    [PunRPC]
    void isRunningRPC(bool isRunning)
    {
        animator.SetBool("isRun", isRunning);
    }

    private void RotateCamera(PointerEventData photonEvent)
    {
        if (isInteraction != null && InteractID == photonEvent.pointerId)
        {
            isInteraction.Drag(this, photonEvent);
            return;
        }
        if(!canRotateCamera) return;
        transform.rotation *= Quaternion.Euler(0, photonEvent.delta.x, 0);
        Camera.main.transform.rotation = Quaternion.Euler(Mathf.Clamp((Camera.main.transform.rotation.eulerAngles.x-photonEvent.delta.y>180 ? -360 : 0) + Camera.main.transform.rotation.eulerAngles.x-photonEvent.delta.y, -60, 60), Camera.main.transform.rotation.eulerAngles.y, 0);
    }

    private void OnPointerDown(PointerEventData eventData)
    {
        if(isRunning) return;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(eventData.position), out RaycastHit hit, interactDistance))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                isInteraction = interactable;
                InteractID = eventData.pointerId;
                interactable.StartInteraction(this);
            }
        }
    }

    private void OnPointerUp(PointerEventData eventData)
    {
        if (isInteraction != null && eventData.pointerId == InteractID)
        {
            isInteraction.EndInteraction(this);
            isInteraction = null;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        float speed = isRunning ? runSpeed : this.speed;
        Vector3 direction = new Vector3(SelfUI.instance.joystick.Horizontal, 0, SelfUI.instance.joystick.Vertical);
        characterController.Move((transform.rotation*direction.normalized*speed+new Vector3(0, yForce, 0))*Time.deltaTime);
        yForce -= gravity *Time.deltaTime;
    }
    
    [PunRPC]
    public void PickUp(int objID)
    {
        PhotonView obj = PhotonView.Find(objID);
        obj.transform.SetParent(hand);
        obj.transform.localPosition = Vector3.zero;
        movableInHand = obj.GetComponent<MovableObject>();
        movableInHand.isCaring = true;
    }
    
    [PunRPC]
    public void Drop() 
    {
        movableInHand.GetComponent<MovableObject>().isCaring = false;
        movableInHand.transform.SetParent(null);
        movableInHand = null;
    }
    
    
}

interface IInteractable
{
    public void StartInteraction(PlayerMovmant player);
    public void Drag(PlayerMovmant player, PointerEventData eventData);
    public void EndInteraction(PlayerMovmant player);
}