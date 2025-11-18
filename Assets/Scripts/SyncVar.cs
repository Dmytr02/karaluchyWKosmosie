using Photon.Pun;
using UnityEngine;

public class SyncVar<T>
{
    private T _value;
    private PhotonView _photonView;
    
    public T Value
    {
        get { return _value; }
        set
        {
            Debug.Log(_photonView);
            _photonView.RPC("setValue", RpcTarget.All, value); 
        }
    }

    public SyncVar(PhotonView pv)
    {
        _photonView = pv;
    }
    public SyncVar(PhotonView pv, T value)
    {
        Debug.Log(_photonView);
        _photonView = pv;
        _value = value;
    }

    [PunRPC]
    private void setValue(T value)
    {
        _value = value;
    }
}
