using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float neededTime;
    [SerializeField] private Slider progressBar;
    
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
        if(!NetworkController.IsGameStarted) return;
        if(!checkBreaked()) _progress -= Time.deltaTime * (ShipMaintenance.DirectionAccuracy-0.5f) * 2;
        progressBar.value = 1-(_progress/neededTime);
        _timeToBreake -= Time.deltaTime;
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
        errorsText.text = $"estimated time of arrival: {new DateTime().AddSeconds(_progress).ToString("mm:ss")}\n" + shipMaintenance1.GetMassage() + connectingWires.GetMassage() + string.Join("", weldingPunctures.Select(n=>n.GetMassage())) + ( (weldingPunctures.Count<=0 && !connectingWires.needToFix) ? "all systems functioning normally":"") + ((int)Time.time%2==0?"_":"");
        return weldingPunctures.Count>0;
    }
    
    private void Breake()
    {
        if(!PhotonNetwork.IsMasterClient) return;
        int n = Random.Range(0, 2);
        while (true)
        {
            if (n == 0 && !connectingWires.needToFix)
            {
                connectingWires.needToFix = true;
                return;
            }
            else if (n == 1)
            {
                Transform[] transfoms = weldingPuncturePlaceToSpown.Where(n => n.gameObject.activeSelf).ToArray();
                if(transfoms.Length <= 0) return;
                Transform t = transfoms[Random.Range(0, transfoms.Length)];
                Vector3 pos = t.localToWorldMatrix.MultiplyPoint(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
                
                weldingPunctures.Add(PhotonNetwork.Instantiate(weldingPuncturePrefab.name, pos, t.rotation).GetComponent<WeldingPunctures>());
                weldingPunctures[^1].place = t.name;
                weldingPunctures[^1].OnDestroyEvent.AddListener((weldingPuncture) =>
                {
                    weldingPunctures.Remove(weldingPuncture);
                    t.gameObject.SetActive(true);
                });
                t.gameObject.SetActive(false);
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
