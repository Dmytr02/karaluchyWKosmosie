using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouchPhase = UnityEngine.TouchPhase;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(RectTransform))]
public class MultiTouchEventTrigger : MonoBehaviour, IOrdered
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

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OrderedUpdate()
    {
        if(!gameObject.activeInHierarchy) return;
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
                    if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos, null) && !fingerToObject.ContainsKey(fingerId))
                    {
                        fingerToObject[fingerId] = this;

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

                case TouchPhase.Canceled:
                case TouchPhase.Ended:
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

                        activePointers.Remove(fingerId);
                        pointerEventDatas.Remove(fingerId);
                    }
                    break;
            }
        }

#if UNITY_EDITOR
        // Мышь в редакторе (тест) — старый Input System
        if (Input.mousePresent)
        {
            Vector2 mousePos = Input.mousePosition;
            int mouseId = -1; // условный ID для мыши (чтобы структура осталась прежней)

            // Нажатие кнопки мыши
            if (Input.GetMouseButtonDown(0))
            {
                if (!fingerToObject.ContainsKey(mouseId) && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, null))
                {
                    fingerToObject[mouseId] = this;

                    activePointers.Add(mouseId);
                    PointerEventData ped = CreatePointerEvent(mouseId, mousePos);
                    pointerEventDatas[mouseId] = ped;

                    OnPointerDownEvent?.Invoke(ped);
                }
            }
            // Удержание кнопки (перемещение)
            else if (Input.GetMouseButton(0) && fingerToObject.ContainsKey(mouseId) && fingerToObject[mouseId] == this)
            {
                PointerEventData ped = pointerEventDatas[mouseId];
                ped.delta = (Vector2)mousePos - ped.position;
                ped.position = mousePos;
                OnDragEvent?.Invoke(ped);
            }
            // Отпускание кнопки
            else if (Input.GetMouseButtonUp(0) && fingerToObject.ContainsKey(mouseId) && fingerToObject[mouseId] == this)
            {
                PointerEventData ped = pointerEventDatas[mouseId];
                ped.delta = (Vector2)mousePos - ped.position;
                ped.position = mousePos;

                OnPointerUpEvent?.Invoke(ped);

                if (Vector2.Distance(ped.pressPosition, mousePos) <= clickThreshold)
                    OnClickEvent?.Invoke(ped);

                fingerToObject.Remove(mouseId);

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
