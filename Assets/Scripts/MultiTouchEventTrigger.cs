using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouchPhase = UnityEngine.TouchPhase;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(RectTransform))]
public class MultiTouchEventTrigger : MonoBehaviour
{
    [Serializable] public class PointerEvent : UnityEngine.Events.UnityEvent<PointerEventData> { }

    public PointerEvent OnPointerDownEvent;
    public PointerEvent OnPointerUpEvent;
    public PointerEvent OnPointerEnterEvent;
    public PointerEvent OnPointerExitEvent;
    public PointerEvent OnDragEvent;
    public PointerEvent OnClickEvent;

    private RectTransform rectTransform;
    private float clickThreshold = 10f;

    // Активные пальцы на этом объекте
    private HashSet<int> activePointers = new HashSet<int>();
    private Dictionary<int, PointerEventData> pointerEventDatas = new Dictionary<int, PointerEventData>();

    // Глобальная привязка: палец -> объект
    private static Dictionary<int, MultiTouchEventTrigger> fingerToObject = new Dictionary<int, MultiTouchEventTrigger>();
    // Глобальная привязка: объект -> палец
    private static Dictionary<MultiTouchEventTrigger, int> objectToFinger = new Dictionary<MultiTouchEventTrigger, int>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            int fingerId = touch.fingerId;
            Vector2 touchPos = touch.position;
            bool isOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos, null);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Палец ещё не занят и объект свободен
                    if (isOver && !fingerToObject.ContainsKey(fingerId) && !objectToFinger.ContainsKey(this))
                    {
                        fingerToObject[fingerId] = this;
                        objectToFinger[this] = fingerId;

                        activePointers.Add(fingerId);

                        PointerEventData ped = CreatePointerEvent(fingerId, touchPos);
                        pointerEventDatas[fingerId] = ped;

                        OnPointerDownEvent?.Invoke(ped);
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // Обрабатываем только свои пальцы
                    if (fingerToObject.ContainsKey(fingerId) && fingerToObject[fingerId] == this)
                    {
                        PointerEventData ped = pointerEventDatas[fingerId];
                        ped.delta = touchPos - ped.position;
                        ped.position = touchPos;
                        OnDragEvent?.Invoke(ped);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // Обрабатываем только свои пальцы
                    if (fingerToObject.ContainsKey(fingerId) && fingerToObject[fingerId] == this)
                    {
                        PointerEventData ped = pointerEventDatas[fingerId];
                        ped.delta = touchPos - ped.position;
                        ped.position = touchPos;

                        OnPointerUpEvent?.Invoke(ped);

                        if (Vector2.Distance(ped.pressPosition, touchPos) <= clickThreshold)
                            OnClickEvent?.Invoke(ped);

                        // Освобождаем палец и объект
                        fingerToObject.Remove(fingerId);
                        objectToFinger.Remove(this);

                        activePointers.Remove(fingerId);
                        pointerEventDatas.Remove(fingerId);
                    }
                    break;
            }
        }

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        // Мышь в редакторе (тест)
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            int mouseId = -1;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (!fingerToObject.ContainsKey(mouseId) && !objectToFinger.ContainsKey(this) &&
                    RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, null))
                {
                    fingerToObject[mouseId] = this;
                    objectToFinger[this] = mouseId;

                    activePointers.Add(mouseId);
                    PointerEventData ped = CreatePointerEvent(mouseId, mousePos);
                    pointerEventDatas[mouseId] = ped;

                    OnPointerDownEvent?.Invoke(ped);
                }
            }
            else if (Mouse.current.leftButton.isPressed && fingerToObject.ContainsKey(mouseId) && fingerToObject[mouseId] == this)
            {
                PointerEventData ped = pointerEventDatas[mouseId];
                ped.delta = mousePos - ped.position;
                ped.position = mousePos;
                OnDragEvent?.Invoke(ped);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && fingerToObject.ContainsKey(mouseId) && fingerToObject[mouseId] == this)
            {
                PointerEventData ped = pointerEventDatas[mouseId];
                ped.delta = mousePos - ped.position;
                ped.position = mousePos;

                OnPointerUpEvent?.Invoke(ped);
                if (Vector2.Distance(ped.pressPosition, mousePos) <= clickThreshold)
                    OnClickEvent?.Invoke(ped);

                fingerToObject.Remove(mouseId);
                objectToFinger.Remove(this);

                activePointers.Remove(mouseId);
                pointerEventDatas.Remove(mouseId);
            }
        }
#endif
    }

    private PointerEventData CreatePointerEvent(int pointerId, Vector2 pos)
    {
        return new PointerEventData(EventSystem.current)
        {
            pointerId = pointerId,
            position = pos,
            pressPosition = pos,
            delta = Vector2.zero
        };
    }
}
