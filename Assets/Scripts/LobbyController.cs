using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class LobbyController : MonoBehaviourPunCallbacks
{
    public TMP_Text roomName;
    public TMP_Text readyCount;
    private bool _ready = false;
    
    public static LobbyController Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        UpdateReady();
    }

    public void Ready()
    {
        _ready = !_ready;
        NetworkController.Instance.photonView.RPC("SetReady",  RpcTarget.All, _ready);
    }

    [PunRPC]
    public void UpdateReady()
    {
        NetworkController.ReadyPlayers = 0;
        readyCount.text = $"0 / {PhotonNetwork.CurrentRoom.PlayerCount}";
        if(_ready) NetworkController.Instance.photonView.RPC("SetReady", RpcTarget.All, _ready);
    }
}
