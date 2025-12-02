using System;
using System.Collections;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ConnectingWires : MonoBehaviourPunCallbacks, IInteractable, IMiniGame
{
    bool _isCanInteracting = true;
    
    [SerializeField] private Button exitButton;
    [SerializeField] private Wire[] wires;
    [SerializeField] private Transform[] wireTargets;
    
    private Vector3[] _targetPositions;
    

    [SerializeField] private int time = 30;
    [SerializeField] private TMPro.TMP_Text timerText;
    public DateTime endTime = DateTime.Now; 
    
    private bool _needToFix = true;

    public bool needToFix
    {
        get { return _needToFix; }
        set {  photonView.RPC("SetNeedToFix", RpcTarget.All, value); } 
    }
    [PunRPC]
    void SetNeedToFix(bool value)
    {
        _isCanInteracting = value;
    }
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
        exitButton.OnInteract.AddListener(EndInteraction);
        _targetPositions = wireTargets.Select(i=>i.transform.position).ToArray();
    }

    void RandomizeTargets()
    {
        _targetPositions.Shuffle();
        for (int i = 0; i < wireTargets.Length; i++)
        {
            wireTargets[i].position = _targetPositions[i];
        }
    }
    
    private void Update()
    {
        if (endTime > DateTime.Now)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = (endTime-DateTime.Now).ToString("ss");
        }
        else 
        {
            timerText.gameObject.SetActive(false);
            if(exitButton.isActiveAndEnabled) Restart();
        }
    }

    void Restart()
    {
        foreach (Wire wire in wires)
        {
            wire.transform.position = wire.StartPos;
            wire.enabled = true;
        }
        endTime = DateTime.Now.AddSeconds(time);
    }

    public void StartInteraction(PlayerMovmant player)
    {
        if(!needToFix) return;
        if (IsCanInteracting)
        {
            RandomizeTargets();
            endTime = DateTime.Now.AddSeconds(time);
            StartCoroutine(SetCameraPosition(new Vector3(0, 0, -70), transform));
            IsCanInteracting = false;
            exitButton.gameObject.SetActive(true);
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

    public void TryToVerify(PlayerMovmant player)
    {
        for (int i = 0; i < wires.Length; i++) if (wires[i].enabled) return;
        
        needToFix = false;
        EndInteraction(player);
        Debug.Log("PassVerifyConnectingWires");
    }

    void EndInteraction(PlayerMovmant player)
    {
        StartCoroutine(SetCameraPosition(Vector3.zero, player.transform));
        IsCanInteracting = true;
        exitButton.gameObject.SetActive(false);
        SelfUI.instance.eventTriggerJumpButon.gameObject.SetActive(true);
        SelfUI.instance.eventTriggerRunButon.gameObject.SetActive(true);
        SelfUI.instance.joystick.gameObject.SetActive(true);
        PlayerMovmant.Instance.canRotateCamera = true;
        endTime = DateTime.Now;
        foreach (Wire wire in wires)
        {
            wire.transform.position = wire.StartPos;
            wire.enabled = true;
        }
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

    public void StartMiniGame()
    {
        photonView.RPC("StartMiniGameRPC", RpcTarget.All);
    }

    public string GetMassage()
    {
        if(needToFix) return "<color=#FF0000> connect Wires need to fix</color>\n";
        return "";
    }

    [PunRPC]
    public void StartMiniGameRPC()
    {
        needToFix = true;
    }
}
