using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PhotonView))]
public class WeldingPunctures : MonoBehaviourPunCallbacks, IInteractable, IMiniGame
{
    bool _isCanInteracting = true;
    
    [SerializeField] private Button _exitButton;
    private List<Vector2> line = new List<Vector2>();
    [SerializeField] private List<Vector2> correctLine = new List<Vector2>();
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LineRenderer correctLineRenderer;
    [SerializeField] private float segmentLength = 0.05f;

    public UnityEvent<WeldingPunctures> OnDestroyEvent;

    private void OnDestroy()
    {
        OnDestroyEvent.Invoke(this);
    }

    private float lineCorrectLength
    {
        get
        {
            float sum = 0;
            for(int i = 0; i < correctLine.Count-1; i++)
                sum += Vector2.Distance(correctLine[i], correctLine[i+1]);
            return sum;
        }
    }private float lineLength
    {
        get
        {
            float sum = 0;
            for(int i = 0; i < line.Count-1; i++)
                sum += Vector2.Distance(line[i], line[i+1]);
            return sum;
        }
    }
    public bool needToFix = true;

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
        correctLineRenderer.positionCount = correctLine.Count;
        for (int i = 0; i < correctLine.Count; i++)
            correctLineRenderer.SetPosition(i, transform.localToWorldMatrix.MultiplyPoint((Vector3)correctLine[i]+new Vector3(0, 0, -0.51f)));
        
    }

    public void StartInteraction(PlayerMovmant player)
    {
        if(!needToFix) return;
        if (IsCanInteracting)
        {
            StartCoroutine(SetCameraPosition(new Vector3(0, 0, -200), transform));
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
        if(_exitButton.gameObject.activeInHierarchy)
        {
            if(Physics.Raycast(Camera.main.ScreenPointToRay(eventData.position), out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("ConnectingWires")))
            {
                Vector2 point = transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                if (line.Count < 1 || Vector2.Distance(point, line[^1]) > segmentLength)
                {
                    line.Add(point);
                    lineRenderer.positionCount = line.Count;
                    for (int i = 0; i < line.Count; i++)
                    {
                        lineRenderer.SetPosition(i, transform.localToWorldMatrix.MultiplyPoint((Vector3)line[i]+new Vector3(0,0,-0.51f)));
                    }
                }
            }   
        }
    }

    void IInteractable.EndInteraction(PlayerMovmant player)
    {
        if (TryToVerify())
        {
            needToFix = false;
            Destroy(this.gameObject);
            EndInteraction(player);
        }
        line.Clear();
        lineRenderer.positionCount = 0;
    }

    public bool TryToVerify()
    {
        if(lineCorrectLength*.8f > lineLength) return false;
        float sumDistance = 0;
        for (int i = 0; i < line.Count; i++)
            sumDistance += DistanceToPolyline(line[i], correctLine);
        Debug.Log( $"average deviation: {sumDistance/line.Count}");
        if(sumDistance/line.Count < 0.05f)
            return true;
        return false;
    }

    float PointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return Vector2.Distance(p, a + t * ab);
    }

    float DistanceToPolyline(Vector2 p, List<Vector2> pts)
    {
        if (pts == null || pts.Count < 2)
            return float.NaN;

        float best = float.MaxValue;

        for (int i = 0; i < pts.Count - 1; i++)
        {
            float d = PointToSegment(p, pts[i], pts[i + 1]);
            if (d < best) best = d;
        }

        return best;
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

    public void StartMiniGame()
    {
        
    }

    public string GetMassage()
    {
        if(needToFix) return "<color=#FF0000>Welding Punctures need to fix</color>\n";
        return "";
    }
}
