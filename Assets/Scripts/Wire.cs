using System;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(LineRenderer))]
public class Wire : MonoBehaviour,  IInteractable
{
    private Vector3 StartPos;
    [SerializeField] private Vector3 StartPos2;
    [SerializeField] private Transform EndPos;
    LineRenderer lineRenderer;

    private void Start()
    {
        StartPos = transform.position;
        StartPos2 = transform.localToWorldMatrix.MultiplyPoint(StartPos2);
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        lineRenderer.positionCount = 3;
        lineRenderer.SetPosition(0, StartPos2);
        lineRenderer.SetPosition(1, StartPos);
        lineRenderer.SetPosition(2, transform.position);
    }

    public void StartInteraction(PlayerMovmant player)
    {
        enabled = true;
    }

    public void Drag(PlayerMovmant player, PointerEventData eventData)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(eventData.position), out RaycastHit hit, Mathf.Infinity,
                LayerMask.GetMask("ConnectingWires")))
        {
            transform.position = hit.point;
        }
    }

    public void EndInteraction(PlayerMovmant player)
    {
        if (Vector3.Distance(EndPos.position, transform.position) < 0.1f)
        {
            transform.position = EndPos.position;
            Update();
            enabled = false;
        }
        else
        {
            transform.position = StartPos;
        }
    }
}
