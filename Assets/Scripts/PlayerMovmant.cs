using Photon.Pun;
using UnityEngine;

public class PlayerMovmant : MonoBehaviourPunCallbacks
{
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 2.0f;
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float jumpForce = 10;
    
    DynamicJoystick joystick;
    
    float yForce = 0;
    void Start()
    {
        if (photonView.IsMine) Camera.main.transform.SetParent(transform);
        else enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        /*#if UNITY_EDITOR
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        #else*/
        Vector3 direction = new Vector3(SelfUI.instance.joystick.Horizontal, 0, SelfUI.instance.joystick.Vertical);
        /*#endif*/
        if(Input.GetButtonDown("Jump")) yForce = jumpForce;
        characterController.Move((direction.normalized*speed+new Vector3(0, yForce, 0))*Time.deltaTime);
        yForce -= gravity *Time.deltaTime;
    }
}
