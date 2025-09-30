using System;
using Photon.Pun;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public static int ReadyPlayers = 0;
    public static NetworkController Instance;
    public static bool IsGameStarted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    [PunRPC]
    public void SetReady(bool ready)
    {
        if (ready) ReadyPlayers++;
        else ReadyPlayers--;
        
        LobbyController.Instance.readyCount.text = $"{ReadyPlayers.ToString()} / {PhotonNetwork.CurrentRoom.PlayerCount}";

        if (ReadyPlayers >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            IsGameStarted = true;
            Destroy(LobbyController.Instance.gameObject);
        }
    }
}
