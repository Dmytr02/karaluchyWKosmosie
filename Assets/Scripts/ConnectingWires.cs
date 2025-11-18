using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConnectingWires : MonoBehaviourPunCallbacks, IInteractable
{
    bool _isCanInteracting = true;
    
    [SerializeField] private Button _exitButton;

    public bool IsCanInteracting
    {
        get { return _isCanInteracting; }
        set { photonView.RPC("SetIsCanInteracting", RpcTarget.All, value); }
    }

    [PunRPC]
    void SetIsCanInteracting(bool value)
    {
        _isCanInteracting = value;
    }

    private void Start()
    {
        _exitButton.OnInteract.AddListener(EndInteraction);
    }

    public void StartInteraction(PlayerMovmant player)
    {
        if (IsCanInteracting)
        {
            StartCoroutine(SetCameraPosition(new Vector3(0, 0, -2), transform));
            IsCanInteracting = false;
            _exitButton.gameObject.SetActive(true);
            SelfUI.instance.eventTriggerJumpButon.gameObject.SetActive(false);
            SelfUI.instance.eventTriggerRunButon.gameObject.SetActive(false);
            SelfUI.instance.joystick.gameObject.SetActive(false);
            PlayerMovmant.Instance.canRotateCamera = false;
        }
    }

    public void Drag(PlayerMovmant player, PointerEventData eventData)
    {
        
    }

    void IInteractable.EndInteraction(PlayerMovmant player)
    {
        
    }

    void EndInteraction(PlayerMovmant player)
    {
        StartCoroutine(SetCameraPosition(Vector3.zero, player.transform));
        IsCanInteracting = true;
        _exitButton.gameObject.SetActive(false);
        SelfUI.instance.eventTriggerJumpButon.gameObject.SetActive(true);
        SelfUI.instance.eventTriggerRunButon.gameObject.SetActive(true);
        SelfUI.instance.joystick.gameObject.SetActive(true);
        PlayerMovmant.Instance.canRotateCamera = true;
    }

    IEnumerator SetCameraPosition(Vector3 position, Transform parentTransform)
    {
        Vector3 targetPos = parentTransform.localToWorldMatrix.MultiplyPoint(position);
        Quaternion rotation = parentTransform.rotation;
        while (Vector3.Distance(Camera.main.transform.position, targetPos) > 0.1f)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, Time.deltaTime*10);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, rotation, Time.deltaTime*10);
            yield return null;
        }
        Camera.main.transform.position = targetPos;
        Camera.main.transform.rotation = rotation;
    }
}
