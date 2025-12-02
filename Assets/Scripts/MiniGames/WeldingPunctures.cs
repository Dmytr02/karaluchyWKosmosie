using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

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
    
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private GameObject segmentPrefab2;

    public UnityEvent<WeldingPunctures> OnDestroyEvent;

    public string place = "";

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
        _exitButton.OnInteract.AddListener(End);
        Vector2 start;
        if (Random.Range(0, 2) == 0)
            start = new Vector2(-0.5f, Random.Range(-0.3f, 0.3f));
        else
            start = new Vector2(Random.Range(-0.3f, 0.3f), -0.5f);
        correctLine = Generate(start: start,
            end: -start,
            segmentLength: 0.05f,
            maxTurnAngleDeg: 25f
        );
        SpawnAlongLine(correctLine.Select(n=>transform.localToWorldMatrix.MultiplyPoint(n)+new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f))).ToList(),
            segmentPrefab2,
            0.05f, 
            () => Random.rotation, 
            () => new Vector3(0.1f, 0.1f, 0.1f));
        /*SpawnAlongLine(correctLine.Select(n=>transform.localToWorldMatrix.MultiplyPoint(n)+new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f))).ToList(),
            segmentPrefab, 
            0.5f, 
            () => transform.rotation, 
            () => new Vector3(1, 1, 1));*/
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
                        lineRenderer.SetPosition(i, transform.localToWorldMatrix.MultiplyPoint((Vector3)line[i]+new Vector3(0,0,-11)));
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
            End(player);
            Camera.main.transform.position = player.transform.position;
            Camera.main.transform.localRotation = Quaternion.identity;
            photonView.RPC("Destry", RpcTarget.MasterClient);
        }
        line.Clear();
        lineRenderer.positionCount = 0;
    }

    [PunRPC]
    private void Destry()
    {
        PhotonNetwork.Destroy(this.gameObject);
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
    
    void End(PlayerMovmant player)
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
        if(needToFix) return $"<color=#FF0000>damage to the ship's hull in the “{place}”, movement suspended  </color>\n";
        return "";
    }
    
    public static List<Vector2> Generate(
        Vector2 start,
        Vector2 end,
        float segmentLength,
        float maxTurnAngleDeg,
        int maxAttempts = 200)
    {
        float maxTurn = maxTurnAngleDeg * Mathf.Deg2Rad;
        var points = new List<Vector2> { start };

        Vector2 prevDir = (end - start).normalized;

        for (int i = 0; i < maxAttempts; i++)
        {
            bool found = false;
            for (int attempt = 0; attempt < 50; attempt++)
            {
                float angle = Random.Range(-maxTurn, maxTurn);
                Vector2 dir = Rotate(prevDir, angle);

                Vector2 candidate = points[^1] + dir * segmentLength;

                if (Mathf.Abs(candidate.x) > 0.5f || Mathf.Abs(candidate.y) > 0.5f)
                    continue;

                points.Add(candidate);
                prevDir = dir;
                found = true;
                break;
            }

            if (!found)
            {
                if (points.Count > 1)
                {
                    points.RemoveAt(points.Count - 1);
                    prevDir = (points[^1] - points[^2]).normalized;
                    continue;
                }
                else break;
            }

            if (Vector2.Distance(points[^1], end) < segmentLength)
            {
                points.Add(end);
                return points;
            }
        }

        return points;
    }

    private static Vector2 Rotate(Vector2 v, float angle)
    {
        float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
    
    public void SpawnAlongLine(
        List<Vector3> points,
        GameObject prefab,
        float spacing,
        Func<Quaternion> rotation,
        Func<Vector3> scale)
    {
        if (points == null || points.Count < 2) return;

        float distToNext = spacing;
        float accumulated = 0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];
            float segmentLength = Vector3.Distance(a, b);

            while (distToNext < segmentLength)
            {
                float t = distToNext / segmentLength;
                Vector3 pos = Vector3.Lerp(a, b, t);

                GameObject go = Instantiate(prefab, pos, rotation());
                go.transform.localScale = scale();
                OnDestroyEvent.AddListener((a)=>Destroy(go));
                    
                distToNext += spacing;
            }

            distToNext -= segmentLength;
        }
    }
}
