using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float neededTime;
    [SerializeField] private TMPro.TMP_Text text;
    [SerializeField] private TMPro.TMP_Text errorsText;
    [SerializeField] private Vector2 breakesFrequency;
    private float _progress = 0;
    private float _timeToBreake;

    [SerializeField] private WeldingPunctures weldingPuncturePrefab;
    [SerializeField] private Transform[] weldingPuncturePlaceToSpown;
    
    [SerializeField] private ConnectingWires connectingWires;
    [SerializeField] private ShipMaintenance shipMaintenance1;
    [SerializeField] private ShipMaintenance shipMaintenance2;
    private List<WeldingPunctures> weldingPunctures = new List<WeldingPunctures>();

    private void Start()
    {
        _timeToBreake = Random.Range(breakesFrequency.x, breakesFrequency.y);
        _progress = neededTime;
    }
    
    private void Update()
    {
        if(!checkBreaked()) _progress -= Time.deltaTime;
        _timeToBreake -= Time.deltaTime;
        text.text = new DateTime().AddSeconds(_progress).ToString("mm:ss");
        if (0 > _timeToBreake)
        {
            Breake();
            _timeToBreake = Random.Range(breakesFrequency.x, breakesFrequency.y);
        }
        if (0 > _progress)
        {
            Win();
        }
    }

    bool checkBreaked()
    {
        errorsText.text = connectingWires.GetMassage() + shipMaintenance1.GetMassage() + string.Join("", weldingPunctures.Select(n=>n.GetMassage())) + ((int)Time.time%2==0?"_":"");
        return connectingWires.needToFix || shipMaintenance1.needToFix || weldingPunctures.Count>0;
    }
    
    private void Breake()
    {
        if(!PhotonNetwork.IsMasterClient) return;
        int n = Random.Range(0, 3);
        while (true)
        {
            if (n == 0 && !connectingWires.needToFix)
            {
                connectingWires.StartMiniGame();
                return;
            }
            else if (n == 1 && !shipMaintenance1.needToFix)
            {
                shipMaintenance1.StartMiniGame();
                shipMaintenance2.StartMiniGame();
                return;
            }else if (n == 2)
            {
                Transform t = weldingPuncturePlaceToSpown[Random.Range(0, weldingPuncturePlaceToSpown.Length)];
                Vector3 pos = t.localToWorldMatrix.MultiplyPoint(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
                
                weldingPunctures.Add(PhotonNetwork.Instantiate(weldingPuncturePrefab.name, pos, t.rotation).GetComponent<WeldingPunctures>());
                weldingPunctures[^1].OnDestroyEvent.AddListener((weldingPuncture) => {weldingPunctures.Remove(weldingPuncture);});
            }

            n++;
            if(n>2) return;
        }
    }

    private void Win()
    {
        
    }
}

interface IMiniGame
{
    void StartMiniGame();
    string GetMassage();
}
