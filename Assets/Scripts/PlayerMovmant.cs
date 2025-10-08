using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovmant : MonoBehaviourPunCallbacks
{
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 2.0f;
    [SerializeField] float runSpeed = 2.0f;
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float jumpForce = 10;
    
    Joystick joystick;
    
    float yForce = 0;
    bool isRunning = false;
    void Start()
    {
        if (photonView.IsMine)
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;

            SelfUI.instance.eventTriggerJumpButon.OnPointerDownEvent.AddListener((eventData) =>{Jump();});
            
            SelfUI.instance.eventTriggerFullScrean.OnDragEvent.AddListener((eventData) =>{RotateCamera((PointerEventData)eventData);});
            
            SelfUI.instance.eventTriggerRunButon.OnPointerDownEvent.AddListener((eventData) =>{isRunning = !isRunning;});
        }
        else enabled = false;
    }

    private void Jump()
    {
        if(characterController.isGrounded) yForce = jumpForce;
    }

    private void RotateCamera(PointerEventData photonEvent)
    {
        transform.rotation *= Quaternion.Euler(0, photonEvent.delta.x, 0);
        Camera.main.transform.rotation = Quaternion.Euler(Mathf.Clamp((Camera.main.transform.rotation.eulerAngles.x-photonEvent.delta.y>180 ? -360 : 0) + Camera.main.transform.rotation.eulerAngles.x-photonEvent.delta.y, -60, 60), Camera.main.transform.rotation.eulerAngles.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = isRunning ? runSpeed : this.speed;
        Vector3 direction = new Vector3(SelfUI.instance.joystick.Horizontal, 0, SelfUI.instance.joystick.Vertical);
        characterController.Move((transform.rotation*direction.normalized*speed+new Vector3(0, yForce, 0))*Time.deltaTime);
        yForce -= gravity *Time.deltaTime;
    }
}
