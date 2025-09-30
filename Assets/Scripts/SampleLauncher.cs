using System;
using Photon.Pun;
using UnityEngine;

public class SampleLauncher : MonoBehaviourPunCallbacks
{
    public PhotonView playerPrefab;

    private void Update()
    {
        if (NetworkController.IsGameStarted)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
            Destroy(this);
        }
    }
}
