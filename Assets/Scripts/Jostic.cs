using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Jostic : MonoBehaviour
{
    [SerializeField] private RectTransform center;

    [SerializeField] private int _currentTouchID = -1;

    public Vector2 delta;
    
    public void _PointerDown(BaseEventData data)
    {
        Debug.Log("Pointer down");
        _currentTouchID = ((PointerEventData)data).pointerId;
    }

    private void Update()
    {
        if (_currentTouchID != -1)
        {
            Debug.Log("Pointer Hold 1");
            Debug.Log(Input.touches.Length);
            foreach (Touch touch in Input.touches)
            {
                Debug.Log("Pointer Hold 2");
                if (touch.fingerId == _currentTouchID)
                {
                    Debug.Log("Pointer Hold 3");
                    switch (touch.phase)
                    {
                        case TouchPhase.Ended:
                            _currentTouchID = -1;
                            break;
                    }

                    center.anchoredPosition = touch.position;
                    delta = touch.position;
                    
                    return;
                }
            }
        }
    }
}
