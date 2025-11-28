using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class ShipMaintenance : MonoBehaviourPunCallbacks, IInteractable
{
    [SerializeField] private Button exitButton;
    [SerializeField] Button[] buttons;
    Image[] images;
    
    [SerializeField] bool _isCanInteracting = true;
    
    [SerializeField] private int time = 30;
    [SerializeField] private TMPro.TMP_Text timerText;
    public DateTime endTime = DateTime.Now;
    private float duration = 3;
    
    Vector2 frequency = new Vector2(2f, 1f);
    
    private HashSet<int> indicated =  new HashSet<int>();
    
    public bool IsCanInteracting
    {
        get { return _isCanInteracting; }
        set { photonView.RPC("SetIsCanInteracting", RpcTarget.All, value); }
    }

    [SerializeField] private ShipMaintenance secondObj;

    [PunRPC]
    void SetIsCanInteracting(bool value)
    {
        _isCanInteracting = value;
    }

    private void Start()
    {
        images = new Image[buttons.Length];
        for(int i = 0; i < buttons.Length; i++)
        {
            images[i] = buttons[i].transform.GetChild(0).GetComponent<Image>();
            int i1 = i;
            buttons[i].OnInteract.AddListener((player)=>clickbuton(i1));
        }
        exitButton.OnInteract.AddListener(Exit);
    }

    private void Update()
    {
        if (!_isCanInteracting && !secondObj.IsCanInteracting && endTime <= DateTime.Now)
        {
            StartCoroutine("Game");
            Debug.Log("endTime");
        }
    }

    IEnumerator Game()
    {
        endTime = DateTime.Now.AddSeconds(time);
        while (endTime > DateTime.Now)
        {
            float t = (float)(endTime - DateTime.Now).Seconds / time;
            yield return new WaitForSeconds(Mathf.Lerp(frequency.x, frequency.y, t));
            int index = Random.Range(0, buttons.Length);
            while(indicated.Contains(index)) index = Random.Range(0, buttons.Length);
            indicated.Add(index);
            StartCoroutine("ImageAnim", index);
        }
    }

    IEnumerator ImageAnim(int i)
    {
        float time = 0;
        while (time < duration && indicated.Contains(i))
        {
            images[i].transform.localScale = Vector3.one*(time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        images[i].transform.localScale = Vector3.zero;
        if (indicated.Contains(i))
        {
            Restart();
        }
    }

    void Restart()
    {
        Debug.Log("restart");
    }
    
    void clickbuton(int i)
    {
        if (!indicated.Remove(i))
        {
            Restart();
        }
    }

    public void StartInteraction(PlayerMovmant player)
    {
        if (IsCanInteracting)
        {
            StartCoroutine(SetCameraPosition(new Vector3(0, 0, -2), transform));
            IsCanInteracting = false;
            exitButton.gameObject.SetActive(true);
            SelfUI.instance.eventTriggerJumpButon.gameObject.SetActive(false);
            SelfUI.instance.eventTriggerRunButon.gameObject.SetActive(false);
            SelfUI.instance.joystick.gameObject.SetActive(false);
            PlayerMovmant.Instance.canRotateCamera = false;
        }
    }
    void Exit(PlayerMovmant player)
    {
        StartCoroutine(SetCameraPosition(Vector3.zero, player.transform));
        IsCanInteracting = true;
        exitButton.gameObject.SetActive(false);
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

    public void Drag(PlayerMovmant player, PointerEventData eventData)
    {
        
    }

    public void EndInteraction(PlayerMovmant player)
    {
        
    }
    
    
}
